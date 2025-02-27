using Framework.Core.Utils;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Core.Serializer
{
    /// <summary>
    /// 消息工厂类，采用单例模式，负责管理消息类型的注册和查找。
    /// 在初始化时扫描所有 Message 类的子类，将模块和命令信息与消息类型进行映射，
    /// 之后可以根据模块和命令信息查找对应的消息类型。
    /// </summary>
    class MessageFactory
    {
        // 单例实例，确保整个应用程序中只有一个 MessageFactory 实例
        public static MessageFactory Instance = new MessageFactory();

        // 字典，用于存储键（由模块和命令生成）和对应的消息类型
        private Dictionary<int, Type> keyTypeDic = new();
        // private Dictionary<Type, int> TypeKeyDic = new();

        // NLog 日志记录器，用于记录日志信息
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 初始化消息池，扫描所有 Message 类的子类，为每个子类提取 MessageMeta 特性，
        /// 生成唯一的键，并将键与对应的消息类型存储在字典中。
        /// </summary>
        public void InitMeesagePool()
        {
            // 获取所有 Message 类的子类
            List<Type> messages = TypeScanner.ListAllSubTypes(typeof(Message)).ToList();

            // 遍历所有子类
            foreach (Type message in messages)
            {
                // 获取子类的 MessageMeta 特性
                MessageMeta meta = message.GetCustomAttribute<MessageMeta>();
                // 如果特性不存在，抛出异常
                if (meta == null)
                {
                    throw new RuntimeBinderException($"[致命错误]:没有找到[{message.Name}]的MessageMeta");
                }

                // 根据模块和命令信息生成唯一的键
                int key = BuildKey(meta.module, meta.cmd);
                // 检查键是否已经存在于字典中
                if (keyTypeDic.ContainsKey(key))
                {
                    throw new RuntimeBinderException($"[致命错误]:[{key}]重复注册");
                }

                // 将键和对应的消息类型添加到字典中
                keyTypeDic.Add(key, message);
                // TypeKeyDic.Add(message, key);
            }
        }

        /// <summary>
        /// 根据模块和命令信息生成唯一的键。
        /// </summary>
        /// <param name="module">模块编号</param>
        /// <param name="cmd">命令编号</param>
        /// <returns>生成的键</returns>
        public int BuildKey(short module, short cmd)
        {
            return module * (10000) + cmd;
        }

        /// <summary>
        /// 根据模块和命令信息查找对应的消息类型。
        /// </summary>
        /// <param name="module">模块编号</param>
        /// <param name="cmd">命令编号</param>
        /// <param name="msgType">输出参数，用于返回找到的消息类型</param>
        /// <returns>如果找到对应的消息类型返回 true，否则返回 false</returns>
        public bool GetMessage(short module, short cmd, out Type msgType)
        {
            return keyTypeDic.TryGetValue(BuildKey(module, cmd), out msgType);
        }

    }
}