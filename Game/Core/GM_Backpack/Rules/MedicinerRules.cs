using Game.Datas.GMEntities;
using Game.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core.GM_Backpack
{
    /// <summary>
    /// MedicinerRules 类用于定义药品相关的规则处理方法。
    /// 该类使用 [RuleModule] 特性标记，表明它是一个规则模块，对应规则类型为背包规则，模块编号为 10000。
    /// </summary>
    [RuleModule((int)RuleType.Backpack, 10000)]
    public class MedicinerRules
    {
        /// <summary>
        /// 处理默认物品出售规则的方法。
        /// 使用 [RuleProcesser] 特性标记，规则操作为 "SellOut"，规则编号为 -1。
        /// 该方法接收玩家实体、背包物品和额外参数作为输入，目前返回 false，表示默认不允许出售。
        /// </summary>
        /// <param name="player">玩家实体对象。</param>
        /// <param name="item">背包中的物品。</param>
        /// <param name="param">额外的参数对象。</param>
        /// <returns>返回一个布尔值，表示是否允许出售该物品，当前固定返回 false。</returns>
        [RuleProcesser("SellOut", -1)]
        static bool DefaultGoodsSellOut(GM_PlayerEntity player, PackItem item, object param)
        {
            return false;
        }

        /// <summary>
        /// 处理编号为 10004 的大蓝物品出售规则的方法。
        /// 使用 [RuleProcesser] 特性标记，规则操作为 "SellOut"，规则编号为 10004。
        /// 该方法接收玩家实体、背包物品和额外参数作为输入，目前返回 false，表示不允许出售该特定物品。
        /// </summary>
        /// <param name="player">玩家实体对象。</param>
        /// <param name="item">背包中的物品。</param>
        /// <param name="param">额外的参数对象。</param>
        /// <returns>返回一个布尔值，表示是否允许出售该特定物品，当前固定返回 false。</returns>
        [RuleProcesser("SellOut", 10004)] // 10004
        static bool BigBlueGoodsSellOut(GM_PlayerEntity player, PackItem item, object param)
        {
            return false;
        }

        /// <summary>
        /// 处理默认物品买入规则的方法。
        /// 使用 [RuleProcesser] 特性标记，规则操作为 "BuyIn"，规则编号为 -1。
        /// 该方法接收玩家实体、背包物品和额外参数作为输入，目前返回 false，表示默认不允许买入。
        /// </summary>
        /// <param name="player">玩家实体对象。</param>
        /// <param name="item">背包中的物品。</param>
        /// <param name="param">额外的参数对象。</param>
        /// <returns>返回一个布尔值，表示是否允许买入该物品，当前固定返回 false。</returns>
        [RuleProcesser("BuyIn", -1)]
        static bool DefaultGoodsBuyIn(GM_PlayerEntity player, PackItem item, object param)
        {
            return false;
        }

        /// <summary>
        /// 处理默认物品装备规则的方法。
        /// 使用 [RuleProcesser] 特性标记，规则操作为 "PutOn"，规则编号为 -1。
        /// 该方法接收玩家实体、背包物品和额外参数作为输入，目前返回 false，表示默认不允许装备。
        /// </summary>
        /// <param name="player">玩家实体对象。</param>
        /// <param name="item">背包中的物品。</param>
        /// <param name="param">额外的参数对象。</param>
        /// <returns>返回一个布尔值，表示是否允许装备该物品，当前固定返回 false。</returns>
        [RuleProcesser("PutOn", -1)]
        static bool DefaultGoodsPutOn(GM_PlayerEntity player, PackItem item, object param)
        {
            return false;
        }
    }
}