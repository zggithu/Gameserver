using Game.Core.Caches;
using Game.Core.Configs;
using Game.Core.GM_MailMessage;
using Game.Core.GM_Rank;
using Game.Datas.GMEntities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core.EntityMgr
{
    /// <summary>
    /// GM_EntityMgr 类负责管理游戏中玩家实体的相关操作，是一个单例类，确保全局只有一个实例。
    /// 提供了玩家实体的初始化、添加、移除和获取功能，同时处理玩家实体组件的初始化和退出逻辑。
    /// 还涉及到玩家信息缓存的使用、邮件消息发送以及排行榜数据刷新等操作。
    /// </summary>
    public class GM_EntityMgr
    {
        /// <summary>
        /// GM_EntityMgr 类的单例实例，方便全局访问。
        /// </summary>
        public static GM_EntityMgr Instance = new GM_EntityMgr();

        // 线程安全的字典，用于存储玩家 ID 和对应的玩家实体，确保在多线程环境下操作的安全性
        private ConcurrentDictionary<long, GM_PlayerEntity> players = null;

        /// <summary>
        /// 初始化方法，用于初始化存储玩家实体的字典。
        /// </summary>
        public void Init()
        {
            // 创建一个新的并发字典来存储玩家实体
            this.players = new ConcurrentDictionary<long, GM_PlayerEntity>();
        }

        /// <summary>
        /// 初始化玩家任务组件的方法。
        /// </summary>
        /// <param name="uTask">要初始化的任务组件的引用。</param>
        /// <param name="playerId">玩家的 ID。</param>
        private void InitTaskComponent(ref TaskComponent uTask, long playerId)
        {
        }

        /// <summary>
        /// 向管理器中添加一个玩家实体。
        /// 如果玩家已经存在，则直接获取该玩家实体；如果不存在，则创建一个新的玩家实体并添加到字典中。
        /// 同时初始化玩家实体的组件，并根据配置刷新排行榜数据。
        /// </summary>
        /// <param name="playerId">玩家的 ID。</param>
        /// <param name="accountId">玩家的账户 ID。</param>
        public void AddPlayer(long playerId, long accountId)
        {
            GM_PlayerEntity player = null;
            // 检查玩家是否已经存在于字典中
            if (this.players.ContainsKey(playerId))
            {
                // 如果存在，直接从字典中获取该玩家实体
                player = this.players[playerId];
            }
            else
            {
                // 如果不存在，创建一个新的玩家实体
                player = new GM_PlayerEntity();
                // 将新的玩家实体添加到并发字典中
                this.players.TryAdd(playerId, player);
            }

            // 从缓存中获取玩家信息和账户信息，并赋值给玩家实体的相应属性
            player.uPlayer.playerInfo = PlayerIDCache.Instance.Get(playerId);
            player.uPlayer.accountInfo = AccountIDCache.Instance.Get(accountId);

            // 获取 GM_PlayerEntity 类的所有字段
            FieldInfo[] fileds = (typeof(GM_PlayerEntity)).GetFields();
            foreach (var f in fileds)
            {
                // 如果字段类型是 PlayerComponent，则跳过该字段
                if (f.FieldType == typeof(PlayerComponent))
                {
                    continue;
                }

                // 获取字段类型的 Init 方法
                MethodInfo m = f.FieldType.GetMethod("Init", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (m != null)
                {
                    // 如果存在 Init 方法，则调用该方法并传入玩家实体作为参数
                    m.Invoke(null, new object[] { player });
                }
            }

            // 做一个测试，发送一个邮件：
            // 向指定玩家发送欢迎邮件
            // GM_MailMsgMgr.Instance.SendMailMsg(-1, playerId, "欢迎来到游戏世界,这是第2封邮件消息测试");
            // end

            // 如果配置中启用了 Redis，则刷新世界金币排行榜数据
            if (ConfigMgr.Instance.useRedis)
            {
                GM_RankMgr.Instance.FlushRank((int)RankType.WorldCoin, playerId, player.uPlayer.playerInfo.ucoin);
            }

        }


        /// <summary>
        /// 从管理器中移除一个玩家实体。
        /// 移除玩家实体后，调用玩家实体组件的 Exit 方法进行清理操作。
        /// </summary>
        /// <param name="playerId">要移除的玩家的 ID。</param>
        public void RemovePlayer(long playerId)
        {
            GM_PlayerEntity player = null;
            // 从并发字典中移除指定玩家 ID 的玩家实体
            this.players.TryRemove(playerId, out player);

            // 获取 GM_PlayerEntity 类的所有字段
            FieldInfo[] fileds = (typeof(GM_PlayerEntity)).GetFields();
            foreach (var f in fileds)
            {
                // 如果字段类型是 PlayerComponent，则跳过该字段
                if (f.FieldType == typeof(PlayerComponent))
                {
                    continue;
                }

                // 获取字段类型的 Exit 方法
                MethodInfo m = f.FieldType.GetMethod("Exit", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (m != null)
                {
                    // 如果存在 Exit 方法，则调用该方法并传入玩家实体作为参数
                    m.Invoke(null, new object[] { player });
                }
            }
        }

        /// <summary>
        /// 根据玩家 ID 获取对应的玩家实体。
        /// </summary>
        /// <param name="playerId">要获取的玩家的 ID。</param>
        /// <returns>如果玩家存在，则返回对应的玩家实体；否则返回 null。</returns>
        public GM_PlayerEntity Get(long playerId)
        {
            // 检查玩家是否存在于字典中
            if (this.players.ContainsKey(playerId))
            {
                // 如果存在，返回对应的玩家实体
                return this.players[playerId];
            }

            // 如果不存在，返回 null
            return null;
        }

    }
}