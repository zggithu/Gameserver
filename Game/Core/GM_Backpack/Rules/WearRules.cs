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
    /// WearRules 类用于处理游戏中穿戴物品（如衣物等）在背包相关操作的规则。
    /// 它被标记为一个规则模块，规则类型属于背包规则，模块编号为 20000。
    /// </summary>
    [RuleModule((int)RuleType.Backpack, 20000)]
    public class WearRules
    {
        /// <summary>
        /// 处理默认穿戴物品出售规则的方法。
        /// 该方法使用 [RuleProcesser] 特性标记，规则操作为 "SellOut"，规则编号为 -1。
        /// 方法接收玩家实体、背包中的穿戴物品以及额外参数作为输入。
        /// 当前逻辑下，此方法固定返回 false，表示默认不允许出售该穿戴物品。
        /// </summary>
        /// <param name="player">玩家实体对象，包含玩家的相关信息。</param>
        /// <param name="item">背包中的穿戴物品对象。</param>
        /// <param name="param">额外的参数对象，可用于传递一些规则判断所需的额外信息。</param>
        /// <returns>一个布尔值，指示是否允许出售该穿戴物品，当前总是返回 false。</returns>
        [RuleProcesser("SellOut", -1)]  // 20000 + (-1)
        static bool DefaultGoodsSellOut(GM_PlayerEntity player, PackItem item, object param)
        {
            return false;
        }

        /// <summary>
        /// 处理默认穿戴物品装备规则的方法。
        /// 该方法使用 [RuleProcesser] 特性标记，规则操作为 "PutOn"，规则编号为 -1。
        /// 方法接收玩家实体、背包中的穿戴物品以及额外参数作为输入。
        /// 当前逻辑下，此方法固定返回 false，表示默认不允许装备该穿戴物品。
        /// </summary>
        /// <param name="player">玩家实体对象，包含玩家的相关信息。</param>
        /// <param name="item">背包中的穿戴物品对象。</param>
        /// <param name="param">额外的参数对象，可用于传递一些规则判断所需的额外信息。</param>
        /// <returns>一个布尔值，指示是否允许装备该穿戴物品，当前总是返回 false。</returns>
        [RuleProcesser("PutOn", -1)]
        static bool DefaultGoodsPutOn(GM_PlayerEntity player, PackItem item, object param)
        {
            return false;
        }
    }
}