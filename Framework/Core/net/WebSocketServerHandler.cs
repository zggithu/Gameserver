namespace Framework.Core.Net
{
    // 引入必要的命名空间
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Threading.Tasks;
    using DotNetty.Buffers;
    using DotNetty.Codecs.Http;
    using DotNetty.Codecs.Http.WebSockets;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;

    // 使用静态导入，方便使用 HttpVersion 和 HttpResponseStatus 中的常量
    using static DotNetty.Codecs.Http.HttpVersion;
    using static DotNetty.Codecs.Http.HttpResponseStatus;
    using System.IO;

    // 定义一个密封类 WebSocketServerHandler，继承自 SimpleChannelInboundHandler<object>
    // 用于处理 WebSocket 服务器接收到的各种对象类型的消息
    public sealed class WebSocketServerHandler : SimpleChannelInboundHandler<object>
    {
        // 定义 WebSocket 连接的路径常量
        const string WebsocketPath = "/websocket";

        // 用于处理 WebSocket 握手过程的对象
        WebSocketServerHandshaker handshaker;

        // 使用 NLog 记录日志，获取当前类的日志记录器
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        // 重写 ChannelRead0 方法，该方法会在接收到消息时被调用
        protected override void ChannelRead0(IChannelHandlerContext ctx, object msg)
        {
            // 如果接收到的消息是 HTTP 请求
            if (msg is IFullHttpRequest request)
            {
                // 调用 HandleHttpRequest 方法处理该 HTTP 请求
                this.HandleHttpRequest(ctx, request);
            }
            // 如果接收到的消息是 WebSocket 帧
            else if (msg is WebSocketFrame frame)
            {
                // 调用 HandleWebSocketFrame 方法处理该 WebSocket 帧
                this.HandleWebSocketFrame(ctx, frame);
            }
        }

        // 重写 ChannelReadComplete 方法，当读取操作完成时被调用
        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            // 刷新通道，确保数据被发送
            context.Flush();
        }

        // 处理 HTTP 请求的方法
        private void HandleHttpRequest(IChannelHandlerContext ctx, IFullHttpRequest req)
        {
            // 检查请求结果是否成功，如果不成功
            if (!req.Result.IsSuccess)
            {
                // 发送一个 400 Bad Request 的响应
                SendHttpResponse(ctx, req, new DefaultFullHttpResponse(Http11, BadRequest));
                return;
            }

            // 检查请求方法是否为 GET，如果不是
            if (!Equals(req.Method, HttpMethod.Get))
            {
                // 发送一个 403 Forbidden 的响应
                SendHttpResponse(ctx, req, new DefaultFullHttpResponse(Http11, Forbidden));
                return;
            }

            // 如果请求的 URI 是根路径 "/"
            if ("/".Equals(req.Uri))
            {
                // 获取 WebSocket 性能测试页面的内容
                IByteBuffer content = WebSocketServerBenchmarkPage.GetContent(GetWebSocketLocation(req));
                // 创建一个 HTTP 响应，状态码为 200 OK，并包含页面内容
                var res = new DefaultFullHttpResponse(Http11, OK, content);
                // 设置响应的 Content-Type 为 text/html，并指定字符集为 UTF-8
                res.Headers.Set(HttpHeaderNames.ContentType, "text/html; charset=UTF-8");
                // 设置响应的 Content-Length
                HttpUtil.SetContentLength(res, content.ReadableBytes);
                // 发送该 HTTP 响应
                SendHttpResponse(ctx, req, res);
                return;
            }

            // 如果请求的 URI 是 "/favicon.ico"
            if ("/favicon.ico".Equals(req.Uri))
            {
                // 发送一个 404 Not Found 的响应
                var res = new DefaultFullHttpResponse(Http11, NotFound);
                SendHttpResponse(ctx, req, res);
                return;
            }

            // 创建 WebSocket 握手工厂，设置允许的最大内容长度为 5MB
            var wsFactory = new WebSocketServerHandshakerFactory(
                GetWebSocketLocation(req), null, true, 5 * 1024 * 1024);
            // 根据请求创建 WebSocket 握手器
            this.handshaker = wsFactory.NewHandshaker(req);
            // 如果握手器创建失败
            if (this.handshaker == null)
            {
                // 发送不支持的 WebSocket 版本响应
                WebSocketServerHandshakerFactory.SendUnsupportedVersionResponse(ctx.Channel);
            }
            else
            {
                // 执行 WebSocket 握手操作
                this.handshaker.HandshakeAsync(ctx.Channel, req);
            }

            // 创建一个新的会话对象，将当前通道关联到该会话
            IdSession s = SessionMgr.Instance.CreateIdSession(ctx.Channel, true);
            // 通知消息分发器有新的客户端进入
            MessageDispatcher.Instance.OnClientEnter(s);
        }

        // 处理 WebSocket 帧的方法
        void HandleWebSocketFrame(IChannelHandlerContext ctx, WebSocketFrame frame)
        {
            // 如果接收到的是关闭帧
            if (frame is CloseWebSocketFrame)
            {
                // 根据通道获取对应的会话对象
                IdSession s = SessionMgr.Instance.GetSessionBy(ctx.Channel);
                // 通知消息分发器该客户端已退出
                MessageDispatcher.Instance.OnClientExit(s);
                // 保留当前关闭帧并关闭 WebSocket 连接
                this.handshaker.CloseAsync(ctx.Channel, (CloseWebSocketFrame)frame.Retain());
                return;
            }

            // 如果接收到的是 Ping 帧
            if (frame is PingWebSocketFrame)
            {
                // 保留 Ping 帧的内容，创建并发送一个 Pong 帧作为响应
                ctx.WriteAsync(new PongWebSocketFrame((IByteBuffer)frame.Content.Retain()));
                return;
            }

            // 如果接收到的是文本帧
            if (frame is TextWebSocketFrame)
            {
                // 记录警告日志，提示使用 arraybuffer 模式
                string warningMsg = "Server WebSocket: Use arraybuffer Model";
                this.logger.Warn(warningMsg);
                // 创建一个文本帧，包含警告消息并发送
                TextWebSocketFrame f = new TextWebSocketFrame(warningMsg);
                ctx.WriteAsync(f);
                return;
            }

            // 如果接收到的是二进制帧
            if (frame is BinaryWebSocketFrame)
            {
                // 根据通道获取对应的会话对象
                IdSession s = SessionMgr.Instance.GetSessionBy(ctx.Channel);
                // 获取二进制帧的内容
                IByteBuffer msg = frame.Content;
                // 将二进制数据传递给消息分发器进行处理
                MessageDispatcher.Instance.OnClientMsg(s, msg.Array, msg.ArrayOffset, msg.ReadableBytes);
            }
        }

        // 发送 HTTP 响应的静态方法
        static void SendHttpResponse(IChannelHandlerContext ctx, IFullHttpRequest req, IFullHttpResponse res)
        {
            // 如果响应状态码不是 200 OK
            if (res.Status.Code != 200)
            {
                // 将状态码信息转换为字节数组并复制到响应内容中
                IByteBuffer buf = Unpooled.CopiedBuffer(Encoding.UTF8.GetBytes(res.Status.ToString()));
                res.Content.WriteBytes(buf);
                // 释放字节缓冲区
                buf.Release();
                // 设置响应的 Content-Length
                HttpUtil.SetContentLength(res, res.Content.ReadableBytes);
            }

            // 异步发送响应
            Task task = ctx.Channel.WriteAndFlushAsync(res);
            // 如果请求不是保持连接的，或者响应状态码不是 200 OK
            if (!HttpUtil.IsKeepAlive(req) || res.Status.Code != 200)
            {
                // 当发送任务完成后，关闭通道
                task.ContinueWith((t, c) => ((IChannelHandlerContext)c).CloseAsync(),
                    ctx, TaskContinuationOptions.ExecuteSynchronously);
            }
        }

        // 处理异常的方法，当通道处理过程中出现异常时被调用
        public override void ExceptionCaught(IChannelHandlerContext context, Exception e)
        {
            // 获取当前通道
            IChannel channel = context.Channel;
            // 如果通道处于活动或打开状态
            if (channel.Active || channel.Open)
            {
                // 关闭通道
                context.CloseAsync();
            }

            // 如果异常不是 IOException
            if (!(e is IOException))
            {
                // 记录调试日志，包含远程地址和异常消息
                this.logger.Debug("remote:" + channel.RemoteAddress, e.Message);
            }
        }

        // 获取 WebSocket 连接地址的静态方法
        static string GetWebSocketLocation(IFullHttpRequest req, bool IsSsl = false)
        {
            // 尝试从请求头中获取 Host 信息
            bool result = req.Headers.TryGet(HttpHeaderNames.Host, out ICharSequence value);
            // 拼接 WebSocket 路径
            string location = value.ToString() + WebsocketPath;

            // 如果是 SSL 连接
            if (IsSsl)
            {
                return "wss://" + location;
            }
            else
            {
                return "ws://" + location;
            }
        }
    }
}