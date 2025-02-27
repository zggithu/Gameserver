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
    // ���� NettySocketServer �࣬���ڴ����͹������ DotNetty �����������
    public class NettySocketServer
    {

        // �������¼�ѭ���飬���ڴ���ͻ�������
        IEventLoopGroup bossGroup = null;
        // ���幤���¼�ѭ���飬���ڴ���ͻ�������
        IEventLoopGroup workerGroup = null;

        // �洢 WebSocket �������󶨵�ͨ��
        IChannel wsBoundChannel = null;
        // �洢 TCP �������󶨵�ͨ��
        IChannel tcpBoundChannel = null;

        // �洢 HTTP �������󶨵�ͨ��
        IChannel httpBoundChannel = null;


        // ��ȡ��ǰ��� NLog ��־��¼��
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        // �첽���� TCP �� WebSocket �������ķ���
        public async System.Threading.Tasks.Task StartSterverAt(int tcpPort, int wsPort, bool IsSsl) {

            // �������¼�ѭ���飬ʹ�� 4 ���߳�
            this.bossGroup = new MultithreadEventLoopGroup(4);
            // ���������¼�ѭ���飬ʹ��Ĭ���߳���
            this.workerGroup = new MultithreadEventLoopGroup();

            // ���� WebSocket ����������������
            ServerBootstrap wsBootstrap = null;
            // ���� TCP ����������������
            ServerBootstrap bootstrap = null;


            try {
                // ���� TCP ������
                if (tcpPort > 0) {
                    // ��¼ TCP ������������Ϣ
                    this.logger.Info("netty TcpSocket���������������ڼ����û�������@port:" + tcpPort + "......");
                    // ��ʼ�� TCP ����������������
                    bootstrap = new ServerBootstrap();
                    // Ϊ���������������¼�ѭ����͹����¼�ѭ����
                    bootstrap.Group(bossGroup, workerGroup);

                    // ����ͨ������Ϊ TcpServerSocketChannel
                    bootstrap.Channel<TcpServerSocketChannel>();
                    // ���� TCP ���ӵ�����Ŷӳ���
                    bootstrap.Option(ChannelOption.SoBacklog, 512);

                    // Ϊ��ͨ����Ӵ�����
                    bootstrap.ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel => {
                        // ��ȡͨ���Ĺܵ�
                        IChannelPipeline pipeline = channel.Pipeline;
                        // ��ӳ����ֶ�ǰ�ñ���������������Ϣǰ��ӳ����ֶ�
                        pipeline.AddLast("framing-enc", new LengthFieldPrepender(ByteOrder.LittleEndian, 2, 0, true));
                        // ��ӳ����ֶλ��ڵ�֡�����������ڸ��ݳ����ֶν�����Ϣ
                        pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ByteOrder.LittleEndian, ushort.MaxValue, 0, 2, -2, 2, true));

                        // ����Զ���� TCP �׽��ַ�����������
                        pipeline.AddLast("IoEventHandler", new TcpSocketServerHandler());
                    }));
                    // �첽�� TCP ��������ָ���˿�
                    this.tcpBoundChannel = await bootstrap.BindAsync(tcpPort);
                }
                // TCP ��������������

                // ���� WebSocket ������
                if (wsPort > 0) {
                    // ��¼ WebSocket ������������Ϣ
                    this.logger.Info("netty WebSocket ���������������ڼ����û�������@port:" + wsPort + "......");
                    // �������� SSL/TLS ���ܵ� X509 ֤��
                    X509Certificate2 tlsCertificate = null;
                    // �����Ҫ���� SSL/TLS ����
                    if (IsSsl) // ע: sslΪ���ԣ������߼�֤���ʱ������������;
                    {
                        // ���� SSL/TLS ֤���ļ�
                        tlsCertificate = new X509Certificate2(Path.Combine(UtilsHelper.ProcessDirectory, "dotnetty.com.pfx"), "password");
                    }
                    // ��ʼ�� WebSocket ����������������
                    wsBootstrap = new ServerBootstrap();
                    // Ϊ���������������¼�ѭ����͹����¼�ѭ����
                    wsBootstrap.Group(this.bossGroup, this.workerGroup);
                    // ����ͨ������Ϊ TcpServerSocketChannel
                    wsBootstrap.Channel<TcpServerSocketChannel>();
                    // ���� TCP ���ӵ�����Ŷӳ���
                    wsBootstrap.Option(ChannelOption.SoBacklog, 512);
                    // Ϊ��ͨ����Ӵ�����
                    wsBootstrap.ChildHandler(new ActionChannelInitializer<IChannel>(channel => {
                        // ��ȡͨ���Ĺܵ�
                        IChannelPipeline pipeline = channel.Pipeline;
                        // ����� SSL/TLS ֤�飬��� TLS ������
                        if (tlsCertificate != null) {
                            pipeline.AddLast(TlsHandler.Server(tlsCertificate));
                        }
                        // ��� HTTP �������������
                        pipeline.AddLast(new HttpServerCodec());
                        // ��� HTTP ����ۺ��������ڽ���� HTTP ��Ϣ�ϲ�Ϊһ����������Ϣ
                        pipeline.AddLast(new HttpObjectAggregator(65536));
                        // ����Զ���� WebSocket ������������
                        pipeline.AddLast(new WebSocketServerHandler());
                    }));
                    // �첽�� WebSocket ��������ָ���˿�
                    // IChannel wsBoundChannel = await wsBootstrap.BindAsync(IPAddress.Loopback, wsPort);
                    wsBoundChannel = await wsBootstrap.BindAsync(wsPort);
                }
                // WebSocket ��������������
            } finally {
                // �˴��������Դ������������ղ����Ĵ���
            }
        }

        // �첽���� HTTP �������ķ���
        public async Task StartHttpServer(int port, bool IsSsl = false) {
            // ����˿ں�С�ڵ��� 1024�������� HTTP ������
            if (port <= 1024) {
                return;
            }

            // ��¼ HTTP ������������Ϣ
            this.logger.Info("netty HttpServer ���������������ڼ����û�������@port:" + port + "......");

            // ��������� Windows ����ϵͳ�����У������������յ��ӳ�ģʽΪ�������ӳ�
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            }

            // �������� SSL/TLS ���ܵ� X509 ֤��
            X509Certificate2 tlsCertificate = null;
            // �����Ҫ���� SSL/TLS ����
            if (IsSsl) // ע: sslΪ���ԣ������߼�֤���ʱ������������;
            {
                // ���� SSL/TLS ֤���ļ�
                tlsCertificate = new X509Certificate2(Path.Combine("", "dotnetty.com.pfx"), "password");
            }

            // ��ʼ�� HTTP ����������������
            var bootstrap = new ServerBootstrap();
            // Ϊ���������������¼�ѭ����͹����¼�ѭ����
            bootstrap.Group(this.bossGroup, this.workerGroup);
            // ����ͨ������Ϊ TcpServerSocketChannel
            bootstrap.Channel<TcpServerSocketChannel>();

            // ���� TCP ���ӵ�����Ŷӳ��ȣ���Ϊ��ͨ����Ӵ�����
            bootstrap
                    .Option(ChannelOption.SoBacklog, 8192)
                    .ChildHandler(new ActionChannelInitializer<IChannel>(channel => {
                        // ��ȡͨ���Ĺܵ�
                        IChannelPipeline pipeline = channel.Pipeline;
                        // ����� SSL/TLS ֤�飬��� TLS ������
                        if (tlsCertificate != null) {
                            pipeline.AddLast(TlsHandler.Server(tlsCertificate));
                        }
                        // ��� HTTP ��Ӧ������
                        pipeline.AddLast("encoder", new HttpResponseEncoder());
                        // ��� HTTP ���������
                        pipeline.AddLast("decoder", new HttpRequestDecoder(4096, 8192, 8192, false));
                        // ����Զ���� HTTP ������������
                        pipeline.AddLast("handler", new HttpServerHandler());
                    }));


            // �첽�� HTTP ��������ָ���� IPv6 ��ַ�Ͷ˿�
            this.httpBoundChannel = await bootstrap.BindAsync(IPAddress.IPv6Any, port);

            // ��¼ HTTP �����������ɹ���Ϣ
            this.logger.Info($"Httpd started. Listening on {this.httpBoundChannel.LocalAddress}");
        }


        // �첽�ر����з������ķ���
        public async Task Shutdown() {
            // ��� WebSocket ������ͨ���Ѱ󶨣��رո�ͨ��
            if (wsBoundChannel != null) {
                await wsBoundChannel.CloseAsync();
            }

            // ��� TCP ������ͨ���Ѱ󶨣��رո�ͨ��
            if (this.tcpBoundChannel != null) {
                await this.tcpBoundChannel.CloseAsync();
            }

            // ��� HTTP ������ͨ���Ѱ󶨣��رո�ͨ��
            if (this.httpBoundChannel != null) {
                await this.httpBoundChannel.CloseAsync();
            }

            // �첽���ŵعر����¼�ѭ����͹����¼�ѭ����
            await Task.WhenAll(
                   bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                   workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));
        }
    }
}