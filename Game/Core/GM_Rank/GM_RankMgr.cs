using Game.Core.Db;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core.GM_Rank
{
    /// <summary>
    /// 排行榜数据结构体，用于存储排行榜中的单条数据信息。
    /// </summary>
    public struct RankData
    {
        /// <summary>
        /// 玩家的唯一标识符。
        /// </summary>
        public long uid;
        /// <summary>
        /// 玩家对应的排名数值。
        /// </summary>
        public int value;
    }

    /// <summary>
    /// GM_RankMgr 类是一个单例类，用于管理游戏中的排行榜数据。
    /// 提供了排行榜数据的更新和获取功能，使用 Redis 作为数据存储。
    /// </summary>
    public class GM_RankMgr
    {
        /// <summary>
        /// GM_RankMgr 类的单例实例，方便全局访问。
        /// </summary>
        public static GM_RankMgr Instance = new GM_RankMgr();

        /// <summary>
        /// 初始化方法，目前为空，可用于后续添加初始化逻辑，
        /// 例如连接 Redis 等操作。
        /// </summary>
        public void Init()
        {
        }

        /// <summary>
        /// 更新排行榜数据的方法，将玩家的排名信息添加或更新到 Redis 的有序集合中。
        /// </summary>
        /// <param name="rankType">排行榜的类型，用于标识不同的排行榜。</param>
        /// <param name="playrId">玩家的唯一标识符。</param>
        /// <param name="value">玩家对应的排名数值。</param>
        public void FlushRank(int rankType, long playrId, int value)
        {
            // 调用 RedisService 的 SortedSetAdd 方法，将玩家的排名信息添加到 Redis 的有序集合中
            // 有序集合的键为排行榜类型的字符串表示，成员为玩家 ID 的字符串表示，分数为排名数值
            RedisService.Instance.SortedSetAdd(rankType.ToString(), playrId.ToString(), value);
        }

        /// <summary>
        /// 获取指定排行榜前 num 条数据的方法。
        /// </summary>
        /// <param name="rankType">排行榜的类型，用于标识不同的排行榜。</param>
        /// <param name="num">要获取的排行榜数据的数量。</param>
        /// <returns>包含排行榜数据的 RankData 数组，如果没有数据则返回 null。</returns>
        public RankData[] GetRankData(int rankType, int num)
        {
            // 调用 RedisService 的 SortedSetRangeByRankWithScores 方法，从 Redis 的有序集合中获取前 num 条数据
            SortedSetEntry[] rankData = RedisService.Instance.SortedSetRangeByRankWithScores(rankType.ToString(), num);
            // 如果获取的数据为空或数量为 0，则返回 null
            if (rankData == null || rankData.Length <= 0)
            {
                return null;
            }

            // 创建一个与获取的数据长度相同的 RankData 数组
            RankData[] randList = new RankData[rankData.Length];

            // 遍历获取的数据，将其转换为 RankData 结构体数组
            for (int i = 0; i < randList.Length; i++)
            {
                // 将 Redis 中的成员（玩家 ID）转换为 long 类型并赋值给 RankData 的 uid
                randList[i].uid = long.Parse(rankData[i].Element);
                // 将 Redis 中的分数（排名数值）转换为 int 类型并赋值给 RankData 的 value
                randList[i].value = (int)rankData[i].Score;
            }
            return randList;
        }
    }
}