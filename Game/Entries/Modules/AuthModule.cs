using Framework.Core.Net;
using Framework.Core.Utils;
using Game.Datas.DBEntities;
using Game.Datas.Messages;
using Game.Core.GM_Bonues;
using Game.Core.Caches;

namespace Game.Entries.Modules
{
    /// <summary>
    /// 认证模块类，负责处理游戏中的认证相关业务，如游客登录、游客升级、用户登录和注册等。
    /// 采用单例模式，确保全局只有一个实例。
    /// </summary>
    public class AuthModule
    {
        /// <summary>
        /// 单例实例，全局唯一。
        /// </summary>
        public static AuthModule Instance = new AuthModule();

        /// <summary>
        /// 初始化方法，用于初始化账户缓存。
        /// 调用 AccountGuestCache 和 AccountIDCache 的初始化方法。
        /// </summary>
        public void Init() {
            AccountGuestCache.Instance.Init();
            AccountIDCache.Instance.Init();
        }

        /// <summary>
        /// 将数据库中的账户信息复制到响应数据对象中。
        /// </summary>
        /// <param name="dbAccount">数据库中的账户对象。</param>
        /// <param name="aInfo">要填充的账户信息响应对象。</param>
        private void CopyDbAccountToResponesData(Account dbAccount, AccountInfo aInfo) {
            aInfo.uface = dbAccount.uface;
            aInfo.unick = dbAccount.unick;
            aInfo.isGuest = dbAccount.is_guest;
            aInfo.uvip = dbAccount.uvip;
        }

        /// <summary>
        /// 处理游客登录请求。
        /// 验证请求参数，检查账户是否为游客账户且未被冻结，将账户信息复制到响应对象，并关联会话和账户 ID。
        /// </summary>
        /// <param name="s">会话对象。</param>
        /// <param name="req">游客登录请求对象。</param>
        /// <returns>游客登录响应对象。</returns>
        public ResGuestLogin HandlerReqGuestLogin(IdSession s, ReqGuestLogin req) {
            ResGuestLogin res = new ResGuestLogin();
            res.uinfo = null;

            // 检查请求参数的有效性
            if (req == null ||
                req.guestKey == null ||
                req.guestKey.Equals("") ||
                req.channal <= (int)Channal.InvalidChannal) {
                res.status = (int)Respones.InvalidParams;
                res.uinfo = null;

                return res;
            }

            // 从缓存中获取或创建账户
            Account dbAccount = AccountGuestCache.Instance.GetOrCreate(req.guestKey, req.channal);
            if (dbAccount.is_guest != 1) // 该账号是已升级的正式账号，不允许游客登录
            {
                res.status = (int)Respones.UserIsNotGuest;
                return res;
            }

            if (dbAccount.status != 0) // 账号被冻结
            {
                res.status = (int)Respones.UserIsFreeze;
                return res;
            }

            res.status = (int)Respones.OK;
            res.uinfo = new AccountInfo();
            this.CopyDbAccountToResponesData(dbAccount, res.uinfo);

            // 将账户 ID 与会话关联，方便后续处理
            s.accountId = dbAccount.uid;

            return res;
        }

