using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Codecs.Http;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Framework.Core.Utils;
using System;
using System.IO;
using System.Net;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Framework.Core.Net
{
    // 定义 NettySocketServer 类，用于创建和管理基于 DotNetty 的网络服务器
    public class NettySocketServer
    {

        // 定义主事件循环组，用于处理客户端连接
        IEventLoopGroup bossGroup = null;
        // 定义工作事件循环组，用于处理客户端请求
        IEventLoopGroup workerGroup = null;

        // 存储 WebSocket 服务器绑定的通道
        IChannel wsBoundChannel = null;
        // 存储 TCP 服务器绑定的通道
        IChannel tcpBoundChannel = null;

        // 存储 HTTP 服务器绑定的通道
        IChannel httpBoundChannel = null;


        // 获取当前类的 NLog 日志记录器
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        // 异步启动 TCP 和 WebSocket 服务器的方法
        public async System.Threading.Tasks.Task StartSterverAt(int tcpPort, int wsPort, bool IsSsl) {

            // 创建主事件循环组，使用 4 个线程
            this.bossGroup = new MultithreadEventLoopGroup(4);
            // 创建工作事件循环组，使用默认线程数
            this.workerGroup = new MultithreadEventLoopGroup();

            // 定义 WebSocket 服务器的引导程序
            ServerBootstrap wsBootstrap = null;
            // 定义 TCP 服务器的引导程序
            ServerBootstrap bootstrap = null;


            try {
                // 启动 TCP 服务器
                if (tcpPort > 0) {
                    // 记录 TCP 服务器启动信息
                    this.logger.Info("netty TcpSocket服务已启动，正在监听用户的请求@port:" + tcpPort + "......");
                    // 初始化 TCP 服务器的引导程序
                    bootstrap = new ServerBootstrap();
                    // 为引导程序设置主事件循环组和工作事件循环组
                    bootstrap.Group(bossGroup, workerGroup);

                    // 设置通道类型为 TcpServerSocketChannel
                    bootstrap.Channel<TcpServerSocketChannel>();
                    // 设置 TCP 连接的最大排队长度
                    bootstrap.Option(ChannelOption.SoBacklog, 512);

                    // 为子通道添加处理器
                    bootstrap.ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel => {
                        // 获取通道的管道
                        IChannelPipeline pipeline = channel.Pipeline;
                        // 添加长度字段前置编码器，用于在消息前添加长度字段
                        pipeline.AddLast("framing-enc", new LengthFieldPrepender(ByteOrder.LittleEndian, 2, 0, true));
                        // 添加长度字段基于的帧解码器，用于根据长度字段解析消息
                        pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ByteOrder.LittleEndian, ushort.MaxValue, 0, 2, -2, 2, true));

                        // 添加自定义的 TCP 套接字服务器处理器
                        pipeline.AddLast("IoEventHandler", new TcpSocketServerHandler());
                    }));
                    // 异步绑定 TCP 服务器到指定端口
                    this.tcpBoundChannel = await bootstrap.BindAsync(tcpPort);
                }
                // TCP 服务器启动结束

                // 启动 WebSocket 服务器
                if (wsPort > 0) {
                    // 记录 WebSocket 服务器启动信息
                    this.logger.Info("netty WebSocket 服务已启动，正在监听用户的请求@port:" + wsPort + "......");
                    // 定义用于 SSL/TLS 加密的 X509 证书
                    X509Certificate2 tlsCertificate = null;
                    // 如果需要启用 SSL/TLS 加密
                    if (IsSsl) // 注: ssl为调试，等上线加证书的时候我们来处理;
                    {
                        // 加载 SSL/TLS 证书文件
                        tlsCertificate = new X509Certificate2(Path.Combine(UtilsHelper.ProcessDirectory, "dotnetty.com.pfx"), "password");
                    }
                    // 初始化 WebSocket 服务器的引导程序
                    wsBootstrap = new ServerBootstrap();
                    // 为引导程序设置主事件循环组和工作事件循环组
                    wsBootstrap.Group(this.bossGroup, this.workerGroup);
                    // 设置通道类型为 TcpServerSocketChannel
                    wsBootstrap.Channel<TcpServerSocketChannel>();
                    // 设置 TCP 连接的最大排队长度
                    wsBootstrap.Option(ChannelOption.SoBacklog, 512);
                    // 为子通道添加处理器
                    wsBootstrap.ChildHandler(new ActionChannelInitializer<IChannel>(channel => {
                        // 获取通道的管道
                        IChannelPipeline pipeline = channel.Pipeline;
                        // 如果有 SSL/TLS 证书，添加 TLS 处理器
                        if (tlsCertificate != null) {
                            pipeline.AddLast(TlsHandler.Server(tlsCertificate));
                        }
                        // 添加 HTTP 服务器编解码器
                        pipeline.AddLast(new HttpServerCodec());
                        // 添加 HTTP 对象聚合器，用于将多个 HTTP 消息合并为一个完整的消息
                        pipeline.AddLast(new HttpObjectAggregator(65536));
                        // 添加自定义的 WebSocket 服务器处理器
                        pipeline.AddLast(new WebSocketServerHandler());
                    }));
                    // 异步绑定 WebSocket 服务器到指定端口
                    // IChannel wsBoundChannel = await wsBootstrap.BindAsync(IPAddress.Loopback, wsPort);
                    wsBoundChannel = await wsBootstrap.BindAsync(wsPort);
                }
                // WebSocket 服务器启动结束
            } finally {
                // 此处可添加资源清理或其他最终操作的代码
            }
        }

        // 异步启动 HTTP 服务器的方法
        public async Task StartHttpServer(int port, bool IsSsl = false) {
            // 如果端口号小于等于 1024，不启动 HTTP 服务器
            if (port <= 1024) {
                return;
            }

            // 记录 HTTP 服务器启动信息
            this.logger.Info("netty HttpServer 服务已启动，正在监听用户的请求@port:" + port + "......");

            // 如果不是在 Windows 操作系统上运行，设置垃圾回收的延迟模式为持续低延迟
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            }

            // 定义用于 SSL/TLS 加密的 X509 证书
            X509Certificate2 tlsCertificate = null;
            // 如果需要启用 SSL/TLS 加密
            if (IsSsl) // 注: ssl为调试，等上线加证书的时候我们来处理;
            {
                // 加载 SSL/TLS 证书文件
                tlsCertificate = new X509Certificate2(Path.Combine("", "dotnetty.com.pfx"), "password");
            }

            // 初始化 HTTP 服务器的引导程序
            var bootstrap = new ServerBootstrap();
            // 为引导程序设置主事件循环组和工作事件循环组
            bootstrap.Group(this.bossGroup, this.workerGroup);
            // 设置通道类型为 TcpServerSocketChannel
            bootstrap.Channel<TcpServerSocketChannel>();

            // 设置 TCP 连接的最大排队长度，并为子通道添加处理器
            bootstrap
                    .Option(ChannelOption.SoBacklog, 8192)
                    .ChildHandler(new ActionChannelInitializer<IChannel>(channel => {
                        // 获取通道的管道
                        IChannelPipeline pipeline = channel.Pipeline;
                        // 如果有 SSL/TLS 证书，添加 TLS 处理器
                        if (tlsCertificate != null) {
                            pipeline.AddLast(TlsHandler.Server(tlsCertificate));
                        }
                        // 添加 HTTP 响应编码器
                        pipeline.AddLast("encoder", new HttpResponseEncoder());
                        // 添加 HTTP 请求解码器
                        pipeline.AddLast("decoder", new HttpRequestDecoder(4096, 8192, 8192, false));
                        // 添加自定义的 HTTP 服务器处理器
                        pipeline.AddLast("handler", new HttpServerHandler());
                    }));


            // 异步绑定 HTTP 服务器到指定的 IPv6 地址和端口
            this.httpBoundChannel = await bootstrap.BindAsync(IPAddress.IPv6Any, port);

            // 记录 HTTP 服务器启动成功信息
            this.logger.Info($"Httpd started. Listening on {this.httpBoundChannel.LocalAddress}");
        }


        // 异步关闭所有服务器的方法
        public async Task Shutdown() {
            // 如果 WebSocket 服务器通道已绑定，关闭该通道
            if (wsBoundChannel != null) {
                await wsBoundChannel.CloseAsync();
            }

            // 如果 TCP 服务器通道已绑定，关闭该通道
            if (this.tcpBoundChannel != null) {
                await this.tcpBoundChannel.CloseAsync();
            }

            // 如果 HTTP 服务器通道已绑定，关闭该通道
            if (this.httpBoundChannel != null) {
                await this.httpBoundChannel.CloseAsync();
            }

            // 异步优雅地关闭主事件循环组和工作事件循环组
            await Task.WhenAll(
                   bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                   workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));
        }
    }
}