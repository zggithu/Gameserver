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

namespace Game.Core.GM_Trading
{
    /// <summary>
    /// GM_TradingMgr 类是一个单例类，用于管理游戏中的交易操作。
    /// 提供了交换交易和订单交易相关功能的调用方法。
    /// </summary>
    public class GM_TradingMgr
    {
        /// <summary>
        /// GM_TradingMgr 类的单例实例，方便全局访问。
        /// </summary>
        public static GM_TradingMgr Instance = new GM_TradingMgr();

        /// <summary>
        /// NLog 日志记录器，用于记录交易操作过程中的日志信息。
        /// </summary>
        private NLog.Logger logger = null;

        /// <summary>
        /// 初始化方法，用于初始化日志记录器。
        /// </summary>
        public void Init()
        {
            // 获取当前类的日志记录器
            this.logger = NLog.LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// 根据产品 ID 计算主类型。
        /// 主类型是产品 ID 除以 1000000 后取整再乘以 1000000 的结果。
        /// </summary>
        /// <param name="tid">产品 ID。</param>
        /// <returns>产品的主类型。</returns>
        private int MainType(int tid)
        {
            return (tid / 1000000) * 1000000;
        }

        /// <summary>
        /// 从规则处理函数映射中获取指定名称的处理函数。
        /// 优先从自定义函数映射中查找，如果找不到则从默认函数映射中查找。
        /// </summary>
        /// <param name="funcName">处理函数的名称。</param>
        /// <param name="funMap">自定义处理函数映射。</param>
        /// <param name="defaultFunMap">默认处理函数映射。</param>
        /// <returns>找到的处理函数的 MethodInfo 对象，如果未找到则返回 null。</returns>
        private MethodInfo GetProcesserFunc(string funcName, Dictionary<string, MethodInfo> funMap, Dictionary<string, MethodInfo> defaultFunMap)
        {
            if (funMap.ContainsKey(funcName))
            {
                return funMap[funcName];
            }
            if (defaultFunMap.ContainsKey(funcName))
            {
                return defaultFunMap[funcName];
            }

            return null;
        }

        /// <summary>
        /// 检查玩家是否可以交换指定产品。
        /// 通过 RuleProcesserFactory 获取对应的处理方法并调用。
        /// </summary>
        /// <param name="player">玩家实体对象。</param>
        /// <param name="productId">产品 ID。</param>
        /// <returns>返回一个整数值，表示检查结果，具体含义由 Respones 枚举定义。</returns>
        public int CanExchangeProduct(GM_PlayerEntity player, int productId)
        {
            // 通过规则处理器工厂获取 CanExchange 处理方法
            MethodInfo m = RuleProcesserFactory.GetProcesser((int)RuleType.Trading, "CanExchange", productId, this.MainType(productId));
            if (m == null)
            {
                // 若未找到处理方法，记录错误日志并返回无效参数错误码
                this.logger.Error($"tid:{productId} has action: CanExchange func is null");
                return (int)Respones.InvalidParams;
            }

            // 调用处理方法并获取返回结果
            int ret = (int)(m.Invoke(null, new object[] { player, productId }));

            return ret;
        }

        /// <summary>
        /// 执行玩家交换指定产品的操作。
        /// 通过 RuleProcesserFactory 获取对应的处理方法并调用。
        /// </summary>
        /// <param name="player">玩家实体对象。</param>
        /// <param name="productId">产品 ID。</param>
        public void DoExchangeProduct(GM_PlayerEntity player, int productId)
        {
            // 通过规则处理器工厂获取 DoExchange 处理方法
            MethodInfo m = RuleProcesserFactory.GetProcesser((int)RuleType.Trading, "DoExchange", productId, this.MainType(productId));

            if (m == null)
            {
                // 若未找到处理方法，记录错误日志并返回
                this.logger.Error($"tid:{productId} has action: DoExchange func is null");
                return;
            }

            // 调用处理方法并获取返回结果
            bool ret = (bool)(m.Invoke(null, new object[] { player, productId }));
            if (!ret)
            {
                // 若处理方法返回 false，记录错误日志
                this.logger.Error($"tid:{productId} do action: DoExchange return false !");
            }
        }

        /// <summary>
        /// 为玩家生成指定产品的订单。
        /// 通过 RuleProcesserFactory 获取对应的处理方法并调用。
        /// </summary>
        /// <param name="player">玩家实体对象。</param>
        /// <param name="productId">产品 ID。</param>
        /// <param name="payType">支付类型。</param>
        /// <returns>生成的订单 ID，如果处理方法未找到则返回 0。</returns>
        public long GenProductOrder(GM_PlayerEntity player, int productId, int payType)
        {
            // 通过规则处理器工厂获取 GenOrder 处理方法
            MethodInfo m = RuleProcesserFactory.GetProcesser((int)RuleType.Trading, "GenOrder", productId, this.MainType(productId));

            if (m == null)
            {
                // 若未找到处理方法，记录错误日志并返回 0
                this.logger.Error($"tid:{productId} has action: GenOrder func is null");
                return 0;
            }

            // 调用处理方法并返回生成的订单 ID
            return (long)(m.Invoke(null, new object[] { player, productId, payType }));
        }

        /// <summary>
        /// 处理玩家订单的交付操作。
        /// 通过 RuleProcesserFactory 获取对应的处理方法并调用。
        /// </summary>
        /// <param name="player">玩家实体对象。</param>
        /// <param name="productId">产品 ID。</param>
        /// <param name="orderId">订单 ID。</param>
        public void UserOrderDelivery(GM_PlayerEntity player, int productId, long orderId)
        {
            // 通过规则处理器工厂获取 OrderDelivery 处理方法
            MethodInfo m = RuleProcesserFactory.GetProcesser((int)RuleType.Trading, "OrderDelivery", productId, this.MainType(productId));

            if (m == null)
            {
                // 若未找到处理方法，记录错误日志并返回
                this.logger.Error($"tid:{productId} has action: GenOrder func is null");
                return;
            }

            // 调用处理方法并获取返回结果
            bool ret = (bool)(m.Invoke(null, new object[] { player, productId, orderId }));
            if (!ret)
            {
                // 若处理方法返回 false，记录错误日志
                this.logger.Error($"orderId:{orderId} OrderDelivery: Failed !");
            }
        }
    }
}