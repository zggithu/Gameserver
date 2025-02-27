using Framework.Core.Net;
using Framework.Core.Utils;
using Game.Datas.DBEntities;
using Game.Datas.Messages;
using Game.Core.GM_Bonues;
using Game.Core.Caches;
using Game.Core.EntityMgr;
using Game.Datas.GMEntities;
using Game.Core.GM_Task;
using System.Collections.Generic;
using Game.Core.GM_MailMessage;
using Game.Core.GM_Rank;
using Game.Core.GM_Backpack;
using Game.Core.GM_Trading;

namespace Game.Entries.Modules
{
    /// <summary>
    /// ���ģ���࣬�����������صĸ���ҵ���߼������õ���ģʽ��
    /// </summary>
    public class PlayerModule
    {
        /// <summary>
        /// ����ʵ����ȫ��Ψһ��
        /// </summary>
        public static PlayerModule Instance = new PlayerModule();

        /// <summary>
        /// NLog ��־��¼�������ڼ�¼ģ�����й����е���Ϣ��
        /// </summary>
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// ��ʼ��ģ�飬��Ҫ��ʼ����صĻ��档
        /// </summary>
        public void Init() {
            PlayerIDCache.Instance.Init();
            PlayerAcountIDCache.Instance.Init();
            PlayerIdLoginBonuesCache.Instance.Init();
        }

        /// <summary>
        /// �����ݿ��е������Ϣ���Ƶ���Ӧ���ݶ����С�
        /// </summary>
        /// <param name="p">���ݿ��е���Ҷ���</param>
        /// <param name="pInfo">Ҫ���������Ϣ��Ӧ����</param>
        private void CopyDbPlayerToResponesData(Game.Datas.DBEntities.Player p, PlayerInfo pInfo) {
            pInfo.exp = p.exp;
            pInfo.hp = p.HP;
            pInfo.mp = p.MP;
            pInfo.umoney = p.umoney;
            pInfo.unick = p.name;
            pInfo.ucion = p.ucoin;
            pInfo.usex = p.usex;
        }

        /// <summary>
        /// ������ȡ��¼����������
        /// ��֤��ҵ�¼״̬����齱���Ƿ����ȡ��������ҽ�Һͽ���״̬��
        /// </summary>
        /// <param name="s">�Ự���󣬰�����ҵ�¼��Ϣ��</param>
        /// <param name="req">��ȡ��¼�������������</param>
        /// <returns>��ȡ��¼��������Ӧ����</returns>
        public ResRecvLoginBonues HandlerRecvLoginBonues(IdSession s, ReqRecvLoginBonues req) {
            ResRecvLoginBonues res = new ResRecvLoginBonues();

            // ��֤����˺��Ƿ��ѵ�¼
            if (s.accountId <= 0 || s.playerId <= 0) {
                res.status = (int)Respones.AccountIsNotLogin;
                return res;
            }

            // �ӻ����л�ȡ��¼��������
            Loginbonues data = PlayerIdLoginBonuesCache.Instance.Get(s.playerId);
            if (data == null) {
                res.status = (int)Respones.SystemErr;
                return res;
            }

            // ��齱���Ƿ����ȡ
            if (data.days == 0 || data.status != 0) {
                res.status = (int)Respones.InvalidOpt;
                return res;
            }

            res.status = (int)Respones.OK;
            res.num = data.bonues;

            // ������ҽ������
            Game.Datas.DBEntities.Player p = PlayerAcountIDCache.Instance.Get(s.accountIdAndJob);
            if (p != null) {
                p.ucoin += data.bonues;
            }

            p = PlayerIDCache.Instance.Get((s.playerId));
            if (p != null) {
                p.ucoin += data.bonues;
            }

            if (p != null) {
                PlayerIDCache.Instance.UpdateDataToDb(p);
            }

            // ���½���״̬Ϊ����ȡ
            data.bonues = 0;
            data.status = 1;
            PlayerIdLoginBonuesCache.Instance.UpdateDataToDb(data);

            return res;
        }

