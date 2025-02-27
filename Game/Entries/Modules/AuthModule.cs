using Framework.Core.Net;
using Framework.Core.Utils;
using Game.Datas.DBEntities;
using Game.Datas.Messages;
using Game.Core.GM_Bonues;
using Game.Core.Caches;

namespace Game.Entries.Modules
{
    /// <summary>
    /// ��֤ģ���࣬��������Ϸ�е���֤���ҵ�����ο͵�¼���ο��������û���¼��ע��ȡ�
    /// ���õ���ģʽ��ȷ��ȫ��ֻ��һ��ʵ����
    /// </summary>
    public class AuthModule
    {
        /// <summary>
        /// ����ʵ����ȫ��Ψһ��
        /// </summary>
        public static AuthModule Instance = new AuthModule();

        /// <summary>
        /// ��ʼ�����������ڳ�ʼ���˻����档
        /// ���� AccountGuestCache �� AccountIDCache �ĳ�ʼ��������
        /// </summary>
        public void Init() {
            AccountGuestCache.Instance.Init();
            AccountIDCache.Instance.Init();
        }

        /// <summary>
        /// �����ݿ��е��˻���Ϣ���Ƶ���Ӧ���ݶ����С�
        /// </summary>
        /// <param name="dbAccount">���ݿ��е��˻�����</param>
        /// <param name="aInfo">Ҫ�����˻���Ϣ��Ӧ����</param>
        private void CopyDbAccountToResponesData(Account dbAccount, AccountInfo aInfo) {
            aInfo.uface = dbAccount.uface;
            aInfo.unick = dbAccount.unick;
            aInfo.isGuest = dbAccount.is_guest;
            aInfo.uvip = dbAccount.uvip;
        }

        /// <summary>
        /// �����ο͵�¼����
        /// ��֤�������������˻��Ƿ�Ϊ�ο��˻���δ�����ᣬ���˻���Ϣ���Ƶ���Ӧ���󣬲������Ự���˻� ID��
        /// </summary>
        /// <param name="s">�Ự����</param>
        /// <param name="req">�ο͵�¼�������</param>
        /// <returns>�ο͵�¼��Ӧ����</returns>
        public ResGuestLogin HandlerReqGuestLogin(IdSession s, ReqGuestLogin req) {
            ResGuestLogin res = new ResGuestLogin();
            res.uinfo = null;

            // ��������������Ч��
            if (req == null ||
                req.guestKey == null ||
                req.guestKey.Equals("") ||
                req.channal <= (int)Channal.InvalidChannal) {
                res.status = (int)Respones.InvalidParams;
                res.uinfo = null;

                return res;
            }

            // �ӻ����л�ȡ�򴴽��˻�
            Account dbAccount = AccountGuestCache.Instance.GetOrCreate(req.guestKey, req.channal);
            if (dbAccount.is_guest != 1) // ���˺�������������ʽ�˺ţ��������ο͵�¼
            {
                res.status = (int)Respones.UserIsNotGuest;
                return res;
            }

            if (dbAccount.status != 0) // �˺ű�����
            {
                res.status = (int)Respones.UserIsFreeze;
                return res;
            }

            res.status = (int)Respones.OK;
            res.uinfo = new AccountInfo();
            this.CopyDbAccountToResponesData(dbAccount, res.uinfo);

            // ���˻� ID ��Ự�����������������
            s.accountId = dbAccount.uid;

            return res;
        }

