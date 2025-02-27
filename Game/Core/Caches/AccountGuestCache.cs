using Framework.Core.Cache;
using Framework.Core.Utils;
using Game.Datas.DBEntities;
using Game.Core.Db;
using Game.Utils;

namespace Game.Core.Caches
{
    public class AccountGuestCache : BaseCacheSerivce<string, Account>
    {
        // 单例模式，确保整个应用中只有一个 AccountGuestCache 实例
        public static AccountGuestCache Instance = new AccountGuestCache();
        // 日志记录器，用于记录程序运行过程中的信息
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        // 初始化方法，目前为空，可用于后续添加初始化逻辑
        public void Init()
        {
        }

        // 重写基类的 Load 方法，根据游客密钥从数据库中加载账户信息
        public override Account Load(string guestKey)
        {
            return DBService.Instance.GetAuthInstance().Queryable<Account>().First(it => it.guest_key == guestKey);
        }

        // 随机生成游客昵称的方法，格式为“游客XXXX”，XXXX 是 1000 到 9999 之间的随机数
        private string RandGuestNick()
        {
            return "游客" + GameUtils.Random(1000, 10000);
        }

        // 根据游客密钥和渠道号获取或创建游客账户的方法
        public Account GetOrCreate(string guestKey, int channal)
        {
            // 先从缓存中获取账户信息
            Account dbAccount = Get(guestKey);
            if (dbAccount != null)
            {
                // 如果缓存中存在账户信息，直接返回
                return dbAccount;
            }

            // 若缓存中不存在，则创建新的账户对象
            dbAccount = new Account();
            // 生成唯一的用户 ID
            dbAccount.uid = IdGenerator.GetNextId();
            // 设置游客密钥
            dbAccount.guest_key = guestKey;
            // 标记为游客账户
            dbAccount.is_guest = 1;
            // 设置渠道号
            dbAccount.uchannel = channal;

            // 随机生成游客昵称
            dbAccount.unick = this.RandGuestNick();
            // 随机生成用户头像编号
            dbAccount.uface = GameUtils.Random(0, 6);
            // 根据头像编号设置用户性别
            dbAccount.usex = (dbAccount.uface < 3) ? 1 : 0;
            // 设置账户状态为 0
            dbAccount.status = 0;

            // 异步将新账户信息插入数据库
            DBService.Instance.GetAuthInstance().Insertable(dbAccount).ExecuteCommandAsync();

            // 将新账户信息存入缓存
            this.Put(guestKey, dbAccount);

            // 添加 uid 到账户的映射到 AccountIDCache 中
            AccountIDCache.Instance.Put(dbAccount.uid, dbAccount);

            return dbAccount;
        }
    }
}