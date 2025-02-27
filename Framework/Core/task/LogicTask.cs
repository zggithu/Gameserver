using Framework.Core.Net;
using Framework.Core.Serializer;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Framework.Core.task
{
    /// <summary>
    /// 逻辑事件类，用于封装与逻辑事件相关的信息。
    /// 这些信息将在逻辑处理流程中传递和使用。
    /// </summary>
    public class LogicEvent
    {
        // 表示与该逻辑事件关联的会话对象，用于标识客户端会话
        public IdSession s;
        // 逻辑事件所属的模块号，用于区分不同的业务模块
        public short module;
        // 逻辑事件的命令号，用于标识具体的操作或指令
        public short cmd;
        // 逻辑事件携带的消息内容，可以是各种类型的对象
        public object msg;
    }

    /// <summary>
    /// 逻辑任务类，继承自 AbstractDistributeTask，用于处理逻辑任务。
    /// 管理逻辑事件队列，执行逻辑任务相关的操作。
    /// </summary>
    public class LogicTask : AbstractDistributeTask
    {
        // 使用 NLog 记录日志，获取当前类的日志记录器
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        // 逻辑任务的处理对象，通常是一个包含逻辑处理方法的类实例
        private object handler;
        // 处理逻辑任务的目标方法，通过反射调用该方法执行具体逻辑
        private MethodInfo method;
        // 服务类型，用于标识该逻辑任务所属的服务类型
        private int stype;
        // 存储逻辑事件的队列，用于异步处理逻辑事件
        public Queue<LogicEvent> eventQueue;

        /// <summary>
        /// 向逻辑事件队列中添加一个新的逻辑事件。
        /// </summary>
        /// <param name="s">与事件关联的会话对象。</param>
        /// <param name="module">事件所属的模块号。</param>
        /// <param name="cmd">事件的命令号。</param>
        /// <param name="msg">事件携带的消息内容。</param>
        public void PushEvent(IdSession s, short module, short cmd, object msg)
        {
            // 创建一个新的逻辑事件对象
            LogicEvent e = new LogicEvent();
            // 设置事件的模块号
            e.module = module;
            // 设置事件的命令号
            e.cmd = cmd;
            // 设置事件携带的消息内容
            e.msg = msg;
            // 设置与事件关联的会话对象
            e.s = s;

            // 锁定事件队列，确保线程安全，避免多个线程同时修改队列
            lock (this.eventQueue)
            {
                // 将新的逻辑事件添加到队列末尾
                this.eventQueue.Enqueue(e);
            }
        }

        /// <summary>
        /// 创建一个新的 LogicTask 实例，并进行初始化操作。
        /// </summary>
        /// <param name="distributeKey">分布式任务的分发键，用于任务的分发和调度。</param>
        /// <param name="stype">服务类型。</param>
        /// <param name="handler">逻辑任务的处理对象。</param>
        /// <param name="method">处理逻辑任务的目标方法。</param>
        /// <returns>新创建并初始化的 LogicTask 实例。</returns>
        public static LogicTask Create(long distributeKey, int stype, object handler, MethodInfo method)
        {
            // 创建一个新的 LogicTask 实例
            LogicTask t = new LogicTask();
            // 设置分布式任务的分发键
            t.distributeKey = distributeKey;
            // 设置服务类型
            t.stype = stype;
            // 设置逻辑任务的处理对象
            t.handler = handler;
            // 设置处理逻辑任务的目标方法
            t.method = method;
            // 初始化逻辑事件队列
            t.eventQueue = new Queue<LogicEvent>();

            // 将处理对象转换为 BaseLogicServer 类型
            BaseLogicServer server = (BaseLogicServer)handler;
            // 调用 BaseLogicServer 的 Init 方法进行初始化，传入事件队列
            server.Init(t.eventQueue);

            // 返回创建并初始化好的 LogicTask 实例
            return t;
        }

        /// <summary>
        /// 重写 AbstractDistributeTask 的 DoAction 方法，执行逻辑任务。
        /// </summary>
        public override void DoAction()
        {
            try
            {
                // 使用反射调用处理对象的目标方法，执行逻辑任务
                method.Invoke(handler, null);
            }
            catch (Exception e)
            {
                // 若执行过程中出现异常，记录警告日志，包含异常信息
                logger.Warn("message task execute failed" + e.Message);
            }
        }
    }
}