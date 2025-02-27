using Framework.Core.Cache;
using Framework.Core.Utils;
using Game.Datas.DBEntities;
using Game.Datas.Messages;
using Game.Core.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core.Caches
{
    /// <summary>
    /// 玩家账户 ID 缓存类，用于管理玩家账户相关的缓存操作。
    /// 继承自 BaseCacheSerivce，使用 long 类型的键和 Player 类型的值。
    /// </summary>
    class PlayerAcountIDCache : BaseCacheSerivce<long, Game.Datas.DBEntities.Player>
    {
        // 单例实例，确保整个应用程序中只有一个 PlayerAcountIDCache 实例。
        public static PlayerAcountIDCache Instance = new PlayerAcountIDCache();

        /// <summary>
        /// 初始化方法，目前为空，可用于后续添加初始化逻辑。
        /// </summary>
        public void Init()
        {
        }

        /// <summary>
        /// 重写基类的 Load 方法，根据组合键（账号 ID 和职业信息）从数据库加载玩家信息。
        /// </summary>
        /// <param name="accountIdAndJob">组合键，包含账号 ID 和职业信息。</param>
        /// <returns>从数据库中查询到的玩家信息，如果未找到则返回 null。</returns>
        public override Game.Datas.DBEntities.Player Load(long accountIdAndJob)
        {
            // 从组合键中提取账号 ID
            long accoutID = (accountIdAndJob >> 3);
            // 从组合键中提取职业信息
            long job = accountIdAndJob & 0x07;
            // 从游戏数据库中查询与账号 ID 匹配的第一条玩家记录
            return DBService.Instance.GetGameInstance().Queryable<Game.Datas.DBEntities.Player>().First(it => it.accountId == accoutID);
        }

        /// <summary>
        /// 根据账号 ID 和职业信息生成缓存键。
        /// </summary>
        /// <param name="id">账号 ID。</param>
        /// <param name="job">职业信息。</param>
        /// <returns>生成的缓存键。</returns>
        public long Key(long id, int job)
        {
            // 这里实现一个账号只允许一个职业的逻辑，将账号 ID 左移 3 位
            long key = (id << 3) | ((long)0);
            // 如果需要支持一个账号多个职业，可以使用以下代码
            // long key = (id << 3) | ((long)job);
            return key;
        }

        /// <summary>
        /// 根据账号 ID 和玩家选择请求获取或创建玩家信息。
        /// 如果玩家信息已存在于缓存中，则直接返回；否则创建新的玩家信息并保存到数据库和缓存中。
        /// </summary>
        /// <param name="accountId">账号 ID。</param>
        /// <param name="req">玩家选择请求，包含职业、用户名、性别等信息。</param>
        /// <returns>玩家信息实体。</returns>
        public Game.Datas.DBEntities.Player GetOrCreate(long accountId, ReqSelectPlayer req)
        {
            // 生成缓存键
            long accountIdAndJob = this.Key(accountId, req.job);
            // 从缓存中获取玩家信息
            Game.Datas.DBEntities.Player dbPlayer = this.Get(accountIdAndJob);
            if (dbPlayer != null)
            {
                // 如果缓存中存在玩家信息，直接返回
                return dbPlayer;
            }

            // 后期可以将构造默认数据的逻辑封装成一个函数，根据游戏配置文件决定初始化参数。
            // 创建新的玩家信息实体
            dbPlayer = new Game.Datas.DBEntities.Player();
            // 生成唯一的玩家 ID
            dbPlayer.id = IdGenerator.GetNextId();
            // 设置玩家所属的账号 ID
            dbPlayer.accountId = accountId;
            // 设置玩家的职业
            dbPlayer.job = (byte)req.job;
            // 设置玩家的用户名
            dbPlayer.name = req.uname;
            // 设置玩家的初始生命值
            dbPlayer.HP = 100;
            // 设置玩家的初始魔法值
            dbPlayer.MP = 0;
            // 设置玩家的状态
            dbPlayer.status = 0;
            // 设置玩家的上次每日重置时间
            dbPlayer.lastDailyReset = 0;
            // 设置玩家的初始等级
            dbPlayer.level = 1;
            // 设置玩家的初始金币数量
            dbPlayer.ucoin = 100;
            // 设置玩家的初始货币数量
            dbPlayer.umoney = 0;
            // 再次设置玩家的状态
            dbPlayer.status = 0;
            // 设置玩家的性别
            dbPlayer.usex = req.usex;
            // 设置玩家的 VIP 权限 JSON 信息
            dbPlayer.vipRightJson = "";
            // 以下代码注释掉，可能是多余的或未使用的逻辑
            // dbPlayer.usex = req.usex;
            // dbPlayer.charactorId = req.charactorId;

            // 异步将新玩家信息插入游戏数据库
            DBService.Instance.GetGameInstance().Insertable(dbPlayer).ExecuteCommandAsync();
            // 将新玩家信息存入缓存
            this.Put(accountIdAndJob, dbPlayer);
            // 将玩家 ID 与玩家信息的映射存入 PlayerIDCache
            PlayerIDCache.Instance.Put(dbPlayer.id, dbPlayer);

            return dbPlayer;
        }

        /// <summary>
        /// 尝试根据账号 ID 和职业信息获取玩家信息。
        /// 如果玩家信息存在于缓存中，则返回该信息；否则返回 null。
        /// </summary>
        /// <param name="accountId">账号 ID。</param>
        /// <param name="job">职业信息。</param>
        /// <returns>玩家信息实体，如果未找到则返回 null。</returns>
        public Game.Datas.DBEntities.Player TryGetPlayer(long accountId, int job)
        {
            // 生成缓存键
            long accountIdAndJob = this.Key(accountId, job);
            // 从缓存中获取玩家信息
            Game.Datas.DBEntities.Player dbPlayer = this.Get(accountIdAndJob);
            if (dbPlayer != null)
            {
                // 如果缓存中存在玩家信息，直接返回
                return dbPlayer;
            }

            return null;
        }
    }
}