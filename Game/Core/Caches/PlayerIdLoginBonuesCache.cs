using Framework.Core.Cache;
using Framework.Core.Utils;
using Game.Datas.DBEntities;
using Game.Core.Db;

namespace Game.Core.Caches
{
    /// <summary>
    /// 玩家登录奖励缓存类，用于管理玩家登录奖励信息的缓存与数据库交互。
    /// 继承自 BaseCacheSerivce，使用 long 类型的键（玩家 ID）和 Game.Datas.DBEntities.Loginbonues 类型的值（登录奖励实体）。
    /// </summary>
    class PlayerIdLoginBonuesCache : BaseCacheSerivce<long, Game.Datas.DBEntities.Loginbonues>
    {
        // 单例模式，确保整个应用中只有一个 PlayerIdLoginBonuesCache 实例
        public static PlayerIdLoginBonuesCache Instance = new PlayerIdLoginBonuesCache();

        /// <summary>
        /// 初始化方法，目前为空，可用于后续添加初始化逻辑。
        /// </summary>
        public void Init()
        {
        }

        /// <summary>
        /// 重写基类的 Load 方法，根据玩家 ID 从数据库中加载登录奖励信息。
        /// </summary>
        /// <param name="playerID">要加载登录奖励信息的玩家 ID。</param>
        /// <returns>如果找到对应的登录奖励信息，则返回该登录奖励实体；否则返回 null。</returns>
        public override Loginbonues Load(long playerID)
        {
            // 从游戏数据库中查询第一个满足 uid 等于传入玩家 ID 的登录奖励实体
            return DBService.Instance.GetGameInstance().Queryable<Game.Datas.DBEntities.Loginbonues>().First(it => it.uid == playerID);
        }

        /// <summary>
        /// 将登录奖励数据更新到数据库的方法。
        /// </summary>
        /// <param name="dbData">要更新的登录奖励数据实体。</param>
        public void UpdateDataToDb(Game.Datas.DBEntities.Loginbonues dbData)
        {
            // 异步更新数据库中指定玩家 ID 的登录奖励数据
            DBService.Instance.GetGameInstance().Updateable(dbData).Where(it => it.uid == dbData.uid).ExecuteCommandAsync();
        }

        /// <summary>
        /// 根据玩家 ID 获取或创建登录奖励信息的方法。
        /// 如果登录奖励信息已存在于缓存中，则直接返回；否则创建新的登录奖励信息并保存到数据库和缓存中。
        /// </summary>
        /// <param name="playerId">玩家 ID。</param>
        /// <returns>登录奖励信息实体。</returns>
        public Game.Datas.DBEntities.Loginbonues GetOrCreate(long playerId)
        {
            // 从缓存中获取玩家的登录奖励信息
            Game.Datas.DBEntities.Loginbonues dbLoginBonues = this.Get(playerId);
            if (dbLoginBonues != null)
            {
                // 如果缓存中存在登录奖励信息，直接返回
                return dbLoginBonues;
            }

            // 后期可将构造默认数据的逻辑封装成一个函数，根据游戏配置文件决定初始化参数
            // 创建新的登录奖励信息实体
            dbLoginBonues = new Game.Datas.DBEntities.Loginbonues();
            // 注释掉的代码可能用于生成唯一 ID，但此处可能不需要
            // dbLoginBonues.id = IdGenerator.GetNextId();
            // 设置登录奖励信息所属的玩家 ID
            dbLoginBonues.uid = playerId;
            // 初始化奖励数量为 0
            dbLoginBonues.bonues = 0;
            // 初始化奖励状态为 1
            dbLoginBonues.status = 1;
            // 设置奖励时间为当前时间戳
            dbLoginBonues.bonues_time = (int)(UtilsHelper.Timestamp());
            // 初始化连续登录天数为 0
            dbLoginBonues.days = 0;

            // 异步将新的登录奖励信息插入游戏数据库
            DBService.Instance.GetGameInstance().Insertable(dbLoginBonues).ExecuteCommandAsync();
            // 将新的登录奖励信息存入缓存
            this.Put(playerId, dbLoginBonues);

            return dbLoginBonues;
        }
    }
}