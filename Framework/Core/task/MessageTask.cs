using Framework.Core.Net;
using Framework.Core.Serializer;
using System;
using System.Reflection;
using System.Text;

namespace Framework.Core.task
{
    /// <summary>
    /// 消息任务类，继承自 AbstractDistributeTask，用于处理消息相关的任务。
    /// 封装了消息处理所需的上下文信息，包括会话、处理程序、目标方法和参数。
    /// </summary>
    public class MessageTask : AbstractDistributeTask
    {
        // 表示消息处理所关联的会话对象，用于标识客户端会话
        private IdSession session;
        // 消息的处理程序，通常是一个包含消息处理逻辑的类实例
        private object handler;
        // 处理消息的目标方法，通过反射调用该方法执行具体的消息处理逻辑
        private MethodInfo method;
        // 传递给目标方法的参数数组
        private object[] @params;

        // 使用 NLog 记录日志，获取当前类的日志记录器
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 创建一个新的 MessageTask 实例。
        /// </summary>
        /// <param name="distributeKey">分布式任务的分发键，用于任务的分发和调度。</param>
        /// <param name="handler">消息的处理程序。</param>
        /// <param name="method">处理消息的目标方法。</param>
        /// <param name="@params">传递给目标方法的参数数组。</param>
        /// <param name="s">消息处理所关联的会话对象。</param>
        /// <returns>新创建的 MessageTask 实例。</returns>
        public static MessageTask Create(long distributeKey, object handler, MethodInfo method, object[] @params, IdSession s)
        {
            // 创建一个新的 MessageTask 实例
            MessageTask t = new MessageTask();
            // 设置分布式任务的分发键
            t.distributeKey = distributeKey;
            // 设置消息的处理程序
            t.handler = handler;
            // 设置处理消息的目标方法
            t.method = method;
            // 设置传递给目标方法的参数数组
            t.@params = @params;
            // 设置消息处理所关联的会话对象
            t.session = s;

            return t;
        }

        /// <summary>
        /// 重写 AbstractDistributeTask 的 DoAction 方法，执行消息处理任务。
        /// </summary>
        public override void DoAction()
        {
            try
            {
                // 使用反射调用处理程序的目标方法，并传递参数数组，获取返回结果
                object response = method.Invoke(handler, @params);
                if (response != null)
                {
                    // 如果目标方法有返回值，将返回的消息通过消息推送器推送给关联的会话
                    MessagePusher.PushMessage(this.session, (Message)response);
                }
            }
            catch (Exception e)
            {
                // 若执行过程中出现异常，记录警告日志，包含异常信息
                logger.Warn("message task execute failed" + e.Message);
            }
        }
    }
}