// �����ܵ�������������ռ䣬�ṩ����Ự�ȹ���֧��
using Framework.Core.Net;
// �����ܵ����л������ռ䣬����ʵ�����ݵ����л��ͷ����л�
using Framework.Core.Serializer;
// �����ܵĹ��������ռ䣬����һЩͨ�ù�����ͷ���
using Framework.Core.Utils;
// ������Ϸ������Ϣ�������ռ䣬�������������ص�������Ϣ��
using Game.Datas.Messages;
// ������Ϸģ��������ռ䣬�������崦��ҵ���߼���ģ��
using Game.Entries.Modules;

namespace Game.Entries.Controllers
{
    /// <summary>
    /// ��ҿ������࣬�������������صĸ�������
    /// ͨ�� [Controller] ���Ա��Ϊ�������࣬���������ת���� PlayerModule ����
    /// </summary>
    [Controller]
    public class PlayerController
    {
        /// <summary>
        /// NLog ��־��¼��ʵ�������ڼ�¼�ÿ����������־��Ϣ��
        /// ͨ�� NLog.LogManager.GetCurrentClassLogger() ������ȡ��ǰ�����־��¼����
        /// </summary>
        static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// ������ȡ�����������ķ�����
        /// ʹ�� [RequestMapping] ���Ա�Ǹ÷���Ϊ����������
        /// ����һ�� IdSession �����һ�� ReqPullingPlayerData �������
        /// ������ί�и� PlayerModule ʵ���� HandlerReqPullingPlayerData �������д��������ش�������
        /// </summary>
        /// <param name="s">����Ự���󣬰����ͻ��˵�������Ϣ��</param>
        /// <param name="req">��ȡ������ݵ�������󣬰�����ȡ�����������Ϣ��</param>
        /// <returns>������ȡ�����������Ľ������</returns>
        [RequestMapping]
        public object DoReqPullingPlayerData(IdSession s, ReqPullingPlayerData req) {
            return PlayerModule.Instance.HandlerReqPullingPlayerData(s, req);
        }

        /// <summary>
        /// ����ѡ���������ķ�����
        /// ʹ�� [RequestMapping] ���Ա�Ǹ÷���Ϊ����������
        /// ����һ�� IdSession �����һ�� ReqSelectPlayer �������
        /// ������ί�и� PlayerModule ʵ���� HandlerReqSelectPlayer �������д��������ش�������
        /// </summary>
        /// <param name="s">����Ự���󣬰����ͻ��˵�������Ϣ��</param>
        /// <param name="req">ѡ����ҵ�������󣬰���ѡ������������Ϣ��</param>
        /// <returns>����ѡ���������Ľ������</returns>
        [RequestMapping]
        public object DoReqSelectPlayer(IdSession s, ReqSelectPlayer req) {
            return PlayerModule.Instance.HandlerReqSelectPlayer(s, req);
        }

        /// <summary>
        /// ������ȡ��¼��������ķ�����
        /// ʹ�� [RequestMapping] ���Ա�Ǹ÷���Ϊ����������
        /// ����һ�� IdSession �����һ�� ReqRecvLoginBonues �������
        /// ������ί�и� PlayerModule ʵ���� HandlerRecvLoginBonues �������д��������ش�������
        /// </summary>
        /// <param name="s">����Ự���󣬰����ͻ��˵�������Ϣ��</param>
        /// <param name="req">��ȡ��¼������������󣬰�����ȡ�����������Ϣ��</param>
        /// <returns>������ȡ��¼��������Ľ������</returns>
        [RequestMapping]
        public object DoReqRecvLoginBonues(IdSession s, ReqRecvLoginBonues req) {
            return PlayerModule.Instance.HandlerRecvLoginBonues(s, req);
        }

        /// <summary>
        /// ������ȡ�����б�����ķ�����
        /// ʹ�� [RequestMapping] ���Ա�Ǹ÷���Ϊ����������
        /// ����һ�� IdSession �����һ�� ReqPullingBonuesList �������
        /// ������ί�и� PlayerModule ʵ���� HandlerReqPullingBonuesList �������д��������ش�������
        /// </summary>
        /// <param name="s">����Ự���󣬰����ͻ��˵�������Ϣ��</param>
        /// <param name="req">��ȡ�����б��������󣬰�����ȡ�����б��������Ϣ��</param>
        /// <returns>������ȡ�����б�����Ľ������</returns>
        [RequestMapping]
        public object DoReqPullingBonuesList(IdSession s, ReqPullingBonuesList req) {
            return PlayerModule.Instance.HandlerReqPullingBonuesList(s, req);
        }

