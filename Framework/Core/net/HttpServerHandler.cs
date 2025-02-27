namespace Framework.Core.Net
{
    // 引入处理 HTTP 编解码相关的命名空间
    using DotNetty.Codecs.Http;
    // 引入 DotNetty 通用工具类命名空间
    using DotNetty.Common.Utilities;
    // 引入 DotNetty 通道处理相关的命名空间
    using DotNetty.Transport.Channels;
    using System;

    /// <summary>
    /// 密封类 HttpServerHandler，继承自 ChannelHandlerAdapter，
    /// 用于处理 Netty 通道接收到的 HTTP 请求。密封类意味着该类不能被继承。
    /// </summary>
    sealed class HttpServerHandler : ChannelHandlerAdapter
    {
        // 使用 NLog 记录日志，获取当前类的日志记录器
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 重写 ChannelRead 方法，当通道接收到新消息时会调用此方法。
        /// </summary>
        /// <param name="ctx">通道处理上下文，包含通道的相关信息和操作方法。</param>
        /// <param name="message">接收到的消息对象。</param>
        public override void ChannelRead(IChannelHandlerContext ctx, object message)
        {
            // 判断接收到的消息是否为 HTTP 请求
            if (message is IHttpRequest request)
            {
                try
                {
                    // 调用 Process 方法处理 HTTP 请求
                    this.Process(ctx, request);
                }
                finally
                {
                    // 释放消息对象的引用计数，避免内存泄漏
                    ReferenceCountUtil.Release(message);
                }
            }
            else
            {
                // 如果消息不是 HTTP 请求，将消息传递给下一个通道处理器
                ctx.FireChannelRead(message);
            }
        }

        /// <summary>
        /// 处理 HTTP 请求的方法。
        /// </summary>
        /// <param name="ctx">通道处理上下文。</param>
        /// <param name="request">HTTP 请求对象。</param>
        void Process(IChannelHandlerContext ctx, IHttpRequest request)
        {
            // 调用 HttpRouteDispatcher 的单例实例的 OnHttpRequrestProcess 方法来处理请求
            HttpRouteDispatcher.Instance.OnHttpRequrestProcess(ctx, request);
        }

        /// <summary>
        /// 重写 ExceptionCaught 方法，当通道处理过程中出现异常时会调用此方法。
        /// </summary>
        /// <param name="context">通道处理上下文。</param>
        /// <param name="exception">捕获到的异常对象。</param>
        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            // 记录异常日志（原代码未记录，可根据需求添加）
            logger.Error(exception, "通道处理过程中出现异常");
            // 关闭通道，释放相关资源
            context.CloseAsync();
        }

        /// <summary>
        /// 重写 ChannelReadComplete 方法，当通道读取完成时会调用此方法。
        /// </summary>
        /// <param name="context">通道处理上下文。</param>
        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            // 刷新通道，确保所有待发送的数据都被发送出去
            context.Flush();
        }
    }
}