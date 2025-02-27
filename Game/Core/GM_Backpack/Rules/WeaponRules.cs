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
    /// WeaponRules 类用于定义武器相关的规则处理方法。
    /// 该类使用 [RuleModule] 特性标记，表明它是一个规则模块，对应规则类型为背包规则，模块编号为 30000。
    /// </summary>
    [RuleModule((int)RuleType.Backpack, 30000)]
    public class WeaponRules
    {
        /// <summary>
        /// 处理默认武器出售规则的方法。
        /// 使用 [RuleProcesser] 特性标记，规则操作为 "SellOut"，规则编号为 -1。
        /// 该方法接收玩家实体、背包物品和额外参数作为输入，目前返回 false，表示默认不允许出售武器。
        /// </summary>
        /// <param name="player">玩家实体对象。</param>
        /// <param name="item">背包中的武器物品。</param>
        /// <param name="param">额外的参数对象。</param>
        /// <returns>返回一个布尔值，表示是否允许出售该武器，当前固定返回 false。</returns>
        [RuleProcesser("SellOut", -1)]
        static bool DefaultGoodsSellOut(GM_PlayerEntity player, PackItem item, object param)
        {
            return false;
        }

        /// <summary>
        /// 处理默认武器装备规则的方法。
        /// 使用 [RuleProcesser] 特性标记，规则操作为 "PutOn"，规则编号为 -1。
        /// 该方法接收玩家实体、背包物品和额外参数作为输入，目前返回 false，表示默认不允许装备武器。
        /// </summary>
        /// <param name="player">玩家实体对象。</param>
        /// <param name="item">背包中的武器物品。</param>
        /// <param name="param">额外的参数对象。</param>
        /// <returns>返回一个布尔值，表示是否允许装备该武器，当前固定返回 false。</returns>
        [RuleProcesser("PutOn", -1)]
        static bool DefaultGoodsPutOn(GM_PlayerEntity player, PackItem item, object param)
        {
            return false;
        }
    }
}