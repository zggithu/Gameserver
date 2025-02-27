using Framework.Core.Utils;
using Game.Datas.DBEntities;
using Game.Core.Db;
using System;
using System.Collections.Generic;
using System.Reflection;
using Game.Utils;


/*
 * 奖励类型的ID编码:  100000类 100001  ---> 100000类的第1种,  ....;
 * 200000类200001---->200000的第1种，依此类推;
 */

namespace Game.Core.GM_Bonues
{
    /// <summary>
    /// GM_BonuesMgr 类是一个单例类，负责管理游戏中奖励的生成、应用、接收等操作。
    /// 提供了与奖励相关的一系列方法，包括生成奖励、将奖励应用到玩家、接收奖励等。
    /// </summary>
    public class GM_BonuesMgr
    {
        /// <summary>
        /// GM_BonuesMgr 类的单例实例，方便全局访问。
        /// </summary>
        public static GM_BonuesMgr Instance = new GM_BonuesMgr();

        /// <summary>
        /// NLog 日志记录器，用于记录系统运行过程中的日志信息，便于调试和监控。
        /// </summary>
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 初始化方法，目前为空，可用于后续添加初始化逻辑。
        /// </summary>
        public void Init()
        {
        }

        /// <summary>
        /// 生成奖励信息并插入到数据库中。
        /// </summary>
        /// <param name="playerId">玩家的 ID。</param>
        /// <param name="typeId">奖励的类型 ID。</param>
        /// <param name="desic">奖励的描述信息。</param>
        /// <param name="duration">奖励的持续时间，若小于等于 0 则表示无时间限制。</param>
        /// <param name="b1">奖励的第一个数值，默认为 0。</param>
        /// <param name="b2">奖励的第二个数值，默认为 0。</param>
        /// <param name="b3">奖励的第三个数值，默认为 0。</param>
        /// <param name="b4">奖励的第四个数值，默认为 0。</param>
        /// <param name="b5">奖励的第五个数值，默认为 0。</param>
        /// <returns>生成奖励成功返回 true，目前该方法总是返回 true。</returns>
        public bool GenBonues(long playerId, int typeId, string desic, int duration,
            int b1 = 0, int b2 = 0, int b3 = 0, int b4 = 0, int b5 = 0)
        {
            // 创建一个新的奖励对象
            Bonues dbBonues = new Bonues();
            // 设置奖励对象的玩家 ID
            dbBonues.uid = playerId;
            // 设置奖励对象的类型 ID
            dbBonues.tid = typeId;
            // 设置奖励对象的描述信息
            dbBonues.bonuesDesic = desic;
            // 设置奖励对象的状态为 0，表示未领取
            dbBonues.status = 0;
            // 设置奖励对象的创建时间为当前时间戳
            dbBonues.time = (int)UtilsHelper.Timestamp();
            // 设置奖励对象的结束时间，如果持续时间小于等于 0 则为 -1，表示无结束时间
            dbBonues.endTime = (duration <= 0) ? -1 : dbBonues.time + duration;

            // 设置奖励对象的各项奖励数值
            dbBonues.bonues1 = b1;
            dbBonues.bonues2 = b2;
            dbBonues.bonues3 = b3;
            dbBonues.bonues4 = b4;
            dbBonues.bonues5 = b5;

            // 将奖励对象插入到数据库中，异步执行
            DBService.Instance.GetGameInstance().Insertable(dbBonues).ExecuteCommandAsync();

            return true;
        }

        /// <summary>
        /// 将奖励应用到玩家。
        /// </summary>
        /// <param name="dbBonues">奖励对象。</param>
        /// <returns>应用成功返回 true，否则返回 false。</returns>
        private bool ApplayBonuesToPlayer(Bonues dbBonues)
        {
            // 检查奖励对象是否为空或状态不为 0（未领取），若不满足条件则返回 false
            if (dbBonues == null || dbBonues.status != 0)
            {
                return false;
            }

            MethodInfo method = null;
            // 计算奖励类型的主键，用于规则处理器查找
            int mainKey = ((dbBonues.tid) / 100000) * 100000;
            // 通过规则处理器工厂获取奖励应用的方法
            method = RuleProcesserFactory.GetProcesser((int)RuleType.Bonues, "Applay", dbBonues.tid, mainKey);

            // 若未找到对应的方法，记录警告日志并返回 false
            if (method == null)
            {
                logger.Warn($"unkonw BonuesType: {dbBonues.tid}");
                return false;
            }

            // 调用获取到的方法，将奖励应用到玩家
            method.Invoke(null, new object[] { dbBonues });

            return true;
        }

