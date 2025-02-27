using Game.Core.Db;
using Game.Datas.GMEntities;
using Game.Datas.Messages;
using Game.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core.GM_Backpack
{
    /// <summary>
    /// GM_BackpackMgr 类是一个单例类，负责管理游戏中玩家背包的相关操作，
    /// 包括初始化日志记录器、获取背包数据、添加和减少物品、更新物品数量以及执行物品动作等。
    /// </summary>
    public class GM_BackpackMgr
    {
        /// <summary>
        /// GM_BackpackMgr 类的单例实例，方便全局访问。
        /// </summary>
        public static GM_BackpackMgr Instance = new GM_BackpackMgr();

        /// <summary>
        /// NLog 日志记录器，用于记录系统运行过程中的日志信息，便于后续的调试和监控。
        /// </summary>
        private NLog.Logger logger = null;

        /// <summary>
        /// 初始化方法，用于获取当前类的日志记录器，确保后续可以正常记录日志。
        /// </summary>
        public void Init()
        {
            this.logger = NLog.LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// 根据物品类型 ID 计算主类型 ID。
        /// 主类型 ID 是将类型 ID 除以 10000 后再乘以 10000 得到的结果，用于对物品进行分类。
        /// </summary>
        /// <param name="tid">物品类型 ID。</param>
        /// <returns>物品的主类型 ID。</returns>
        private int MainType(int tid)
        {
            return (tid / 10000) * 10000;
        }

        /// <summary>
        /// 从背包组件中获取背包数据，将物品按主类型进行分类。
        /// </summary>
        /// <param name="backpack">背包组件的引用，包含了玩家背包中的物品信息。</param>
        /// <returns>一个字典，键为物品主类型 ID，值为该主类型下的物品列表。</returns>
        public Dictionary<int, List<GoodsItem>> GetBackpackData(ref BackpackComponent backpack)
        {
            // 用于存储按主类型分类的物品集合
            var ret = new Dictionary<int, List<GoodsItem>>();
            // 遍历背包中所有物品的键（类型 ID）
            foreach (var k in backpack.packItems.Keys)
            {
                // 获取该类型 ID 对应的物品列表
                List<PackItem> items = backpack.packItems[k];
                for (int i = 0; i < items.Count; i++)
                {
                    // 获取单个物品
                    PackItem item = items[i];

                    // 计算物品的主类型 ID
                    int mainType = this.MainType(item.goods.tid);
                    // 创建一个新的 GoodsItem 对象，用于存储物品信息
                    GoodsItem goods = new GoodsItem();
                    goods.num = item.goods.num;
                    goods.typeId = item.goods.tid;
                    goods.strengData = item.goods.strengData;

                    // 用于存储同一主类型下的物品列表
                    List<GoodsItem> goodsSet = null;
                    if (ret.ContainsKey(mainType))
                    {
                        // 如果字典中已经包含该主类型，获取对应的物品列表
                        goodsSet = ret[mainType];
                    }
                    else
                    {
                        // 如果字典中不包含该主类型，创建一个新的物品列表并添加到字典中
                        goodsSet = new List<GoodsItem>();
                        ret.Add(mainType, goodsSet);
                    }
                    // 将物品添加到对应的物品列表中
                    goodsSet.Add(goods);
                }
            }
            return ret;
        }

        /// <summary>
        /// 根据物品类型 ID 向背包中添加指定数量的物品，并更新数据库。
        /// </summary>
        /// <param name="backpack">背包组件的引用，包含了玩家背包中的物品信息。</param>
        /// <param name="tid">物品类型 ID，用于标识物品的种类。</param>
        /// <param name="num">要添加的物品数量。</param>
        /// <param name="playerId">玩家 ID，用于关联物品所属的玩家。</param>
        /// <returns>如果添加成功返回 true，否则返回 false。</returns>
        private bool AddGoodsWithTid(ref BackpackComponent backpack, int tid, int num, long playerId)
        {
            if (backpack.packItems.ContainsKey(tid))
            {
                // 获取该类型 ID 对应的物品列表
                List<PackItem> items = backpack.packItems[tid];
                if (items.Count > 1)
                {
                    // 如果物品列表中物品数量大于 1，记录错误日志并返回 false
                    this.logger.Error($"tid:{tid} has multi PackItem");
                    return false;
                }

                // 增加物品数量，该操作是线程安全的
                items[0].goods.num += num;

                // 将物品信息更新到数据库
                DBService.Instance.GetGameInstance().Updateable<Game.Datas.DBEntities.Gamegoods>(items[0].goods).Where(it => it.id == items[0].goods.id).ExecuteCommandAsync();
            }
            else
            {
                // 如果背包中不存在该类型的物品，创建一个新的物品列表
                List<PackItem> items = new List<PackItem>();
                // 将物品列表添加到背包中
                backpack.packItems.Add(tid, items);
                // 创建一个新的 PackItem 对象
                PackItem item = new PackItem();
                item.goods = new Game.Datas.DBEntities.Gamegoods();
                item.goods.tid = tid;
                item.goods.num = num;
                item.goods.uid = playerId;
                item.strengData = null;
                // 将新物品添加到物品列表中
                items.Add(item);

                // 将新物品信息插入到数据库中
                DBService.Instance.GetGameInstance().Insertable<Game.Datas.DBEntities.Gamegoods>(item.goods).ExecuteCommandAsync();
            }
            return true;
        }

        /// <summary>
        /// 根据物品类型 ID 从背包中减少指定数量的物品，并更新数据库。
        /// </summary>
        /// <param name="backpack">背包组件的引用，包含了玩家背包中的物品信息。</param>
        /// <param name="tid">物品类型 ID，用于标识物品的种类。</param>
        /// <param name="num">要减少的物品数量。</param>
        /// <param name="playerId">玩家 ID，用于关联物品所属的玩家。</param>
        /// <returns>如果减少操作成功返回 true，否则返回 false。</returns>
        private bool DecGoodsWithTid(ref BackpackComponent backpack, int tid, int num, long playerId)
        {
            if (backpack.packItems.ContainsKey(tid))
            {
                // 获取该类型 ID 对应的物品列表
                List<PackItem> items = backpack.packItems[tid];
                if (items.Count > 1)
                {
                    // 如果物品列表中物品数量大于 1，记录错误日志并返回 false
                    this.logger.Error($"tid:{tid} has multi PackItem");
                    return false;
                }

                // 减少物品数量，该操作是线程安全的
                items[0].goods.num -= num;

                // 将物品信息更新到数据库
                DBService.Instance.GetGameInstance().Updateable<Game.Datas.DBEntities.Gamegoods>(items[0].goods).Where(it => it.id == items[0].goods.id).ExecuteCommandAsync();
            }

            return false;
        }

        /// <summary>
        /// 根据物品类型 ID 更新背包中物品的数量，可增加或减少物品。
        /// </summary>
        /// <param name="entity">玩家实体对象，包含了玩家的各种信息。</param>
        /// <param name="tid">物品类型 ID，用于标识物品的种类。</param>
        /// <param name="num">要更新的物品数量，正数表示增加，负数表示减少。</param>
        /// <returns>如果更新操作成功返回 true，否则返回 false。</returns>
        public bool UpdateGoodsWithTid(GM_PlayerEntity entity, int tid, int num)
        {
            // 验证 num 的合法性，若为正数则增加物品，负数则减少物品
            if (num > 0)
            {
                return this.AddGoodsWithTid(ref entity.uBackpack, tid, num, entity.uPlayer.playerInfo.id);
            }
            else
            {
                return DecGoodsWithTid(ref entity.uBackpack, tid, -num, entity.uPlayer.playerInfo.id);
            }
        }

        /// <summary>
        /// 根据物品类型 ID 对背包中的物品执行指定动作。
        /// </summary>
        /// <param name="entity">玩家实体对象，包含了玩家的各种信息。</param>
        /// <param name="tid">物品类型 ID，用于标识物品的种类。</param>
        /// <param name="actionName">要执行的动作名称。</param>
        /// <param name="param">执行动作所需的额外参数。</param>
        /// <returns>如果动作执行成功返回 true，否则返回 false。</returns>
        public bool DoGoodsActionWithTid(GM_PlayerEntity entity, int tid, string actionName, object param)
        {
            if (entity.uBackpack.packItems.ContainsKey(tid))
            {
                // 获取该类型 ID 对应的物品列表
                List<PackItem> items = entity.uBackpack.packItems[tid];
                if (items.Count > 1)
                {
                    // 如果物品列表中物品数量大于 1，记录错误日志并返回 false
                    this.logger.Error($"tid:{tid} has multi PackItem");
                    return false;
                }

                // 通过规则处理器工厂获取执行动作的方法
                MethodInfo m = RuleProcesserFactory.GetProcesser((int)RuleType.Backpack, actionName, tid, this.MainType(tid));
                if (m == null)
                {
                    // 如果未找到执行动作的方法，记录错误日志并返回 false
                    this.logger.Error($"tid:{tid} has action: {actionName} func is null");
                    return false;
                }

                // 调用执行动作的方法并返回结果
                return (bool)(m.Invoke(null, new object[] { entity, items[0], param }));
            }

            return false;
        }
    }
}