        /// <summary>
        /// ������ȡ��������ķ�����
        /// ʹ�� [RequestMapping] ���Ա�Ǹ÷���Ϊ����������
        /// ����һ�� IdSession �����һ�� ReqRecvBonues �������
        /// ������ί�и� PlayerModule ʵ���� HandlerReqRecvBonues �������д��������ش�������
        /// </summary>
        /// <param name="s">����Ự���󣬰����ͻ��˵�������Ϣ��</param>
        /// <param name="req">��ȡ������������󣬰�����ȡ�����������Ϣ��</param>
        /// <returns>������ȡ��������Ľ������</returns>
        [RequestMapping]
        public object DoReqRecvBonues(IdSession s, ReqRecvBonues req) {
            return PlayerModule.Instance.HandlerReqRecvBonues(s, req);
        }

        /// <summary>
        /// ������ȡ�����б�����ķ�����
        /// ʹ�� [RequestMapping] ���Ա�Ǹ÷���Ϊ����������
        /// ����һ�� IdSession �����һ�� ReqPullingTaskList �������
        /// ������ί�и� PlayerModule ʵ���� HandlerReqPullingTaskList �������д��������ش�������
        /// </summary>
        /// <param name="s">����Ự���󣬰����ͻ��˵�������Ϣ��</param>
        /// <param name="req">��ȡ�����б��������󣬰�����ȡ�����б��������Ϣ��</param>
        /// <returns>������ȡ�����б�����Ľ������</returns>
        [RequestMapping]
        public object DoReqPullingTaskList(IdSession s, ReqPullingTaskList req) {
            return PlayerModule.Instance.HandlerReqPullingTaskList(s, req);
        }

        /// <summary>
        /// ���������ȡ������Ʒ����ķ�����
        /// ʹ�� [RequestMapping] ���Ա�Ǹ÷���Ϊ����������
        /// ����һ�� IdSession �����һ�� ReqTestGetGoods �������
        /// ������ί�и� PlayerModule ʵ���� HandlerReqTestGetGoods �������д��������ش�������
        /// </summary>
        /// <param name="s">����Ự���󣬰����ͻ��˵�������Ϣ��</param>
        /// <param name="req">������ȡ������Ʒ��������󣬰�����ȡ��Ʒ�������Ϣ��</param>
        /// <returns>���������ȡ������Ʒ����Ľ������</returns>
        [RequestMapping]
        public object DoReqTestGetBonuesGoods(IdSession s, ReqTestGetGoods req) {
            return PlayerModule.Instance.HandlerReqTestGetGoods(s, req);
        }

        /// <summary>
        /// ������Ը�����Ʒ����ķ�����
        /// ע�⣺�����д���ƴд���󣬲������� ReqTestUpdateGooods ����ӦΪ ReqTestUpdateGoods��
        /// ���õĴ����� HandlerReqTestUpdatesGoods ����ӦΪ HandlerReqTestUpdateGoods��
        /// ʹ�� [RequestMapping] ���Ա�Ǹ÷���Ϊ����������
        /// ����һ�� IdSession �����һ�� ReqTestUpdateGooods �������
        /// ������ί�и� PlayerModule ʵ���� HandlerReqTestUpdatesGoods �������д��������ش�������
        /// </summary>
        /// <param name="s">����Ự���󣬰����ͻ��˵�������Ϣ��</param>
        /// <param name="req">���Ը�����Ʒ��������󣬰���������Ʒ�������Ϣ��</param>
        /// <returns>������Ը�����Ʒ����Ľ������</returns>
        [RequestMapping]
        public object DoReqTestUpdateGoods(IdSession s, ReqTestUpdateGooods req) {
            return PlayerModule.Instance.HandlerReqTestUpdatesGoods(s, req);
        }

