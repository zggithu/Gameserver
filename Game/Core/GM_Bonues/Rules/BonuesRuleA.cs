using Framework.Core.Utils;
using Game.Core.Caches;
using Game.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * 如果你是要根据奖励的Typeid来奖励的规则计算，你可以将计算规则，放到这里;
 * 标记BonuesCalc,否则的话，你就可以放到外面去计算;
 * 给玩家 playerId ----> 一个typeId的奖励;  ===>根据玩家的等级,  金币，魔法,...
 * calcBonoues;
 */

/*
 * A类奖励，基本上就是直接奖励一些奖品,
 * 不是所有的奖励，都是奖励金币;
 */
namespace Game.Core.GM_Bonues.Rules
{
    /// <summary>
    /// BonuesRuleA 类用于处理游戏中 A 类奖励的规则计算与应用。
    /// 使用 [RuleModule] 特性标记为奖励规则模块，编号为 100000。
    /// </summary>
    [RuleModule((int)RuleType.Bonues, 100000)]
    public class BonuesRuleA
    {
        /// <summary>
        /// NLog 日志记录器，用于记录奖励处理过程中的日志信息。
        /// </summary>
        static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 默认的奖励应用方法，将奖励应用到玩家。
        /// 使用 [RuleProcesser] 特性标记，规则操作为 "Applay"，规则编号为 -1。
        /// </summary>
        /// <param name="bonues">奖励对象，包含奖励的相关信息。</param>
        [RuleProcesser("Applay", -1)]
        public static void DefaultBonuesApplayToPlayer(Game.Datas.DBEntities.Bonues bonues)
        {
            // 记录日志，表明正在执行默认奖励应用方法
            logger.Info("DefaultBonuesApplayToPlayer");
            // 根据奖励对象中的玩家 ID，从玩家 ID 缓存中获取玩家信息
            Game.Datas.DBEntities.Player playerInfo = PlayerIDCache.Instance.Get(bonues.uid);
            // 将奖励中的 bonues1 值添加到玩家的金币数量上
            playerInfo.ucoin += bonues.bonues1;
            // 将更新后的玩家信息更新到数据库
            PlayerIDCache.Instance.UpdateDataToDb(playerInfo);

            return;
        }

        /// <summary>
        /// 默认的奖励计算方法，根据玩家 ID 和奖励类型 ID 计算奖励。
        /// 使用 [RuleProcesser] 特性标记，规则操作为 "Calc"，规则编号为 -1。
        /// </summary>
        /// <param name="playerId">玩家的 ID。</param>
        /// <param name="typeId">奖励的类型 ID。</param>
        /// <param name="param">可选参数，用于传递额外的计算信息。</param>
        /// <returns>计算好的奖励对象。</returns>
        [RuleProcesser("Calc", -1)]
        public static Game.Datas.DBEntities.Bonues DefaultCaleBonuesToPlayer(long playerId, int typeId, object param = null)
        {
            Game.Datas.DBEntities.Bonues bonues = null;

            // 创建一个新的奖励对象
            bonues = new Game.Datas.DBEntities.Bonues();
            // 设置奖励对象的玩家 ID
            bonues.uid = playerId;
            // 设置奖励对象的状态为 0
            bonues.status = 0;
            // 设置奖励对象的类型 ID
            bonues.tid = typeId;
            // 设置奖励对象的时间为当前时间戳
            bonues.time = (int)UtilsHelper.Timestamp();
            // 设置奖励对象的结束时间为 -1，表示无结束时间
            bonues.endTime = -1;

            // 从 Excel 配置文件中获取对应类型 ID 的奖励规则配置项
            Game.Datas.Excels.BonuesRuleA configItem = (Game.Datas.Excels.BonuesRuleA)ExcelUtils.GetConfigData<Game.Datas.Excels.BonuesRuleA>(typeId.ToString());
            // 将参数值赋给奖励对象的 bonues1 属性
            bonues.bonues1 = (int)param;
            // 根据配置项中的描述格式和 bonues1 值生成奖励描述
            bonues.bonuesDesic = String.Format(configItem.desicFormat, bonues.bonues1);

            return bonues;
        }

        /// <summary>
        /// 特定类型（100002）的奖励计算方法，根据玩家 ID 和奖励类型 ID 计算奖励。
        /// 使用 [RuleProcesser] 特性标记，规则操作为 "Calc"，规则编号为 100002。
        /// </summary>
        /// <param name="playerId">玩家的 ID。</param>
        /// <param name="typeId">奖励的类型 ID。</param>
        /// <param name="param">可选参数，用于传递额外的计算信息。</param>
        /// <returns>计算好的奖励对象。</returns>
        [RuleProcesser("Calc", 100002)]
        public static Game.Datas.DBEntities.Bonues MoneyCaleBonuesToPlayer(long playerId, int typeId, object param = null)
        {
            Game.Datas.DBEntities.Bonues bonues = null;

            // 创建一个新的奖励对象
            bonues = new Game.Datas.DBEntities.Bonues();
            // 设置奖励对象的玩家 ID
            bonues.uid = playerId;
            // 设置奖励对象的状态为 0
            bonues.status = 0;
            // 设置奖励对象的类型 ID
            bonues.tid = typeId;
            // 设置奖励对象的时间为当前时间戳
            bonues.time = (int)UtilsHelper.Timestamp();
            // 设置奖励对象的结束时间为 -1，表示无结束时间
            bonues.endTime = -1;

            // 将参数值赋给奖励对象的 bonues1 属性
            bonues.bonues1 = (int)param;

            // 从 Excel 配置文件中获取对应类型 ID 的奖励规则配置项
            Game.Datas.Excels.BonuesRuleA configItem = (Game.Datas.Excels.BonuesRuleA)ExcelUtils.GetConfigData<Game.Datas.Excels.BonuesRuleA>(typeId.ToString());
            // 根据配置项中的描述格式和 bonues1 值生成奖励描述
            bonues.bonuesDesic = String.Format(configItem.desicFormat, bonues.bonues1);

            return bonues;
        }

        /// <summary>
        /// 特定类型（100002）的奖励应用方法，将奖励应用到玩家。
        /// 使用 [RuleProcesser] 特性标记，规则操作为 "Applay"，规则编号为 100002。
        /// </summary>
        /// <param name="bonues">奖励对象，包含奖励的相关信息。</param>
        [RuleProcesser("Applay", 100002)]
        public static void MonneyBonuesApplayToPlayer(Game.Datas.DBEntities.Bonues bonues)
        {
            // 记录日志，表明正在执行特定类型（100002）的奖励应用方法
            logger.Info("MonneyBonuesApplayToPlayer");
            // 根据奖励对象中的玩家 ID，从玩家 ID 缓存中获取玩家信息
            Game.Datas.DBEntities.Player playerInfo = PlayerIDCache.Instance.Get(bonues.uid);
            // 将奖励中的 bonues1 值添加到玩家的货币数量上
            playerInfo.umoney += bonues.bonues1;
            // 将更新后的玩家信息更新到数据库
            PlayerIDCache.Instance.UpdateDataToDb(playerInfo);

            return;
        }
    }
}