        /// <summary>
        /// ����ѡ����ҵ�����
        /// ��֤��ҵ�¼״̬����������������ȡ�򴴽�������ݣ��������Ƿ񱻶��ᣬ
        /// ���������Ϣ����Ӧ���󣬹������ ID �ͻỰ������¼�������������ʵ�塣
        /// </summary>
        /// <param name="s">�Ự���󣬰�����ҵ�¼��Ϣ��</param>
        /// <param name="req">ѡ����ҵ��������</param>
        /// <returns>ѡ����ҵ���Ӧ����</returns>
        public ResSelectPlayer HandlerReqSelectPlayer(IdSession s, ReqSelectPlayer req) {
            ResSelectPlayer res = new ResSelectPlayer();

            // ��֤����˺��Ƿ��ѵ�¼
            if (s.accountId <= 0) {
                res.status = (int)Respones.AccountIsNotLogin;
                return res;
            }

            // ��������������Ч��
            if (req.job <= 0) {
                res.status = (int)Respones.InvalidParams;
                return res;
            }

            // ��ȡ�򴴽��������
            Game.Datas.DBEntities.Player p = PlayerAcountIDCache.Instance.GetOrCreate(s.accountId, req);
            if (p == null) {
                res.status = (int)Respones.SystemErr;
                return res;
            }

            // �������Ƿ񱻶���
            if (p.status != 0) {
                res.status = (int)Respones.PlayerIsFreeze;
                return res;
            }

            res.status = (int)Respones.OK;
            res.pInfo = new PlayerInfo();
            this.CopyDbPlayerToResponesData(p, res.pInfo);

            s.playerId = p.id;
            s.accountIdAndJob = PlayerAcountIDCache.Instance.Key(s.accountId, req.job);

            // ����¼����
            this.CheckLoginBonues(s.playerId, res.pInfo);

            // �������ʵ��
            GM_EntityMgr.Instance.AddPlayer(s.playerId, s.accountId);

            return res;
        }

        /// <summary>
        /// ������ȡ������ݵ�����
        /// ��֤��ҵ�¼״̬������������������������ݣ��������Ƿ񱻶��ᣬ
        /// ���������Ϣ����Ӧ���󣬹������ ID �ͻỰ������¼�������������ʵ�塣
        /// </summary>
        /// <param name="s">�Ự���󣬰�����ҵ�¼��Ϣ��</param>
        /// <param name="req">��ȡ������ݵ��������</param>
        /// <returns>��ȡ������ݵ���Ӧ����</returns>
        public ResPullingPlayerData HandlerReqPullingPlayerData(IdSession s, ReqPullingPlayerData req) {
            ResPullingPlayerData res = new ResPullingPlayerData();

            // ��֤����˺��Ƿ��ѵ�¼
            if (s.accountId <= 0) {
                res.status = (int)Respones.AccountIsNotLogin;
                return res;
            }

            int job = req.job;

            // �����������
            Game.Datas.DBEntities.Player p = PlayerAcountIDCache.Instance.TryGetPlayer(s.accountId, req.job);
            if (p == null) {
                res.status = (int)Respones.PlayerIsNotExist;
                return res;
            }

            // �������Ƿ񱻶���
            if (p.status != 0) {
                res.status = (int)Respones.PlayerIsFreeze;
                return res;
            }

            res.status = (int)Respones.OK;
            res.pInfo = new PlayerInfo();
            this.CopyDbPlayerToResponesData(p, res.pInfo);

            s.playerId = p.id;
            s.accountIdAndJob = PlayerAcountIDCache.Instance.Key(s.accountId, req.job);

            // ����¼����
            this.CheckLoginBonues(s.playerId, res.pInfo);

            // �������ʵ��
            GM_EntityMgr.Instance.AddPlayer(s.playerId, s.accountId);

            return res;
        }

        /// <summary>
        /// �����ҵ�ÿ�յ�¼���������
        /// ���ݽ���ʱ���״̬���½�����Ϣ����ͬ������������ݿ⡣
        /// </summary>
        /// <param name="playerId">��ҵ� ID��</param>
        /// <param name="res">�����Ϣ��Ӧ�������ڸ��½�����Ϣ��</param>
        private void CheckLoginBonues(long playerId, PlayerInfo res) {
            res.hasBonues = 0;
            Loginbonues data = PlayerIdLoginBonuesCache.Instance.GetOrCreate(playerId);
            if (data == null) {
                logger.Error($"ÿ�յ�¼������ȡΪnull: {playerId}");
                return;
            }

            if (data.status == 0) {
                res.hasBonues = 1;
                res.days = data.days;
                res.loginBonues = data.bonues;
            }

            bool hasLoginBonues = (data.bonues_time < UtilsHelper.TimestampToday());
            if (!hasLoginBonues) {
                return;
            }

            bool isSustain = (data.bonues_time < UtilsHelper.TimestampToday()) &&
                (data.bonues_time >= UtilsHelper.TimestampYesterday());

            data.days = (isSustain) ? (data.days + 1) : 1;
            data.bonues = 100;
            data.bonues_time = (int)(UtilsHelper.Timestamp());
            data.status = 0;

            res.hasBonues = 1;
            res.days = data.days;
            res.loginBonues = data.bonues;

            PlayerIdLoginBonuesCache.Instance.UpdateDataToDb(data);
        }

