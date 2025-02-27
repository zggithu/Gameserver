using Framework.Core.Cache;
using Game.Datas.DBEntities;
using Game.Core.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core.Caches
{
    class AccountIDCache : BaseCacheSerivce<long, Account>
    {
        // 单例模式，确保整个应用中只有一个 AccountIDCache 实例
        public static AccountIDCache Instance = new AccountIDCache();

        // 初始化方法，目前为空，可用于后续添加初始化逻辑
        public void Init()
        {
        }

        // 重写基类的 Load 方法，根据账户 ID 从数据库中加载账户信息
        public override Account Load(long accountID)
        {
            // 查询单条记录，根据账户 ID 查找对应的账户信息
            return DBService.Instance.GetAuthInstance().Queryable<Account>().First(it => it.uid == accountID);
        }

        // 将账户信息更新到数据库的方法
        public void UpdateAccountToDatabase(Account dbAccount)
        {
            // 异步更新数据库中指定账户 ID 的账户信息
            DBService.Instance.GetAuthInstance().Updateable(dbAccount).Where(it => it.uid == dbAccount.uid).ExecuteCommandAsync();
        }
    }
}