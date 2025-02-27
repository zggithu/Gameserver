using DotNetty.Buffers;
using DotNetty.Codecs.Http.WebSockets;
using DotNetty.Transport.Channels;
using System.Net;

namespace Framework.Core.Net
{
    /// <summary>
    /// IdSession �����ڱ�ʾһ���ͻ��˻Ự���洢����ͻ���������ص���Ϣ��
    /// ���ṩ�˷������ݺͻ�ȡ�ͻ��˵�ַ��Ϣ�ķ�����
    /// </summary>
    public class IdSession
    {
        // �ͻ���ͨ����������ͻ��˽���ͨ��
        public IChannel client = null;

        // �̳߳طַ�����������ÿ�� IdSession ����ӦΨһ�ı��
        // �����ڽ��ûỰ���䵽�ض����̳߳ؽ��д���
        public long distributeKey = 0;

        // ��Ҷ�Ӧ����Ϸ ID ��
        public long playerId = 0;

        // ��Ҷ�Ӧ���˺� ID
        public long accountId = 0;

        // ��� ID ��ְҵ����Ӧ�� key
        public long accountIdAndJob = 0;

        // ��ǰ�� session �� WebSocket ���� TcpSocket
        public bool isWebSocket = false;

        /// <summary>
        /// ��ȡ�ͻ��˵� IP ��ַ��
        /// </summary>
        /// <returns>�ͻ��˵� IP ��ַ�ַ�����</returns>
        public string GetIp()
        {
            // ��ȡ�ͻ���Զ�̵�ַ�� IP ���֣���ȥ�� IPv6 ǰ׺
            return ((IPEndPoint)this.client.RemoteAddress).Address.ToString().Substring(7);
        }

        /// <summary>
        /// ��ȡ�ͻ��˵�Զ�̵�ַ������ IP ��ַ�Ͷ˿ںš�
        /// </summary>
        /// <returns>�ͻ��˵�Զ�̵�ַ�ַ�����</returns>
        public string GetRemoteAddress()
        {
            // ���ؿͻ��˵�Զ�̵�ַ�ַ���
            return (this.client.RemoteAddress.ToString());
        }

        /// <summary>
        /// ��ͻ��˷������ݡ�
        /// ���� isWebSocket ��־������ʹ�� WebSocket ���� TcpSocket �������ݡ�
        /// </summary>
        /// <param name="data">Ҫ���͵��ֽ��������ݡ�</param>
        public void Send(byte[] data)
        {
            // ����һ���µ��ֽڻ���������������д�뻺����
            IByteBuffer buffer = Unpooled.Buffer();
            buffer.WriteBytes(data);

            // �����ǰ�Ự�� WebSocket ����
            if (this.isWebSocket)
            {
                // ����һ�������� WebSocket ֡������Ҫ���͵�����
                BinaryWebSocketFrame f = new BinaryWebSocketFrame(buffer);
                // �첽�ؽ������� WebSocket ֡д�벢ˢ�µ��ͻ���ͨ��
                this.client.WriteAndFlushAsync(f);
                /*
                 * ���´���ע�Ͳ���չʾ����һ���� eventLoop �߳���ִ��д������ķ�ʽ
                 * ���Ը��ݾ�����������Ƿ�ʹ��
                 * this.client.EventLoop.Execute(()=> {
                 *     this.client.WriteAsync(f);
                 * });
                 */
            }
            else
            {
                // ����� TcpSocket ���ӣ�ֱ�ӽ��ֽڻ�����д�벢ˢ�µ��ͻ���ͨ��
                this.client.WriteAndFlushAsync(buffer);
            }
        }
    }
}