        /// <summary>
        /// ������ȡ�����б������
        /// ��֤��ҵ�¼״̬����������������ȡ�������ݲ�ת��Ϊ��Ӧ����
        /// </summary>
        /// <param name="s">�Ự���󣬰�����ҵ�¼��Ϣ��</param>
        /// <param name="req">��ȡ�����б���������</param>
        /// <returns>��ȡ�����б����Ӧ����</returns>
        public ResPullingBonuesList HandlerReqPullingBonuesList(IdSession s, ReqPullingBonuesList req) {
            ResPullingBonuesList res = new ResPullingBonuesList();

            // ��֤����˺��Ƿ��ѵ�¼
            if (s.accountId <= 0 || s.playerId <= 0) {
                res.status = (int)Respones.AccountIsNotLogin;
                return res;
            }

            int typeId = req.typeId;

            // ��ȡ��������
            Bonues[] datas = GM_BonuesMgr.Instance.PullingBonuesData(s.playerId, typeId);
            if (datas != null && datas.Length > 0) {
                res.bonues = new BonuesItem[datas.Length];
                for (int i = 0; i < datas.Length; i++) {
                    res.bonues[i] = new BonuesItem();
                    res.bonues[i].bonuesId = datas[i].id;
                    res.bonues[i].bonuesDesic = datas[i].bonuesDesic;
                    res.bonues[i].status = datas[i].status;
                    res.bonues[i].typeId = datas[i].tid;
                }
            }

            res.status = (int)Respones.OK;

            return res;
        }

        /// <summary>
        /// ������ȡ����������
        /// ��֤��ҵ�¼״̬����齱���Ƿ����ȡ����ȡ������������Ӧ��Ϣ��
        /// </summary>
        /// <param name="s">�Ự���󣬰�����ҵ�¼��Ϣ��</param>
        /// <param name="req">��ȡ�������������</param>
        /// <returns>��ȡ��������Ӧ����</returns>
        public ResRecvBonues HandlerReqRecvBonues(IdSession s, ReqRecvBonues req) {
            ResRecvBonues res = new ResRecvBonues();

            // ��֤����˺��Ƿ��ѵ�¼
            if (s.accountId <= 0 || s.playerId <= 0) {
                res.status = (int)Respones.AccountIsNotLogin;
                return res;
            }

            // ��齱���Ƿ�����ҿ���ȡ
            Bonues data = GM_BonuesMgr.Instance.GetBonuesBy(req.bonuesId);
            if (data == null || data.uid != s.playerId || data.status != 0) {
                res.status = (int)Respones.InvalidOpt;
                return res;
            }

            // ��ȡ����
            GM_BonuesMgr.Instance.RecvBonues(req.bonuesId, data);
            res.status = (int)Respones.OK;
            res.typeId = data.tid;
            res.b1 = data.bonues1;
            res.b2 = data.bonues2;
            res.b3 = data.bonues3;
            res.b4 = data.bonues4;
            res.b5 = data.bonues5;

            return res;
        }