        /// <summary>
        /// 玩家接收奖励。
        /// </summary>
        /// <param name="bonuesId">奖励的 ID。</param>
        /// <param name="dbBonues">奖励对象，若为 null 则从数据库中查询。</param>
        /// <returns>接收成功返回 true，否则返回 false。</returns>
        public bool RecvBonues(long bonuesId, Bonues dbBonues = null)
        {
            // 若奖励对象为 null，则从数据库中查询该奖励
            if (dbBonues != null)
            {
                dbBonues = DBService.Instance.GetGameInstance().Queryable<Game.Datas.DBEntities.Bonues>().First(it => it.id == bonuesId);
            }

            // 调用应用奖励的方法
            bool ret = this.ApplayBonuesToPlayer(dbBonues);
            // 若应用失败则返回 false
            if (!ret)
            {
                return false;
            }

            // 将奖励状态设置为 1，表示已领取
            dbBonues.status = 1;
            // 更新奖励对象的状态到数据库，异步执行
            DBService.Instance.GetGameInstance().Updateable(dbBonues).Where(it => it.id == dbBonues.id).ExecuteCommandAsync();

            return true;
        }

        /// <summary>
        /// 生成奖励并立即应用到玩家。
        /// </summary>
        /// <param name="playerId">玩家的 ID。</param>
        /// <param name="typeId">奖励的类型 ID。</param>
        /// <param name="desic">奖励的描述信息。</param>
        /// <param name="duration">奖励的持续时间，若小于等于 0 则表示无时间限制。</param>
        /// <param name="b1">奖励的第一个数值，默认为 0。</param>
        /// <param name="b2">奖励的第二个数值，默认为 0。</param>
        /// <param name="b3">奖励的第三个数值，默认为 0。</param>
        /// <param name="b4">奖励的第四个数值，默认为 0。</param>
        /// <param name="b5">奖励的第五个数值，默认为 0。</param>
        /// <returns>生成并应用的奖励对象。</returns>
        public Bonues GenAndRecvBonues(long playerId, int typeId, string desic, int duration,
            int b1 = 0, int b2 = 0, int b3 = 0, int b4 = 0, int b5 = 0)
        {
            // 创建一个新的奖励对象
            Bonues dbBonues = new Bonues();
            // 设置奖励对象的玩家 ID
            dbBonues.uid = playerId;
            // 设置奖励对象的类型 ID
            dbBonues.tid = typeId;
            // 设置奖励对象的描述信息
            dbBonues.bonuesDesic = desic;
            // 设置奖励对象的状态为 0，表示未领取
            dbBonues.status = 0;
            // 设置奖励对象的创建时间为当前时间戳
            dbBonues.time = (int)UtilsHelper.Timestamp();
            // 设置奖励对象的结束时间，如果持续时间小于等于 0 则为 -1，表示无结束时间
            dbBonues.endTime = (duration <= 0) ? -1 : dbBonues.time + duration;

            // 设置奖励对象的各项奖励数值
            dbBonues.bonues1 = b1;
            dbBonues.bonues2 = b2;
            dbBonues.bonues3 = b3;
            dbBonues.bonues4 = b4;
            dbBonues.bonues5 = b5;

            // 将奖励应用到玩家
            this.ApplayBonuesToPlayer(dbBonues);

            // 将奖励状态设置为 1，表示已领取
            dbBonues.status = 1;
            // 将奖励对象插入到数据库中，异步执行
            DBService.Instance.GetGameInstance().Insertable(dbBonues).ExecuteCommandAsync();

            return dbBonues;
        }

