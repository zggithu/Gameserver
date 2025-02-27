using Framework.Core.Utils;
using Game.Core.Caches;
using Game.Core.GM_Backpack;
using Game.Datas.Excels;
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
    /// ExchangeTradingRules 类用于定义交换交易的规则。
    /// 使用 RuleModule 特性标记为交易规则模块，编号为 1000000。
    /// </summary>
    [RuleModule((int)RuleType.Trading, 1000000)]
    public class ExchangeTradingRules
    {
        /// <summary>
        /// 默认的交换交易检查方法，用于判断玩家是否可以进行指定产品的交换交易。
        /// 使用 RuleProcesser 特性标记为交换交易检查规则，规则编号为 -1。
        /// </summary>
        /// <param name="player">玩家实体对象，包含玩家的相关信息。</param>
        /// <param name="productId">要交换的产品 ID。</param>
        /// <returns>返回一个整数值，表示检查结果。具体含义由 Respones 枚举定义。</returns>
        [RuleProcesser("CanExchange", -1)]
        static int DefaultCanExchange(GM_PlayerEntity player, int productId)
        {
            // 从 Excel 配置文件中获取指定产品 ID 的交换交易配置信息
            ExchangeTrading config = (ExchangeTrading)ExcelUtils.GetConfigData<ExchangeTrading>(productId.ToString());
            // 如果配置信息为空，说明传入的产品 ID 无效，返回无效参数错误码
            if (config == null)
            {
                return (int)Respones.InvalidParams;
            }

            // 检查玩家的金钱是否足够支付该产品的价格
            if (player.uPlayer.playerInfo.umoney < config.Price)
            {
                // 如果金钱不足，返回金钱不足错误码
                return (int)Respones.MoneyIsNotEnough;
            }

            // 若以上条件都满足，返回操作成功的状态码
            return (int)Respones.OK;
        }

        /// <summary>
        /// 默认的交换交易执行方法，用于执行玩家对指定产品的交换交易操作。
        /// 使用 RuleProcesser 特性标记为交换交易执行规则，规则编号为 -1。
        /// </summary>
        /// <param name="player">玩家实体对象，包含玩家的相关信息。</param>
        /// <param name="productId">要交换的产品 ID。</param>
        /// <returns>如果交换交易执行成功返回 true，否则返回 false。</returns>
        [RuleProcesser("DoExchange", -1)]
        static bool DefaultDoExchange(GM_PlayerEntity player, int productId)
        {
            // 从 Excel 配置文件中获取指定产品 ID 的交换交易配置信息
            ExchangeTrading config = (ExchangeTrading)ExcelUtils.GetConfigData<ExchangeTrading>(productId.ToString());
            // 如果配置信息为空，说明传入的产品 ID 无效，返回交换失败
            if (config == null)
            {
                return false;
            }

            // 解析配置信息中的产品信息，将其转换为键值对字典
            Dictionary<string, int> products = GameUtils.ParseStringWithKeyIntValue(config.Product);
            // 如果解析后的产品信息为空，说明配置异常，返回交换失败
            if (products.Count <= 0)
            {
                return false;
            }

            // 从玩家的金钱中扣除该产品的价格
            player.uPlayer.playerInfo.umoney -= config.Price;
            // 将更新后的玩家信息更新到数据库
            PlayerIDCache.Instance.UpdateDataToDb(player.uPlayer.playerInfo);

            // 遍历解析后的产品信息字典
            foreach (var key in products.Keys)
            {
                // 获取产品数量
                int value = products[key];
                // 将产品类型 ID 从字符串转换为整数
                int typeId = int.Parse(key);

                // 注意多线程
                // 调用背包管理类的方法，更新玩家背包中指定类型产品的数量
                GM_BackpackMgr.Instance.UpdateGoodsWithTid(player, typeId, value);
            }

            // 若以上操作都成功执行，返回交换成功
            return true;
        }
    }
}