        /// <summary>
        /// �����ο��˺���������
        /// ��֤�Ự���������������û����Ƿ��ظ��������˻���Ϣ��ͬ������������ݿ⣬Ϊ�����������������
        /// </summary>
        /// <param name="s">�Ự����</param>
        /// <param name="req">�ο������������</param>
        /// <returns>�ο�������Ӧ����</returns>
        public ResGuestUpgrade HandlerReqGuestUpgrade(IdSession s, ReqGuestUpgrade req) {
            ResGuestUpgrade res = new ResGuestUpgrade();

            // ��֤�Ự�е��˻� ID �Ƿ���Ч
            if (s.accountId <= 0) {
                res.status = (int)Respones.AccountIsNotLogin;
                return res;
            }

            // ��������������Ч��
            if (req == null ||
                req.uname == null ||
                req.uname.Equals("") ||
                req.upwd == null ||
                req.upwd.Equals("") ||
                req.unick == null ||
                req.unick.Equals("")) {
                res.status = (int)Respones.InvalidParams;
                return res;
            }

            // ����û����Ƿ��Ѵ���
            Account dbAccount = AccountUnameCache.Instance.Get(req.uname);
            if (dbAccount != null) {
                res.status = (int)Respones.UnameIsExist;
                return res;
            }

            // ����Ƿ�Ϊ�ο��˺�
            dbAccount = AccountIDCache.Instance.Get(s.accountId);
            if (dbAccount.is_guest == 0) {
                res.status = (int)Respones.UserIsNotGuest;
                return res;
            }

            // ���οͻ������Ƴ����˺�
            AccountGuestCache.Instance.Remove(dbAccount.guest_key);

            // �����˻���Ϣ
            dbAccount.is_guest = 0;
            dbAccount.guest_key = "";
            dbAccount.uname = req.uname;
            dbAccount.upwd = UtilsHelper.Md5(req.upwd);
            dbAccount.unick = req.unick;

            // �����º���˻���Ϣͬ�������ݿ�
            AccountIDCache.Instance.UpdateAccountToDatabase(dbAccount);

            // ���˻���Ϣ��ӵ��û���������
            AccountUnameCache.Instance.Put(dbAccount.uname, dbAccount);

            res.status = (int)Respones.OK;
            res.uinfo = new AccountInfo();
            this.CopyDbAccountToResponesData(dbAccount, res.uinfo);

            // Ϊ��������ο��˺������Ľ���
            Game.Datas.Excels.BonuesRuleA configItem = (Game.Datas.Excels.BonuesRuleA)ExcelUtils.GetConfigData<Game.Datas.Excels.BonuesRuleA>("100001");
            GM_BonuesMgr.Instance.GenBonuesToPlayer(s.playerId, configItem.ID, configItem.value);

            return res;
        }

        /// <summary>
        /// �����û���¼����
        /// ��֤�������������˻��Ƿ���ڣ���֤�����Ƿ���ȷ�����˻���Ϣ���Ƶ���Ӧ���󣬲������Ự���˻� ID��
        /// </summary>
        /// <param name="s">�Ự����</param>
        /// <param name="req">�û���¼�������</param>
        /// <returns>�û���¼��Ӧ����</returns>
        public ResUserLogin HandlerReqUserLogin(IdSession s, ReqUserLogin req) {
            ResUserLogin res = new ResUserLogin();

            // ��������������Ч��
            if (req == null ||
                req.uname == null ||
                req.uname.Equals("") ||
                req.upwd == null ||
                req.upwd.Equals("")) {
                res.status = (int)Respones.InvalidParams;
                return res;
            }

            // ���û��������л�ȡ�˻�
            Account dbAccount = AccountUnameCache.Instance.Get(req.uname);
            if (dbAccount == null) {
                res.status = (int)Respones.AccountIsNotExist;
                return res;
            }

            // ��֤�����Ƿ���ȷ
            string md5Password = UtilsHelper.Md5(req.upwd);
            if (!md5Password.Equals(dbAccount.upwd)) {
                res.status = (int)Respones.UnameOrUpwdError;
                return res;
            }

            res.status = (int)Respones.OK;
            res.uinfo = new AccountInfo();
            this.CopyDbAccountToResponesData(dbAccount, res.uinfo);

            // ���˻� ID ��Ự����
            s.accountId = dbAccount.uid;

            return res;
        }

        /// <summary>
        /// �����û�ע������
        /// ��֤�������������û����Ƿ��ظ����������˻������»��档
        /// </summary>
        /// <param name="s">�Ự����</param>
        /// <param name="req">�û�ע���������</param>
        /// <returns>�û�ע����Ӧ����</returns>
        public ResRegisterUser HandlerReqRegisterUser(IdSession s, ReqRegisterUser req) {
            ResRegisterUser res = new ResRegisterUser();
            res.errorStr = null;

            // ��������������Ч��
            if (req == null ||
                req.uname == null ||
                req.uname.Equals("") ||
                req.upwd == null ||
                req.upwd.Equals("") ||
                req.channal <= (int)Channal.InvalidChannal) {
                res.status = (int)Respones.InvalidParams;
                return res;
            }

            // ����û����Ƿ��Ѵ���
            Account dbAccount = AccountUnameCache.Instance.Get(req.uname);
            if (dbAccount != null) {
                res.status = (int)Respones.UnameIsExist;
                return res;
            }

            // �������˻�
            dbAccount = AccountUnameCache.Instance.GetOrCreate(req);
            if (dbAccount == null) {
                res.status = (int)Respones.SystemErr;
                return res;
            }

            res.status = (int)Respones.OK;
            return res;
        }
    }
}