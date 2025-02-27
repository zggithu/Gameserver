using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Framework.Core.Serializer;
using Framework.Core.task;
using Framework.Core.Utils;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Framework.Core.Net
{
    /// <summary>
    /// MessageDispatcher 类负责消息的分发处理，包括初始化消息处理映射、
    /// 处理客户端的进入、退出事件以及客户端消息。
    /// </summary>
    class MessageDispatcher
    {
        // 单例模式，提供全局唯一的 MessageDispatcher 实例
        public static MessageDispatcher Instance = new MessageDispatcher();

        // 使用 NLog 记录日志，获取当前类的日志记录器
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        // 存储模块命令与 CmdExecutor 的映射关系，键为模块和命令组合的字符串，值为 CmdExecutor
        /** [module_cmd, CmdExecutor] */
        private static Dictionary<string, CmdExecutor> MODULE_CMD_HANDLERS = new();

        /// <summary>
        /// 获取方法中消息参数的元数据，即模块和命令信息。
        /// </summary>
        /// <param name="method">要检查的方法信息。</param>
        /// <returns>包含模块和命令的短整型数组，如果未找到则返回 null。</returns>
        public short[] GetMessageMeta(MethodInfo method)
        {
            // 遍历方法的所有参数
            foreach (ParameterInfo parameter in method.GetParameters())
            {
                // 检查参数类型是否可赋值为 Message 类型
                if (parameter.ParameterType.IsAssignableTo(typeof(Message)))
                {
                    // 获取参数类型上的 MessageMeta 特性
                    MessageMeta msgMeta = parameter.ParameterType.GetCustomAttribute<MessageMeta>();
                    if (msgMeta != null)
                    {
                        // 提取模块和命令信息并返回
                        short[] meta = { msgMeta.module, msgMeta.cmd };
                        return meta;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 根据模块和命令构建唯一的键，用于存储和查找处理程序。
        /// </summary>
        /// <param name="module">模块编号。</param>
        /// <param name="cmd">命令编号。</param>
        /// <returns>模块和命令组合的字符串键。</returns>
        private string BuildKey(short module, short cmd)
        {
            return module + "_" + cmd;
        }

        /// <summary>
        /// 初始化消息处理映射，扫描带有 Controller 特性的类和带有 RequestMapping 特性的方法，
        /// 并将其注册到 MODULE_CMD_HANDLERS 字典中。
        /// </summary>
        public void Init()
        {  // 初始化
            // 获取所有带有 Controller 特性的类
            IEnumerable<Type> controllers = TypeScanner.ListTypesWithAttribute(typeof(Controller));
            foreach (Type controller in controllers)
            {
                try
                {
                    // 创建控制器类的实例
                    object handler = Activator.CreateInstance(controller);
                    // 获取控制器类的所有方法
                    MethodInfo[] methods = controller.GetMethods();

                    foreach (MethodInfo method in methods)
                    {
                        // 获取方法上的 RequestMapping 特性
                        RequestMapping mapperAttribute = method.GetCustomAttribute<RequestMapping>();
                        if (mapperAttribute == null)
                        {
                            // 如果方法没有该特性，则跳过
                            continue;
                        }

                        // 获取方法中消息参数的元数据
                        short[] meta = this.GetMessageMeta(method);
                        short module = meta[0];
                        short cmd = meta[1];

                        // 构建模块和命令组合的键
                        string key = BuildKey(module, cmd);
                        // 检查该键是否已经存在于映射中
                        MODULE_CMD_HANDLERS.TryGetValue(key, out CmdExecutor cmdExecutor);

                        if (cmdExecutor != null)
                        {
                            // 如果已经存在，则记录警告日志并返回
                            logger.Warn($"module[{module}] cmd[{cmd}] 重复注册处理器");
                            return;
                        }

                        // 创建 CmdExecutor 实例，封装处理方法、参数类型和处理程序实例
                        cmdExecutor = CmdExecutor.Create(method, method.GetParameters().Select(parameterInfo => parameterInfo.ParameterType).ToArray(), handler);

                        // 将键和对应的 CmdExecutor 存入映射中
                        MODULE_CMD_HANDLERS.Add(key, cmdExecutor);
                    }
                }
                catch (Exception e)
                {
                    // 捕获异常并记录错误日志
                    logger.Error(e.Message);
                }
            }
        }

        /// <summary>
        /// 处理有新客户端进入的事件，记录客户端的远程地址。
        /// </summary>
        /// <param name="s">客户端会话信息。</param>
        public void OnClientEnter(IdSession s)
        {
            this.logger.Debug($"On Client Enter: {s.GetRemoteAddress()}");
        }
         
        /// <summary>
        /// 处理客户端离开的事件，记录客户端的远程地址。
        /// </summary>
        /// <param name="s">客户端会话信息。</param>
        public void OnClientExit(IdSession s)
        {
            this.logger.Debug($"On Client Exit: {s.GetRemoteAddress()}");
        }

        /// <summary>
        /// 将客户端会话、消息等信息转换为处理方法所需的参数数组。
        /// </summary>
        /// <param name="session">客户端会话信息。</param>
        /// <param name="methodParams">处理方法的参数类型数组。</param>
        /// <param name="message">接收到的消息。</param>
        /// <returns>处理方法所需的参数数组。</returns>
        private object[] ConvertToMethodParams(IdSession session, Type[] methodParams, Message message)
        {
            // 创建一个与方法参数数量相同的对象数组
            object[] result = new object[methodParams == null ? 0 : methodParams.Length];

            // 遍历参数数组
            for (int i = 0; i < result.Length; i++)
            {
                Type param = methodParams[i];
                if (param.IsAssignableTo(typeof(IdSession)))
                {
                    // 如果参数类型是 IdSession，则将客户端会话信息赋值给该参数
                    result[i] = session;
                }
                else if (param.IsAssignableTo(typeof(long)))
                {
                    // 如果参数类型是 long，则将客户端账号 ID 赋值给该参数
                    result[i] = session.accountId;
                }
                else if (param.IsAssignableTo(typeof(Message)))
                {
                    // 如果参数类型是 Message，则将接收到的消息赋值给该参数
                    result[i] = message;
                }
            }

            return result;
        }

        /// <summary>
        /// 处理接收到的客户端消息，根据模块和命令查找对应的处理程序，
        /// 并将消息处理任务添加到任务池中。
        /// </summary>
        /// <param name="s">客户端会话信息。</param>
        /// <param name="data">接收到的字节数据。</param>
        /// <param name="offset">数据的起始偏移量。</param>
        /// <param name="count">数据的长度。</param>
        public void OnClientMsg(IdSession s, byte[] data, int offset, int count)
        {
            // 记录客户端接收到命令的日志（注释部分，可根据需要启用）
            // this.logger.Debug($"On Client Recv Cmd: {s.GetRemoteAddress()}");

            // 从字节数据中读取模块编号（使用小端字节序）
            short module = UtilsHelper.ReadShortLE(data, offset + 0);
            // 从字节数据中读取命令编号（使用小端字节序）
            short cmd = UtilsHelper.ReadShortLE(data, offset + 2);

            // 从字节数据中读取用户标签（使用小端字节序），网关可能会用到，暂时保留
            uint utag = UtilsHelper.ReadUintLE(data, offset + 4);

            // 使用序列化工具对消息进行解码
            Message msg = SerializerHelper.PbDecode(module, cmd, data, offset + 8, count - 8);

            // 构建模块和命令组合的键
            string key = BuildKey(module, cmd);
            // 从映射中查找对应的 CmdExecutor
            MODULE_CMD_HANDLERS.TryGetValue(key, out CmdExecutor cmdExecutor);
            if (cmdExecutor != null)
            {
                // 转换为处理方法所需的参数数组
                object[] @params = ConvertToMethodParams(s, cmdExecutor.@params, msg);
                // 将消息处理任务添加到任务工作池
                TaskWorkerPool.Instance.AddTask(MessageTask.Create(s.distributeKey, cmdExecutor.handler, cmdExecutor.method, @params, s));
                return;
            }
            else
            {  // 查看一下是否为Logic服务事件,然后直接投递到逻辑服线程处理
                // 如果未找到处理程序，则将消息推送到逻辑服务器线程处理
                LogicWorkerPool.Instance.PushMsgToLogicServer(s, module, cmd, msg);
            }

        }
    }
}