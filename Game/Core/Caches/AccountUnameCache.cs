using Framework.Core.Cache;
using Framework.Core.Utils;
using Game.Datas.DBEntities;
using Game.Datas.Messages;
using Game.Core.Db;
using Game.Utils;

namespace Game.Core.Caches
{
    public class AccountUnameCache : BaseCacheSerivce<string, Account>
    {
        // 单例模式，确保整个应用中只有一个 AccountUnameCache 实例
        public static AccountUnameCache Instance = new AccountUnameCache();
        // 日志记录器，用于记录程序运行过程中的信息
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        // 初始化方法，目前为空，可用于后续添加初始化逻辑
        public void Init()
        {
        }

        // 重写基类的 Load 方法，根据用户名从数据库中加载账户信息
        public override Account Load(string uname)
        {
            return DBService.Instance.GetAuthInstance().Queryable<Account>().First(it => it.uname == uname);
        }

        // 随机生成用户名对应的昵称，格式为“用户XXXX”，XXXX 是 1000 到 9999 之间的随机数
        private string RandUnameNick()
        {
            return "用户" + GameUtils.Random(1000, 10000);
        }

        // 根据注册请求获取或创建账户的方法
        public Account GetOrCreate(ReqRegisterUser req)
        {
            // 先从缓存中获取账户信息
            Account dbAccount = Get(req.uname);
            if (dbAccount != null)
            {
                // 如果缓存中存在账户信息，直接返回
                return dbAccount;
            }

            // 若缓存中不存在，则创建新的账户对象
            dbAccount = new Account();
            // 生成唯一的用户 ID
            dbAccount.uid = IdGenerator.GetNextId();
            // 非游客账户，游客密钥为空
            dbAccount.guest_key = "";
            // 标记为非游客账户
            dbAccount.is_guest = 0;
            // 设置渠道号
            dbAccount.uchannel = req.channal;

            // 随机生成用户昵称
            dbAccount.unick = this.RandUnameNick();
            // 随机生成用户头像编号
            dbAccount.uface = GameUtils.Random(0, 6);
            // 根据头像编号设置用户性别
            dbAccount.usex = (dbAccount.uface < 3) ? 1 : 0;
            // 设置账户状态为 0
            dbAccount.status = 0;
            // 设置用户名
            dbAccount.uname = req.uname;
            // 对用户密码进行 MD5 加密
            dbAccount.upwd = UtilsHelper.Md5(req.upwd);

            // 异步将新账户信息插入数据库
            DBService.Instance.GetAuthInstance().Insertable(dbAccount).ExecuteCommandAsync();

            // 将新账户信息存入缓存
            this.Put(req.uname, dbAccount);

            // 添加 uid 到账户的映射到 AccountIDCache 中
            AccountIDCache.Instance.Put(dbAccount.uid, dbAccount);

            return dbAccount;
        }
    }
}