        /// <summary>
        /// 处理游客账号升级请求。
        /// 验证会话和请求参数，检查用户名是否重复，更新账户信息并同步到缓存和数据库，为玩家生成升级奖励。
        /// </summary>
        /// <param name="s">会话对象。</param>
        /// <param name="req">游客升级请求对象。</param>
        /// <returns>游客升级响应对象。</returns>
        public ResGuestUpgrade HandlerReqGuestUpgrade(IdSession s, ReqGuestUpgrade req) {
            ResGuestUpgrade res = new ResGuestUpgrade();

            // 验证会话中的账户 ID 是否有效
            if (s.accountId <= 0) {
                res.status = (int)Respones.AccountIsNotLogin;
                return res;
            }

            // 检查请求参数的有效性
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

            // 检查用户名是否已存在
            Account dbAccount = AccountUnameCache.Instance.Get(req.uname);
            if (dbAccount != null) {
                res.status = (int)Respones.UnameIsExist;
                return res;
            }

            // 检查是否为游客账号
            dbAccount = AccountIDCache.Instance.Get(s.accountId);
            if (dbAccount.is_guest == 0) {
                res.status = (int)Respones.UserIsNotGuest;
                return res;
            }

            // 从游客缓存中移除该账号
            AccountGuestCache.Instance.Remove(dbAccount.guest_key);

            // 更新账户信息
            dbAccount.is_guest = 0;
            dbAccount.guest_key = "";
            dbAccount.uname = req.uname;
            dbAccount.upwd = UtilsHelper.Md5(req.upwd);
            dbAccount.unick = req.unick;

            // 将更新后的账户信息同步到数据库
            AccountIDCache.Instance.UpdateAccountToDatabase(dbAccount);

            // 将账户信息添加到用户名缓存中
            AccountUnameCache.Instance.Put(dbAccount.uname, dbAccount);

            res.status = (int)Respones.OK;
            res.uinfo = new AccountInfo();
            this.CopyDbAccountToResponesData(dbAccount, res.uinfo);

            // 为玩家生成游客账号升级的奖励
            Game.Datas.Excels.BonuesRuleA configItem = (Game.Datas.Excels.BonuesRuleA)ExcelUtils.GetConfigData<Game.Datas.Excels.BonuesRuleA>("100001");
            GM_BonuesMgr.Instance.GenBonuesToPlayer(s.playerId, configItem.ID, configItem.value);

            return res;
        }

        /// <summary>
        /// 处理用户登录请求。
        /// 验证请求参数，检查账户是否存在，验证密码是否正确，将账户信息复制到响应对象，并关联会话和账户 ID。
        /// </summary>
        /// <param name="s">会话对象。</param>
        /// <param name="req">用户登录请求对象。</param>
        /// <returns>用户登录响应对象。</returns>
        public ResUserLogin HandlerReqUserLogin(IdSession s, ReqUserLogin req) {
            ResUserLogin res = new ResUserLogin();

            // 检查请求参数的有效性
            if (req == null ||
                req.uname == null ||
                req.uname.Equals("") ||
                req.upwd == null ||
                req.upwd.Equals("")) {
                res.status = (int)Respones.InvalidParams;
                return res;
            }

            // 从用户名缓存中获取账户
            Account dbAccount = AccountUnameCache.Instance.Get(req.uname);
            if (dbAccount == null) {
                res.status = (int)Respones.AccountIsNotExist;
                return res;
            }

            // 验证密码是否正确
            string md5Password = UtilsHelper.Md5(req.upwd);
            if (!md5Password.Equals(dbAccount.upwd)) {
                res.status = (int)Respones.UnameOrUpwdError;
                return res;
            }

            res.status = (int)Respones.OK;
            res.uinfo = new AccountInfo();
            this.CopyDbAccountToResponesData(dbAccount, res.uinfo);

            // 将账户 ID 与会话关联
            s.accountId = dbAccount.uid;

            return res;
        }

        /// <summary>
        /// 处理用户注册请求。
        /// 验证请求参数，检查用户名是否重复，创建新账户并更新缓存。
        /// </summary>
        /// <param name="s">会话对象。</param>
        /// <param name="req">用户注册请求对象。</param>
        /// <returns>用户注册响应对象。</returns>
        public ResRegisterUser HandlerReqRegisterUser(IdSession s, ReqRegisterUser req) {
            ResRegisterUser res = new ResRegisterUser();
            res.errorStr = null;

            // 检查请求参数的有效性
            if (req == null ||
                req.uname == null ||
                req.uname.Equals("") ||
                req.upwd == null ||
                req.upwd.Equals("") ||
                req.channal <= (int)Channal.InvalidChannal) {
                res.status = (int)Respones.InvalidParams;
                return res;
            }

            // 检查用户名是否已存在
            Account dbAccount = AccountUnameCache.Instance.Get(req.uname);
            if (dbAccount != null) {
                res.status = (int)Respones.UnameIsExist;
                return res;
            }

            // 创建新账户
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