        /// <summary>
        /// ������ȡ�����б������
        /// ��֤��ҵ�¼״̬����ȡ���ʵ�壬��������������ȡ�����б�ת��Ϊ��Ӧ����
        /// </summary>
        /// <param name="s">�Ự���󣬰�����ҵ�¼��Ϣ��</param>
        /// <param name="req">��ȡ�����б���������</param>
        /// <returns>��ȡ�����б����Ӧ����</returns>
        public ResPullingTaskList HandlerReqPullingTaskList(IdSession s, ReqPullingTaskList req) {
            ResPullingTaskList res = new ResPullingTaskList();

            // ��֤����˺��Ƿ��ѵ�¼
            if (s.accountId <= 0 || s.playerId <= 0) {
                res.status = (int)Respones.AccountIsNotLogin;
                return res;
            }

            int typeId = req.typeId;

            // ��ȡ���ʵ��
            GM_PlayerEntity player = GM_EntityMgr.Instance.Get(s.playerId);
            if (player == null) {
                res.status = (int)Respones.InvalidOpt;
                return res;
            }

            // ��ȡ�����б�
            List<GM_Task> tasks = GM_TaskMgr.Instance.PullingTaskList(player, typeId);

            if (tasks != null && tasks.Count > 0) {
                res.tasks = new TaskItem[tasks.Count];
                for (int i = 0; i < tasks.Count; i++) {
                    res.tasks[i] = new TaskItem();
                    res.tasks[i].taskDesic = tasks[i].taskDesic;
                    res.tasks[i].status = tasks[i].dbTaskInst.status;
                    res.tasks[i].typeId = tasks[i].dbTaskInst.tid;
                }
            }

            res.status = (int)Respones.OK;
            return res;
        }

        /// <summary>
        /// ��������ʼ���Ϣ״̬������
        /// ��֤��ҵ�¼״̬����ȡ���ʵ�壬�����ʼ���Ϣ״̬��
        /// </summary>
        /// <param name="s">�Ự���󣬰�����ҵ�¼��Ϣ��</param>
        /// <param name="req">�����ʼ���Ϣ״̬���������</param>
        /// <returns>�����ʼ���Ϣ״̬����Ӧ����</returns>
        public ResUpdateMailMsg HandlerReqUpdateMailMsg(IdSession s, ReqUpdateMailMsg req) {
            ResUpdateMailMsg res = new ResUpdateMailMsg();

            // ��֤����˺��Ƿ��ѵ�¼
            if (s.accountId <= 0 || s.playerId <= 0) {
                res.status = (int)Respones.AccountIsNotLogin;
                return res;
            }

            // ��ȡ���ʵ��
            GM_PlayerEntity player = GM_EntityMgr.Instance.Get(s.playerId);
            if (player == null) {
                res.status = (int)Respones.InvalidOpt;
                return res;
            }

            // �����ʼ���Ϣ״̬
            GM_MailMsgMgr.Instance.UpdateMailMsgStatus(req.mailMsgId, req.status);
            res.status = (int)Respones.OK;
            return res;
        }

        /// <summary>
        /// ������ȡ�ʼ���Ϣ������
        /// �˷�������֤��ҵĵ�¼״̬����ȡ���ʵ�壬�����ݿ���ȡ�ʼ���Ϣ���ݣ�
        /// ������ת��Ϊ��Ӧ��������ĸ�ʽ�����շ�����ȡ�ʼ���Ϣ����Ӧ�����
        /// </summary>
        /// <param name="s">�Ự���󣬰�����ҵĵ�¼��Ϣ�����˻� ID ����� ID��</param>
        /// <param name="req">��ȡ�ʼ���Ϣ��������󣬿��ܰ���ɸѡ��������Ϣ��</param>
        /// <returns>��ȡ�ʼ���Ϣ����Ӧ���󣬰����ʼ���Ϣ�б�ʹ���״̬��</returns>
        public ResPullingMailMsg HandlerReqPullingMailMsg(IdSession s, ReqPullingMailMsg req) {
            // ������ȡ�ʼ���Ϣ����Ӧ����
            ResPullingMailMsg res = new ResPullingMailMsg();

            // ��֤����˺��Ƿ��ѵ�¼����δ��¼�򷵻ض�Ӧ����״̬
            if (s.accountId <= 0 || s.playerId <= 0) {
                res.status = (int)Respones.AccountIsNotLogin;
                return res;
            }

            // ͨ����� ID ��ʵ��������л�ȡ���ʵ��
            GM_PlayerEntity player = GM_EntityMgr.Instance.Get(s.playerId);
            if (player == null) {
                res.status = (int)Respones.InvalidOpt;
                return res;
            }

            // ������Ӧ״̬Ϊ�ɹ�
            res.status = (int)Respones.OK;

            // ���ʼ���Ϣ����������ȡָ����ҵ��ʼ���Ϣ���ݣ������ 0 ������ĳ��Ĭ�ϵ�ɸѡ����
            Game.Datas.DBEntities.Mailmessage[] mailMessages = GM_MailMsgMgr.Instance.PullingMailMsg(s.playerId, 0);
            if (mailMessages != null && mailMessages.Length > 0) {
                // ��ʼ����Ӧ�����е��ʼ���Ϣ�б�
                res.mailMessages = new MailMsgItem[mailMessages.Length];
                for (int i = 0; i < mailMessages.Length; i++) {
                    // ���������ʼ���Ϣ��
                    res.mailMessages[i] = new MailMsgItem();
                    // �����ʼ���Ϣ������
                    res.mailMessages[i].msgBody = mailMessages[i].msgBody;
                    // �����ʼ���Ϣ��״̬
                    res.mailMessages[i].status = mailMessages[i].status;
                    // �����ʼ���Ϣ�ķ���ʱ��
                    res.mailMessages[i].sendTime = mailMessages[i].sendTime;
                    // �����ʼ���Ϣ�� ID
                    res.mailMessages[i].msgId = mailMessages[i].id;
                }
            }

            return res;
        }

