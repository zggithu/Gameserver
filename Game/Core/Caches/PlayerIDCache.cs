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
    /// <summary>
    /// 玩家 ID 缓存类，用于管理玩家信息的缓存与数据库交互。
    /// 继承自 BaseCacheSerivce，使用 long 类型的键和 Game.Datas.DBEntities.Player 类型的值。
    /// </summary>
    class PlayerIDCache : BaseCacheSerivce<long, Game.Datas.DBEntities.Player>
    {
        // 单例模式，确保整个应用中只有一个 PlayerIDCache 实例
        public static PlayerIDCache Instance = new PlayerIDCache();

        /// <summary>
        /// 初始化方法，目前为空，可用于后续添加初始化逻辑。
        /// </summary>
        public void Init()
        {
        }

        /// <summary>
        /// 重写基类的 Load 方法，根据玩家 ID 从数据库中加载玩家信息。
        /// </summary>
        /// <param name="playerID">要加载的玩家的 ID。</param>
        /// <returns>如果找到对应的玩家信息，则返回该玩家实体；否则返回 null。</returns>
        public override Game.Datas.DBEntities.Player Load(long playerID)
        {
            // 从游戏数据库中查询第一个满足 ID 等于传入参数的玩家实体
            return DBService.Instance.GetGameInstance().Queryable<Game.Datas.DBEntities.Player>().First(it => it.id == playerID);
        }

        /// <summary>
        /// 将玩家数据更新到数据库的方法。
        /// </summary>
        /// <param name="dbData">要更新的玩家数据实体。</param>
        public void UpdateDataToDb(Game.Datas.DBEntities.Player dbData)
        {
            // 异步更新数据库中指定玩家 ID 的玩家数据
            DBService.Instance.GetGameInstance().Updateable(dbData).Where(it => it.id == dbData.id).ExecuteCommandAsync();
        }
    }
}