        /// <summary>
        /// 计算玩家应得的奖励。
        /// </summary>
        /// <param name="playerId">玩家的 ID。</param>
        /// <param name="typeId">奖励的类型 ID。</param>
        /// <param name="param">计算奖励所需的参数。</param>
        /// <returns>计算得到的奖励对象，若未找到对应计算方法则返回 null。</returns>
        private Bonues CalcBonuesToPlayer(long playerId, int typeId, object param)
        {
            MethodInfo method = null;
            // 计算奖励类型的主键，用于规则处理器查找
            int mainKey = ((typeId) / 100000) * 100000;
            // 通过规则处理器工厂获取奖励计算的方法
            method = RuleProcesserFactory.GetProcesser((int)RuleType.Bonues, "Calc", typeId, mainKey);

            // 若未找到对应的方法，记录警告日志并返回 null
            if (method == null)
            {
                logger.Warn($"unkonw BonuesType: {typeId}");
                return null;
            }

            // 调用获取到的方法，计算奖励对象
            Game.Datas.DBEntities.Bonues dbBonues = (Game.Datas.DBEntities.Bonues)method.Invoke(null, new object[] { playerId, typeId, param });
            return dbBonues;
        }

        /// <summary>
        /// 为玩家生成奖励并插入到数据库中。
        /// </summary>
        /// <param name="playerId">玩家的 ID。</param>
        /// <param name="typeId">奖励的类型 ID。</param>
        /// <param name="param">计算奖励所需的参数，默认为 null。</param>
        public void GenBonuesToPlayer(long playerId, int typeId, object param = null)
        {
            // 调用计算奖励的方法
            Bonues dbBonues = this.CalcBonuesToPlayer(playerId, typeId, param);
            // 若计算结果为空则直接返回
            if (dbBonues == null)
            {
                return;
            }

            // 将计算得到的奖励对象插入到数据库中，异步执行
            DBService.Instance.GetGameInstance().Insertable(dbBonues).ExecuteCommandAsync();
        }

        /// <summary>
        /// 为玩家生成奖励并立即应用到玩家，同时更新数据库。
        /// </summary>
        /// <param name="playerId">玩家的 ID。</param>
        /// <param name="typeId">奖励的类型 ID。</param>
        /// <param name="param">计算奖励所需的参数，默认为 null。</param>
        public void GenAndRecvBonuesToPlayer(long playerId, int typeId, object param = null)
        {
            // 调用计算奖励的方法
            Bonues dbBonues = this.CalcBonuesToPlayer(playerId, typeId, param);
            // 若计算结果为空则直接返回
            if (dbBonues == null)
            {
                return;
            }

            // 将奖励应用到玩家
            this.ApplayBonuesToPlayer(dbBonues);
            // 将奖励状态设置为 1，表示已领取
            dbBonues.status = 1;

            // 将奖励对象插入到数据库中，异步执行
            DBService.Instance.GetGameInstance().Insertable(dbBonues).ExecuteCommandAsync();
        }

        /// <summary>
        /// 拉取指定玩家的奖励数据。
        /// </summary>
        /// <param name="playerId">玩家的 ID。</param>
        /// <param name="typeId">奖励的类型 ID，此参数在当前方法中未使用。</param>
        /// <returns>玩家的奖励数据数组。</returns>
        public Bonues[] PullingBonuesData(long playerId, int typeId)
        {
            // 从数据库中查询指定玩家的所有奖励数据
            return DBService.Instance.GetGameInstance().Queryable<Bonues>().Where(it => it.uid == playerId).ToArray();
        }

        /// <summary>
        /// 根据奖励 ID 获取单个奖励对象。
        /// </summary>
        /// <param name="bonuesId">奖励的 ID。</param>
        /// <returns>对应的奖励对象。</returns>
        public Bonues GetBonuesBy(long bonuesId)
        {
            // 从数据库中查询指定 ID 的奖励对象
            return DBService.Instance.GetGameInstance().Queryable<Bonues>().First(it => it.id == bonuesId);
        }
    }
}