        /// <summary>
        /// ������ȡ���а����ݵ�����
        /// �˷�������֤��ҵĵ�¼״̬����ȡ���ʵ�壬�����а�������л�ȡָ�����͵����а����ݣ�
        /// ������ת��Ϊ��Ӧ��������ĸ�ʽ��ͬʱ�����������а��е�λ�ã����շ�����ȡ���а����ݵ���Ӧ�����
        /// </summary>
        /// <param name="s">�Ự���󣬰�����ҵĵ�¼��Ϣ�����˻� ID ����� ID��</param>
        /// <param name="req">��ȡ���а����ݵ�������󣬿��ܰ������а����͵���Ϣ��</param>
        /// <returns>��ȡ���а����ݵ���Ӧ���󣬰������а������б�������������ʹ���״̬��</returns>
        public ResPullingRank HandlerReqPullingRank(IdSession s, ReqPullingRank req) {
            // ������ȡ���а����ݵ���Ӧ����
            ResPullingRank res = new ResPullingRank();

            // ��֤����˺��Ƿ��ѵ�¼����δ��¼�򷵻ض�Ӧ����״̬
            if (s.accountId <= 0 || s.playerId <= 0) {
                res.status = (int)Respones.AccountIsNotLogin;
                return res;
            }

            // ͨ����� ID ��ʵ��������л�ȡ���ʵ��
            GM_PlayerEntity player = GM_EntityMgr.Instance.Get(s.playerId);
            if (player == null) {
                res.status = (int)Respones.InvalidOpt;
                return res;
            }

            // ������Ӧ״̬Ϊ�ɹ�
            res.status = (int)Respones.OK;
            // ��ʼ��������������а��е�����Ϊ -1����ʾδ�ҵ�
            res.selfIndex = -1;

            // ������ʱ�̶����а�����Ϊ���������а񣬿ɸ��������޸�
            int rankType = (int)RankType.WorldCoin;
            // �����а�������л�ȡָ�����͵����а����ݣ�����ȡǰ 30 ��
            RankData[] rankData = GM_RankMgr.Instance.GetRankData(rankType, 30);
            if (rankData == null || rankData.Length <= 0) {
                return res;
            }

            // ��ʼ����Ӧ�����е����а������б�
            res.ranks = new RankItem[rankData.Length];
            for (int i = 0; i < rankData.Length; i++) {
                // �����������а���
                RankItem item = new RankItem();

                // ͨ�����а������е���� ID ����� ID �����л�ȡ�����Ϣ
                Player p = PlayerIDCache.Instance.Get(rankData[i].uid);
                if (p == null) {
                    // ��δ�ҵ������Ϣ����¼������־��������������
                    this.logger.Error($"rankdata Error uid {rankData[i].uid}");
                    continue;
                }
                // ͨ�������Ϣ�е��˻� ID ���˻� ID �����л�ȡ�˻���Ϣ
                Account a = AccountIDCache.Instance.Get(p.accountId);
                if (a == null) {
                    // ��δ�ҵ��˻���Ϣ����¼������־��������������
                    this.logger.Error($"rankdata Error uid {rankData[i].uid}");
                    continue;
                }
                // �����˻���ͷ����Ϣ�����а���
                item.uface = a.uface;
                // �����˻����ǳ���Ϣ�����а���
                item.unick = a.unick;
                // �������а������е�ֵ�����а���
                item.value = rankData[i].value;

                // ��鵱ǰ���а������� ID �Ƿ�Ϊ��ǰ������ҵ� ID���������¼�����������
                if (rankData[i].uid == s.playerId) {
                    res.selfIndex = i;
                }

                // �����а�����ӵ���Ӧ��������а������б���
                res.ranks[i] = item;
            }

            return res;
        }

