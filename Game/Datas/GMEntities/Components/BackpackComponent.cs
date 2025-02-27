using Game.Core.Db;
using System.Collections.Generic;

namespace Game.Datas.GMEntities
{
    /// <summary>
    /// 表示游戏背包中的单个物品
    /// </summary>
    public class PackItem
    {
        /// <summary>
        /// 物品的基本信息，来源于数据库中的 Gamegoods 表
        /// </summary>
        public Game.Datas.DBEntities.Gamegoods goods = null;

        /// <summary>
        /// 如果有强化数据，存储 protobuf 解码以后的对象，可参考任务系统
        /// </summary>
        public object strengData = null;
    }

    /// <summary>
    /// 表示游戏中的背包组件，负责管理玩家背包内的物品
    /// </summary>
    public struct BackpackComponent
    {
        // 注意线程安全，此问题后续处理
        /// <summary>
        /// 物品 ID 到物品数据对象列表的映射
        /// 例如：20001 ==> [20001 实例 1(强化数据 1), 20001 实例 2(强化数据 2)]
        /// </summary>
        public Dictionary<int, List<PackItem>> packItems;

        /// <summary>
        /// 从数据库中加载指定玩家的物品数据
        /// </summary>
        /// <param name="playerId">玩家的 ID</param>
        /// <returns>返回该玩家的物品数据数组</returns>
        private static Game.Datas.DBEntities.Gamegoods[] LoadDataFromDb(long playerId) {
            return DBService.Instance.GetGameInstance().Queryable<Game.Datas.DBEntities.Gamegoods>().Where(it => it.uid == playerId).ToArray();
        }

        /// <summary>
        /// 初始化玩家的背包数据
        /// </summary>
        /// <param name="entity">包含玩家信息和背包信息的实体</param>
        public static void Init(GM_PlayerEntity entity) {
            // 获取当前类的日志记录器
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

            // 初始化玩家背包的物品映射字典
            entity.uBackpack.packItems = new Dictionary<int, List<PackItem>>();

            // 从数据库加载玩家的物品数据
            Game.Datas.DBEntities.Gamegoods[] gameGoods = LoadDataFromDb(entity.uPlayer.playerInfo.id);
            // 如果没有加载到物品数据，则直接返回
            if (gameGoods == null || gameGoods.Length <= 0) {
                return;
            }

            // 遍历加载到的物品数据
            for (int i = 0; i < gameGoods.Length; i++) {
                // 创建一个新的 PackItem 对象
                PackItem item = new PackItem();
                // 将数据库中的物品信息赋值给 PackItem 对象
                item.goods = gameGoods[i];
                // 初始化强化数据为 null，后续可通过规则模块(StrenRules)进行编码解码
                item.strengData = null;
                List<PackItem> goodsSet = null;
                // 如果背包中已经存在该物品 ID 的列表
                if (entity.uBackpack.packItems.ContainsKey(gameGoods[i].tid)) {
                    // 获取该物品 ID 对应的物品列表
                    goodsSet = entity.uBackpack.packItems[gameGoods[i].tid];
                    // 将新的物品添加到列表中
                    goodsSet.Add(item);
                } else {
                    // 创建一个新的物品列表
                    goodsSet = new List<PackItem>();
                    // 将物品 ID 和新的物品列表添加到背包的映射字典中
                    entity.uBackpack.packItems.Add(gameGoods[i].tid, goodsSet);
                    // 将新的物品添加到列表中
                    goodsSet.Add(item);
                }
            }
        }
    }
}