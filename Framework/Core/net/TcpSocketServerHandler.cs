using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using System;
using System.IO;
using System.Text;

namespace Framework.Core.Net
{
    /// <summary>
    /// TcpSocketServerHandler �����ڴ��� TCP �׽��ַ������ĸ����¼���
    /// �����ͻ������ӽ��������ݶ�ȡ�����ӹرպ��쳣����ȡ�
    /// ���̳��� ChannelHandlerAdapter��ʹ�� SessionMgr ����Ự��
    /// ��������¼�ת���� MessageDispatcher ���д���
    /// </summary>
    public class TcpSocketServerHandler : ChannelHandlerAdapter
    {
        // ʹ�� NLog ������־��¼�����ڼ�¼��������������еĸ�����Ϣ
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// ���캯�������û���Ĺ��캯�����г�ʼ����
        /// </summary>
        public TcpSocketServerHandler() : base()
        {
        }

        /// <summary>
        /// �����µĿͻ������ӽ���ʱ���˷����ᱻ���á�
        /// </summary>
        /// <param name="context">ͨ�����������ģ�����ͨ���������Ϣ�Ͳ���������</param>
        public override void ChannelActive(IChannelHandlerContext context) // ���ӽ�����
        {
            // ��ȡ��ǰ���ӵ� IChannel ���󣬴���ͻ����������֮���ͨ��
            IChannel channel = context.Channel;

            // ͨ�� SessionMgr �ĵ���ʵ������һ���µ� IdSession ���󣬲������뵱ǰͨ������
            IdSession s = SessionMgr.Instance.CreateIdSession(channel);

            // ���� MessageDispatcher �ĵ���ʵ���� OnClientEnter ������֪ͨ���¿ͻ��˽���
            MessageDispatcher.Instance.OnClientEnter(s);
        }

        /// <summary>
        /// ���ӿͻ��˽��յ�����ʱ���˷����ᱻ���á�
        /// </summary>
        /// <param name="context">ͨ�����������ġ�</param>
        /// <param name="message">���յ�����Ϣ����</param>
        public override void ChannelRead(IChannelHandlerContext context, object message) // �����ݶ���ʱ�� 
        {
            // �����յ�����Ϣת��Ϊ IByteBuffer ���ͣ����ڱ�ʾ�ֽڻ�����
            var msg = message as IByteBuffer;
            // ��ȡ��ǰ���ӵ� IChannel ����
            IChannel channel = context.Channel;
            // ͨ�� SessionMgr ���ݵ�ǰͨ����ȡ������ IdSession ����
            IdSession s = SessionMgr.Instance.GetSessionBy(channel);

            // ���� MessageDispatcher �ĵ���ʵ���� OnClientMsg �����������յ������ݴ��ݸ���Ϣ�ַ������д���
            MessageDispatcher.Instance.OnClientMsg(s, msg.Array, msg.ArrayOffset, msg.ReadableBytes);
        }

        /// <summary>
        /// �����ݶ�ȡ���ʱ���˷����ᱻ���á�
        /// ���� context.Flush() ����ȷ�����д����͵����ݶ������ͳ�ȥ��
        /// </summary>
        /// <param name="context">ͨ�����������ġ�</param>
        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush(); // ���ݶ�ȡ���һ������

        /// <summary>
        /// ���ͻ������ӹر�ʱ���˷����ᱻ���á�
        /// </summary>
        /// <param name="context">ͨ�����������ġ�</param>
        public override void ChannelInactive(IChannelHandlerContext context) // �ر�
        {
            // ��ȡ��ǰ���ӵ� IChannel ����
            IChannel channel = context.Channel;
            // ͨ�� SessionMgr ���ݵ�ǰͨ����ȡ������ IdSession ����
            IdSession s = SessionMgr.Instance.GetSessionBy(channel);
            // ���� MessageDispatcher �ĵ���ʵ���� OnClientExit ������֪ͨ�ͻ������뿪
            MessageDispatcher.Instance.OnClientExit(s);
            // �첽�Ͽ���ͻ��˵�����
            channel.DisconnectAsync();
        }

        /// <summary>
        /// ����������г����쳣ʱ���˷����ᱻ���á�
        /// </summary>
        /// <param name="context">ͨ�����������ġ�</param>
        /// <param name="e">���񵽵��쳣����</param>
        public override void ExceptionCaught(IChannelHandlerContext context, Exception e) // ���쳣��ʱ��
        {
            // ��ȡ��ǰ���ӵ� IChannel ����
            IChannel channel = context.Channel;
            // ���ͨ�����ڻ���״̬���첽�ر�ͨ��
            if (channel.Active || channel.Open)
            {
                context.CloseAsync();
            }

            // ����쳣���� IOException ���ͣ���¼Զ�̵�ַ���쳣��Ϣ�������Ų�����
            if (!(e is IOException))
            {
                this.logger.Debug("remote:" + channel.RemoteAddress, e.Message);
            }
        }
    }
}