using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using System.Net;
using System.Threading;

namespace Framework.Core.Net
{
    /// <summary>
    /// SessionMgr ����һ���Ự�����������õ���ģʽ��������� IdSession ����
    /// ���ṩ�˴�������ȡ�ͳ�ʼ���Ự�Ĺ��ܣ�ȷ��ÿ���ͻ������Ӷ���Ψһ�ĻỰ��ʶ��
    /// </summary>
    public class SessionMgr
    {
        // ����ʵ����ȷ������Ӧ�ó�����ֻ��һ�� SessionMgr ʵ��
        public static SessionMgr Instance = new SessionMgr();

        // AttributeKey ������ IChannel �ϴ洢�ͻ�ȡ IdSession ����
        // ÿ�� IChannel ���Թ���һ�� IdSession��ͨ�����������ʶ
        private AttributeKey<IdSession> SESSION_KEY = AttributeKey<IdSession>.ValueOf("session");

        // ��������Ψһ�ķֲ�ʽ����distributeKey��
        // ʹ�� Interlocked �ౣ֤�ڶ��̻߳����¸ñ�����ԭ���Ե���
        private long autoId = 0;

        /// <summary>
        /// ��ʼ���Ự���������� autoId ����Ϊ 0��
        /// ��ĳЩ����£�������Ҫ���³�ʼ���Ự�����������ô˷������� autoId��
        /// </summary>
        public void Init()
        {
            this.autoId = 0;
        }

        /// <summary>
        /// ����һ���µ� IdSession ���󣬲����������ָ���� IChannel �ϡ�
        /// </summary>
        /// <param name="channel">���»Ự������ IChannel��</param>
        /// <param name="isWebSocket">ָʾ�ûỰ�Ƿ�Ϊ WebSocket �Ự��Ĭ��Ϊ false��</param>
        /// <returns>�´����� IdSession ����</returns>
        public IdSession CreateIdSession(IChannel channel, bool isWebSocket = false)
        {
            // ����һ���µ� IdSession ʵ��
            IdSession session = new IdSession();
            // ��ʼ���˺� ID Ϊ 0
            session.accountId = 0;
            // ��ʼ����� ID Ϊ 0
            session.playerId = 0;

            // ʹ�� Interlocked ��ԭ���Եص��� autoId�������������ֵ���� distributeKey
            // ȷ��ÿ���Ự��Ψһ�ķֲ�ʽ��
            long id = Interlocked.Increment(ref autoId);
            session.distributeKey = id;

            // ������� IChannel ��ֵ�� session �� client ����
            session.client = channel;
            // ���ݴ���Ĳ��������Ƿ�Ϊ WebSocket �Ự
            session.isWebSocket = isWebSocket;

            // ͨ�� SESSION_KEY ��ȡ IChannel �ϵ�����
            IAttribute<IdSession> sessionAttr = channel.GetAttribute(SESSION_KEY);
            // ʹ�� CompareAndSet �������´����� IdSession ����洢�� IChannel ��
            // ����������Ѿ����ڣ��򲻽������ã�ȷ��ÿ�� IChannel ֻ����һ�� IdSession
            sessionAttr.CompareAndSet(null, session);

            // �����´����� IdSession ����
            return session;
        }

        /// <summary>
        /// ����ָ���� IChannel ��ȡ������ IdSession ����
        /// </summary>
        /// <param name="channel">���ڲ��ҹ��� IdSession �� IChannel��</param>
        /// <returns>������ IdSession ��������������򷵻� null��</returns>
        public IdSession GetSessionBy(IChannel channel)
        {
            // ͨ�� SESSION_KEY ��ȡ IChannel �ϵ�����
            IAttribute<IdSession> sessionAttr = channel.GetAttribute(SESSION_KEY);
            // ���ش洢�ڸ������е� IdSession ��������������򷵻� null
            return sessionAttr.Get();
        }
    }
}