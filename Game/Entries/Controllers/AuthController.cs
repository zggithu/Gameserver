// ���� Framework ��ܵ�������������ռ䣬���ܰ�������Ự�����ӵȹ���
using Framework.Core.Net;
// ���� Framework ��ܵ����л������ռ䣬�ṩ���л��ͷ����л��Ĺ���
using Framework.Core.Serializer;
// ���� Framework ��ܵĹ��������ռ䣬����һЩͨ�õĹ�����ͷ���
using Framework.Core.Utils;
// ������Ϸ������Ϣ��ص������ռ䣬���������������Ӧ��Ϣ��
using Game.Datas.Messages;
// ������Ϸģ����ص������ռ䣬���������ҵ���߼�ģ��
using Game.Entries.Modules;

namespace Game.Entries.Controllers
{
    /// <summary>
    /// ��֤�������࣬����������֤��ص�����
    /// ʹ�� [Controller] ���Ա�Ǹ���Ϊ�����������ڽ��պͷַ���֤��ص�����
    /// </summary>
    [Controller]
    public class AuthController
    {
        /// <summary>
        /// NLog ��־��¼��ʵ�������ڼ�¼�ÿ����������־��Ϣ��
        /// ͨ�� NLog.LogManager.GetCurrentClassLogger() ������ȡ��ǰ�����־��¼����
        /// </summary>
        static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// �����ο͵�¼����ķ�����
        /// ʹ�� [RequestMapping] ���Ա�Ǹ÷���Ϊ����������
        /// ����һ�� IdSession �����һ�� ReqGuestLogin �������
        /// ������ί�и� AuthModule ʵ���� HandlerReqGuestLogin �������д��������ش�������
        /// </summary>
        /// <param name="s">����Ự���󣬰����ͻ��˵�������Ϣ��</param>
        /// <param name="req">�ο͵�¼������󣬰����ο͵�¼�������Ϣ��</param>
        /// <returns>�����ο͵�¼����Ľ������</returns>
        [RequestMapping]
        public object DoReqGuestLogin(IdSession s, ReqGuestLogin req) {
            return AuthModule.Instance.HandlerReqGuestLogin(s, req);
        }

        /// <summary>
        /// �����û�ע������ķ�����
        /// ʹ�� [RequestMapping] ���Ա�Ǹ÷���Ϊ����������
        /// ����һ�� IdSession �����һ�� ReqRegisterUser �������
        /// ������ί�и� AuthModule ʵ���� HandlerReqRegisterUser �������д��������ش�������
        /// </summary>
        /// <param name="s">����Ự���󣬰����ͻ��˵�������Ϣ��</param>
        /// <param name="req">�û�ע��������󣬰����û�ע���������Ϣ��</param>
        /// <returns>�����û�ע������Ľ������</returns>
        [RequestMapping]
        public object DoReqRegisterUser(IdSession s, ReqRegisterUser req) {
            return AuthModule.Instance.HandlerReqRegisterUser(s, req);
        }

        /// <summary>
        /// �����û���¼����ķ�����
        /// ʹ�� [RequestMapping] ���Ա�Ǹ÷���Ϊ����������
        /// ����һ�� IdSession �����һ�� ReqUserLogin �������
        /// ������ί�и� AuthModule ʵ���� HandlerReqUserLogin �������д��������ش�������
        /// </summary>
        /// <param name="s">����Ự���󣬰����ͻ��˵�������Ϣ��</param>
        /// <param name="req">�û���¼������󣬰����û���¼�������Ϣ��</param>
        /// <returns>�����û���¼����Ľ������</returns>
        [RequestMapping]
        public object DoReqUserLogin(IdSession s, ReqUserLogin req) {
            return AuthModule.Instance.HandlerReqUserLogin(s, req);
        }

        /// <summary>
        /// �����ο���������ķ�����
        /// ʹ�� [RequestMapping] ���Ա�Ǹ÷���Ϊ����������
        /// ����һ�� IdSession �����һ�� ReqGuestUpgrade �������
        /// ������ί�и� AuthModule ʵ���� HandlerReqGuestUpgrade �������д��������ش�������
        /// </summary>
        /// <param name="s">����Ự���󣬰����ͻ��˵�������Ϣ��</param>
        /// <param name="req">�ο�����������󣬰����ο������������Ϣ��</param>
        /// <returns>�����ο���������Ľ������</returns>
        [RequestMapping]
        public object DoReqGuestUpgrade(IdSession s, ReqGuestUpgrade req) {
            return AuthModule.Instance.HandlerReqGuestUpgrade(s, req);
        }
    }
}