        /// <summary>
        /// ������ȡ�ʼ���Ϣ����ķ�����
        /// ʹ�� [RequestMapping] ���Ա�Ǹ÷���Ϊ����������
        /// ����һ�� IdSession �����һ�� ReqPullingMailMsg �������
        /// ������ί�и� PlayerModule ʵ���� HandlerReqPullingMailMsg �������д��������ش�������
        /// </summary>
        /// <param name="s">����Ự���󣬰����ͻ��˵�������Ϣ��</param>
        /// <param name="req">��ȡ�ʼ���Ϣ��������󣬰�����ȡ�ʼ���Ϣ�������Ϣ��</param>
        /// <returns>������ȡ�ʼ���Ϣ����Ľ������</returns>
        [RequestMapping]
        public object DoReqPullingMailMsg(IdSession s, ReqPullingMailMsg req) {
            return PlayerModule.Instance.HandlerReqPullingMailMsg(s, req);
        }

        /// <summary>
        /// ��������ʼ���Ϣ����ķ�����
        /// ʹ�� [RequestMapping] ���Ա�Ǹ÷���Ϊ����������
        /// ����һ�� IdSession �����һ�� ReqUpdateMailMsg �������
        /// ������ί�и� PlayerModule ʵ���� HandlerReqUpdateMailMsg �������д��������ش�������
        /// </summary>
        /// <param name="s">����Ự���󣬰����ͻ��˵�������Ϣ��</param>
        /// <param name="req">�����ʼ���Ϣ��������󣬰��������ʼ���Ϣ�������Ϣ��</param>
        /// <returns>��������ʼ���Ϣ����Ľ������</returns>
        [RequestMapping]
        public object DoReqUpdateMailMsg(IdSession s, ReqUpdateMailMsg req) {
            return PlayerModule.Instance.HandlerReqUpdateMailMsg(s, req);
        }

        /// <summary>
        /// ������ȡ���а���������ķ�����
        /// ʹ�� [RequestMapping] ���Ա�Ǹ÷���Ϊ����������
        /// ����һ�� IdSession �����һ�� ReqPullingRank �������
        /// ������ί�и� PlayerModule ʵ���� HandlerReqPullingRank �������д��������ش�������
        /// </summary>
        /// <param name="s">����Ự���󣬰����ͻ��˵�������Ϣ��</param>
        /// <param name="req">��ȡ���а����ݵ�������󣬰�����ȡ���а������������Ϣ��</param>
        /// <returns>������ȡ���а���������Ľ������</returns>
        [RequestMapping]
        public object DoReqPullingRank(IdSession s, ReqPullingRank req) {
            return PlayerModule.Instance.HandlerReqPullingRank(s, req);
        }

        /// <summary>
        /// ������ȡ������������ķ�����
        /// ʹ�� [RequestMapping] ���Ա�Ǹ÷���Ϊ����������
        /// ����һ�� IdSession �����һ�� ReqPullingPackData �������
        /// ������ί�и� PlayerModule ʵ���� HandlerReqPullingPackData �������д��������ش�������
        /// </summary>
        /// <param name="s">����Ự���󣬰����ͻ��˵�������Ϣ��</param>
        /// <param name="req">��ȡ�������ݵ�������󣬰�����ȡ���������������Ϣ��</param>
        /// <returns>������ȡ������������Ľ������</returns>
        [RequestMapping]
        public object DoReqPullingPackData(IdSession s, ReqPullingPackData req) {
            return PlayerModule.Instance.HandlerReqPullingPackData(s, req);
        }

        /// <summary>
        /// �����Ʒ��������ķ�����
        /// ʹ�� [RequestMapping] ���Ա�Ǹ÷���Ϊ����������
        /// ����һ�� IdSession �����һ�� ReqExchangeProduct �������
        /// ������ί�и� PlayerModule ʵ���� HandlerReqExchangeProduct �������д��������ش�������
        /// </summary>
        /// <param name="s">����Ự���󣬰����ͻ��˵�������Ϣ��</param>
        /// <param name="req">��Ʒ������������󣬰�����Ʒ�����������Ϣ��</param>
        /// <returns>�����Ʒ��������Ľ������</returns>
        [RequestMapping]
        public object DoReqExchangeProduct(IdSession s, ReqExchangeProduct req) {
            return PlayerModule.Instance.HandlerReqExchangeProduct(s, req);
        }
    }
}