using Framework.Core.Net;
using Framework.Core.Serializer;
using System;
using System.Reflection;
using System.Text;

namespace Framework.Core.task
{
    /// <summary>
    /// ��Ϣ�����࣬�̳��� AbstractDistributeTask�����ڴ�����Ϣ��ص�����
    /// ��װ����Ϣ�����������������Ϣ�������Ự���������Ŀ�귽���Ͳ�����
    /// </summary>
    public class MessageTask : AbstractDistributeTask
    {
        // ��ʾ��Ϣ�����������ĻỰ�������ڱ�ʶ�ͻ��˻Ự
        private IdSession session;
        // ��Ϣ�Ĵ������ͨ����һ��������Ϣ�����߼�����ʵ��
        private object handler;
        // ������Ϣ��Ŀ�귽����ͨ��������ø÷���ִ�о������Ϣ�����߼�
        private MethodInfo method;
        // ���ݸ�Ŀ�귽���Ĳ�������
        private object[] @params;

        // ʹ�� NLog ��¼��־����ȡ��ǰ�����־��¼��
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// ����һ���µ� MessageTask ʵ����
        /// </summary>
        /// <param name="distributeKey">�ֲ�ʽ����ķַ�������������ķַ��͵��ȡ�</param>
        /// <param name="handler">��Ϣ�Ĵ������</param>
        /// <param name="method">������Ϣ��Ŀ�귽����</param>
        /// <param name="@params">���ݸ�Ŀ�귽���Ĳ������顣</param>
        /// <param name="s">��Ϣ�����������ĻỰ����</param>
        /// <returns>�´����� MessageTask ʵ����</returns>
        public static MessageTask Create(long distributeKey, object handler, MethodInfo method, object[] @params, IdSession s)
        {
            // ����һ���µ� MessageTask ʵ��
            MessageTask t = new MessageTask();
            // ���÷ֲ�ʽ����ķַ���
            t.distributeKey = distributeKey;
            // ������Ϣ�Ĵ������
            t.handler = handler;
            // ���ô�����Ϣ��Ŀ�귽��
            t.method = method;
            // ���ô��ݸ�Ŀ�귽���Ĳ�������
            t.@params = @params;
            // ������Ϣ�����������ĻỰ����
            t.session = s;

            return t;
        }

        /// <summary>
        /// ��д AbstractDistributeTask �� DoAction ������ִ����Ϣ��������
        /// </summary>
        public override void DoAction()
        {
            try
            {
                // ʹ�÷�����ô�������Ŀ�귽���������ݲ������飬��ȡ���ؽ��
                object response = method.Invoke(handler, @params);
                if (response != null)
                {
                    // ���Ŀ�귽���з���ֵ�������ص���Ϣͨ����Ϣ���������͸������ĻỰ
                    MessagePusher.PushMessage(this.session, (Message)response);
                }
            }
            catch (Exception e)
            {
                // ��ִ�й����г����쳣����¼������־�������쳣��Ϣ
                logger.Warn("message task execute failed" + e.Message);
            }
        }
    }
}