        /// <summary>
        /// ����һ���Ʒ������
        /// �˷�������֤��ҵĵ�¼״̬����ȡ���ʵ�壬�������Ƿ���Զһ�ָ����Ʒ��
        /// ��������ִ�жһ����������շ��ضһ���Ʒ����Ӧ�����
        /// </summary>
        /// <param name="s">�Ự���󣬰�����ҵĵ�¼��Ϣ�����˻� ID ����� ID��</param>
        /// <param name="req">�һ���Ʒ��������󣬰���Ҫ�һ�����Ʒ ID ����Ϣ��</param>
        /// <returns>�һ���Ʒ����Ӧ���󣬰�������״̬��</returns>
        public ResExchangeProduct HandlerReqExchangeProduct(IdSession s, ReqExchangeProduct req) {
            // �����һ���Ʒ����Ӧ����
            ResExchangeProduct res = new ResExchangeProduct();

            // ��֤����˺��Ƿ��ѵ�¼����δ��¼�򷵻ض�Ӧ����״̬
            if (s.accountId <= 0 || s.playerId <= 0) {
                res.status = (int)Respones.AccountIsNotLogin;
                return res;
            }

            // ͨ����� ID ��ʵ��������л�ȡ���ʵ��
            GM_PlayerEntity player = GM_EntityMgr.Instance.Get(s.playerId);
            if (player == null) {
                res.status = (int)Respones.InvalidOpt;
                return res;
            }

            // �������Ƿ���Զһ�ָ����Ʒ�����ؼ��״̬
            int status = GM_TradingMgr.Instance.CanExchangeProduct(player, req.productId);
            if (status != (int)Respones.OK) {
                // ����鲻ͨ���������״̬����Ϊ��Ӧ״̬������
                res.status = status;
                return res;
            }

            // �����ͨ����ִ�жһ���Ʒ�Ĳ���
            GM_TradingMgr.Instance.DoExchangeProduct(player, req.productId);

            // ������Ӧ״̬Ϊ�ɹ�
            res.status = (int)Respones.OK;
            return res;
        }

        /// <summary>
        /// ������ȡ�������ݵ�����
        /// �˷�������֤��ҵĵ�¼״̬����ȡ���ʵ�壬�ӱ����������л�ȡ��ҵı������ݣ�
        /// ������ת��Ϊ��Ӧ��������ĸ�ʽ�����շ�����ȡ�������ݵ���Ӧ�����
        /// </summary>
        /// <param name="s">�Ự���󣬰�����ҵĵ�¼��Ϣ�����˻� ID ����� ID��</param>
        /// <param name="req">��ȡ�������ݵ�������󣬿��ܰ���ɸѡ��������Ϣ��</param>
        /// <returns>��ȡ�������ݵ���Ӧ���󣬰���������Ʒ�б�ʹ���״̬��</returns>
        public ResPullingPackData HandlerReqPullingPackData(IdSession s, ReqPullingPackData req) {
            // ������ȡ�������ݵ���Ӧ����
            ResPullingPackData res = new ResPullingPackData();

            // ��֤����˺��Ƿ��ѵ�¼����δ��¼�򷵻ض�Ӧ����״̬
            if (s.accountId <= 0 || s.playerId <= 0) {
                res.status = (int)Respones.AccountIsNotLogin;
                return res;
            }

            // ͨ����� ID ��ʵ��������л�ȡ���ʵ��
            GM_PlayerEntity player = GM_EntityMgr.Instance.Get(s.playerId);
            if (player == null) {
                res.status = (int)Respones.InvalidOpt;
                return res;
            }

            // �ӱ����������л�ȡ��ҵı�������
            Dictionary<int, List<GoodsItem>> ret = GM_BackpackMgr.Instance.GetBackpackData(ref player.uBackpack);
            // ��ʼ����Ӧ�����еı�����Ʒ�б�
            res.packGoods = new List<DicGoodsItem>();
            foreach (var key in ret.Keys) {
                // ��������������Ʒ��
                DicGoodsItem dic = new DicGoodsItem();
                // ������Ʒ��������
                dic.mainType = key;
                // ������Ʒ�б�
                dic.Value = ret[key];
                // ��������Ʒ����ӵ���Ӧ����ı�����Ʒ�б���
                res.packGoods.Add(dic);
            }

            // ������Ӧ״̬Ϊ�ɹ�
            res.status = (int)Respones.OK;
            return res;
        }

