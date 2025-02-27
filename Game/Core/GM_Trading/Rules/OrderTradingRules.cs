using Game.Datas.GMEntities;
using Game.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core.GM_Trading
{
    /// <summary>
    /// OrderTradingRules 类用于定义订单交易的规则。
    /// 使用 RuleModule 特性标记为交易规则模块，编号为 2000000。
    /// </summary>
    [RuleModule((int)RuleType.Trading, 2000000)]
    public class OrderTradingRules
    {
        /// <summary>
        /// 默认的生成产品订单方法，用于为玩家生成指定产品的订单。
        /// 使用 RuleProcesser 特性标记为订单生成规则，规则编号为 -1。
        /// </summary>
        /// <param name="player">玩家实体对象，包含玩家的相关信息。</param>
        /// <param name="productId">要生成订单的产品 ID。</param>
        /// <param name="payType">支付类型。</param>
        /// <returns>返回生成的订单 ID，目前只是简单返回 0，需要根据实际需求实现具体逻辑。</returns>
        [RuleProcesser("GenOrder", -1)]
        static long DefaultGenProductOrder(GM_PlayerEntity player, int productId, int payType)
        {
            return 0;
        }

        /// <summary>
        /// 默认的订单交付方法，用于执行指定订单的交付操作。
        /// 使用 RuleProcesser 特性标记为订单交付规则，规则编号为 -1。
        /// </summary>
        /// <param name="player">玩家实体对象，包含玩家的相关信息。</param>
        /// <param name="orderId">要交付的订单 ID。</param>
        /// <returns>如果订单交付成功返回 true，否则返回 false，目前只是简单返回 false，需要根据实际需求实现具体逻辑。</returns>
        [RuleProcesser("OrderDelivery", -1)]
        static bool DefaultDoOrderDelivery(GM_PlayerEntity player, long orderId)
        {
            return false;
        }
    }
}