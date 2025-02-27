using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using System;
using System.IO;
using System.Text;

namespace Framework.Core.Net
{
    /// <summary>
    /// TcpSocketServerHandler 类用于处理 TCP 套接字服务器的各种事件，
    /// 包括客户端连接建立、数据读取、连接关闭和异常处理等。
    /// 它继承自 ChannelHandlerAdapter，使用 SessionMgr 管理会话，
    /// 并将相关事件转发给 MessageDispatcher 进行处理。
    /// </summary>
    public class TcpSocketServerHandler : ChannelHandlerAdapter
    {
        // 使用 NLog 进行日志记录，用于记录服务器处理过程中的各种信息
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 构造函数，调用基类的构造函数进行初始化。
        /// </summary>
        public TcpSocketServerHandler() : base()
        {
        }

        /// <summary>
        /// 当有新的客户端连接建立时，此方法会被调用。
        /// </summary>
        /// <param name="context">通道处理上下文，包含通道的相关信息和操作方法。</param>
        public override void ChannelActive(IChannelHandlerContext context) // 连接进来的
        {
            // 获取当前连接的 IChannel 对象，代表客户端与服务器之间的通道
            IChannel channel = context.Channel;

            // 通过 SessionMgr 的单例实例创建一个新的 IdSession 对象，并将其与当前通道关联
            IdSession s = SessionMgr.Instance.CreateIdSession(channel);

            // 调用 MessageDispatcher 的单例实例的 OnClientEnter 方法，通知有新客户端进入
            MessageDispatcher.Instance.OnClientEnter(s);
        }

        /// <summary>
        /// 当从客户端接收到数据时，此方法会被调用。
        /// </summary>
        /// <param name="context">通道处理上下文。</param>
        /// <param name="message">接收到的消息对象。</param>
        public override void ChannelRead(IChannelHandlerContext context, object message) // 有数据读的时候 
        {
            // 将接收到的消息转换为 IByteBuffer 类型，用于表示字节缓冲区
            var msg = message as IByteBuffer;
            // 获取当前连接的 IChannel 对象
            IChannel channel = context.Channel;
            // 通过 SessionMgr 根据当前通道获取关联的 IdSession 对象
            IdSession s = SessionMgr.Instance.GetSessionBy(channel);

            // 调用 MessageDispatcher 的单例实例的 OnClientMsg 方法，将接收到的数据传递给消息分发器进行处理
            MessageDispatcher.Instance.OnClientMsg(s, msg.Array, msg.ArrayOffset, msg.ReadableBytes);
        }

        /// <summary>
        /// 当数据读取完成时，此方法会被调用。
        /// 调用 context.Flush() 方法确保所有待发送的数据都被发送出去。
        /// </summary>
        /// <param name="context">通道处理上下文。</param>
        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush(); // 数据读取完的一个处理

        /// <summary>
        /// 当客户端连接关闭时，此方法会被调用。
        /// </summary>
        /// <param name="context">通道处理上下文。</param>
        public override void ChannelInactive(IChannelHandlerContext context) // 关闭
        {
            // 获取当前连接的 IChannel 对象
            IChannel channel = context.Channel;
            // 通过 SessionMgr 根据当前通道获取关联的 IdSession 对象
            IdSession s = SessionMgr.Instance.GetSessionBy(channel);
            // 调用 MessageDispatcher 的单例实例的 OnClientExit 方法，通知客户端已离开
            MessageDispatcher.Instance.OnClientExit(s);
            // 异步断开与客户端的连接
            channel.DisconnectAsync();
        }

        /// <summary>
        /// 当处理过程中出现异常时，此方法会被调用。
        /// </summary>
        /// <param name="context">通道处理上下文。</param>
        /// <param name="e">捕获到的异常对象。</param>
        public override void ExceptionCaught(IChannelHandlerContext context, Exception e) // 有异常的时候
        {
            // 获取当前连接的 IChannel 对象
            IChannel channel = context.Channel;
            // 如果通道处于活动或打开状态，异步关闭通道
            if (channel.Active || channel.Open)
            {
                context.CloseAsync();
            }

            // 如果异常不是 IOException 类型，记录远程地址和异常消息，方便排查问题
            if (!(e is IOException))
            {
                this.logger.Debug("remote:" + channel.RemoteAddress, e.Message);
            }
        }
    }
}