        /// <summary>
        /// ������Ը�����Ʒ������
        /// �˷�������֤��ҵĵ�¼״̬����ȡ���ʵ�壬���ñ�������������ָ��������Ʒ��������
        /// ���շ��ز��Ը�����Ʒ����Ӧ�����
        /// </summary>
        /// <param name="s">�Ự���󣬰�����ҵĵ�¼��Ϣ�����˻� ID ����� ID��</param>
        /// <param name="req">���Ը�����Ʒ��������󣬰�����Ʒ���� ID �͸�����������Ϣ��</param>
        /// <returns>���Ը�����Ʒ����Ӧ���󣬰�������״̬��</returns>
        public ResTestUpdateGoods HandlerReqTestUpdatesGoods(IdSession s, ReqTestUpdateGooods req) {
            // �������Ը�����Ʒ����Ӧ����
            ResTestUpdateGoods res = new ResTestUpdateGoods();

            // ��֤����˺��Ƿ��ѵ�¼����δ��¼�򷵻ض�Ӧ����״̬
            if (s.accountId <= 0 || s.playerId <= 0) {
                res.status = (int)Respones.AccountIsNotLogin;
                return res;
            }

            // ͨ����� ID ��ʵ��������л�ȡ���ʵ��
            GM_PlayerEntity player = GM_EntityMgr.Instance.Get(s.playerId);
            if (player == null) {
                res.status = (int)Respones.InvalidOpt;
                return res;
            }

            // ���ñ�������������ָ��������Ʒ������
            GM_BackpackMgr.Instance.UpdateGoodsWithTid(player, req.typeId, req.num);

            // ������Ӧ״̬Ϊ�ɹ�
            res.status = (int)Respones.OK;
            return res;
        }

        /// <summary>
        /// ������Ի�ȡ��Ʒ������
        /// �˷�������֤��ҵĵ�¼״̬����ȡ���ʵ�壬������Ʒ���͸������������ȣ�
        /// ���շ��ز��Ի�ȡ��Ʒ����Ӧ�����
        /// </summary>
        /// <param name="s">�Ự���󣬰�����ҵĵ�¼��Ϣ�����˻� ID ����� ID��</param>
        /// <param name="req">���Ի�ȡ��Ʒ��������󣬰�����Ʒ���� ID �ͻ�ȡ��������Ϣ��</param>
        /// <returns>���Ի�ȡ��Ʒ����Ӧ���󣬰�������״̬��</returns>
        public ResTestGetGoods HandlerReqTestGetGoods(IdSession s, ReqTestGetGoods req) {
            // �������Ի�ȡ��Ʒ����Ӧ����
            ResTestGetGoods res = new ResTestGetGoods();

            // ��֤����˺��Ƿ��ѵ�¼����δ��¼�򷵻ض�Ӧ����״̬
            if (s.accountId <= 0 || s.playerId <= 0) {
                res.status = (int)Respones.AccountIsNotLogin;
                return res;
            }

            // ͨ����� ID ��ʵ��������л�ȡ���ʵ��
            GM_PlayerEntity player = GM_EntityMgr.Instance.Get(s.playerId);
            if (player == null) {
                res.status = (int)Respones.InvalidOpt;
                return res;
            }

            // ���Դ��룺������Ʒ���͸�������������
            if (req.typeId == 1) // ��ʯ
            {
                // ��ȡ��ǰ��ʯ�ռ���������
                GM_Task collectTask = GM_TaskMgr.Instance.GetCurrectTaskData(player, 100000);
                if (collectTask != null) {
                    // �����������
                    GM_TaskMgr.Instance.UpdateTaskProgress(player, collectTask, "damond", req.num);
                }
            } else if (req.typeId == 2) // ����
              {
                // ��ȡ��ǰ�����ռ���������
                GM_Task collectTask = GM_TaskMgr.Instance.GetCurrectTaskData(player, 100000);
                if (collectTask != null) {
                    // �����������
                    GM_TaskMgr.Instance.UpdateTaskProgress(player, collectTask, "book", req.num);
                }
            }

            // ������Ӧ״̬Ϊ�ɹ�
            res.status = (int)Respones.OK;
            return res;
        }
    }
}