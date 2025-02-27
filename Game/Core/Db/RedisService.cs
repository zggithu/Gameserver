using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core.Db
{
    /// <summary>
    /// 该类用于与 Redis 数据库进行交互，提供有序集合（Sorted Set）的添加和查询操作。
    /// 采用单例模式，确保在整个应用程序中只有一个 RedisService 实例。
    /// </summary>
    class RedisService
    {
        /// <summary>
        /// RedisService 类的单例实例，通过该实例可以访问 RedisService 的所有方法。
        /// </summary>
        public static RedisService Instance = new RedisService();

        /// <summary>
        /// 存储从配置文件中读取的 Redis 服务器 IP 地址的连接字符串设置。
        /// </summary>
        private ConnectionStringSettings redisIpAddr;
        /// <summary>
        /// 存储从配置文件中读取的 Redis 服务器端口号的连接字符串设置。
        /// </summary>
        private ConnectionStringSettings redisPort;

        /// <summary>
        /// 初始化 RedisService 类，从配置文件中读取 Redis 服务器的 IP 地址和端口号。
        /// 该方法应在使用 RedisService 实例的其他方法之前调用。
        /// </summary>
        public void Init()
        {
            // 从配置文件中获取名为 "redisIpAddr" 的连接字符串设置，并赋值给 redisIpAddr 字段
            this.redisIpAddr = ConfigurationManager.ConnectionStrings["redisIpAddr"];
            // 从配置文件中获取名为 "redisPort" 的连接字符串设置，并赋值给 redisPort 字段
            this.redisPort = ConfigurationManager.ConnectionStrings["redisPort"];
        }

        /// <summary>
        /// 向 Redis 的有序集合（Sorted Set）中添加一个元素。
        /// </summary>
        /// <param name="name">有序集合的名称。</param>
        /// <param name="key">要添加的元素的键。</param>
        /// <param name="value">要添加的元素的分数，用于排序。</param>
        public void SortedSetAdd(string name, string key, int value)
        {
            try
            {
                // 创建 Redis 连接的配置选项
                ConfigurationOptions configurationOptions = new ConfigurationOptions
                {
                    // 将从配置文件中读取的 IP 地址和端口号添加到配置选项的终结点列表中
                    EndPoints = { { this.redisIpAddr.ConnectionString, int.Parse(this.redisPort.ConnectionString) } },
                };
                // 根据配置选项建立与 Redis 服务器的连接
                ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(configurationOptions);

                // 获取 Redis 数据库的第 3 个数据库实例
                IDatabase database = connection.GetDatabase(3);
                // 向指定名称的有序集合中添加元素，元素的键为 key，分数为 value
                database.SortedSetAdd(name, key, value);

                // 关闭与 Redis 服务器的连接
                connection.Close();
                // 释放连接占用的资源
                connection.Dispose();
            }
            catch (Exception e)
            {
                // 捕获异常，但此处未做具体处理，可根据实际需求添加日志记录等操作
            }
        }

        /// <summary>
        /// 从 Redis 的有序集合（Sorted Set）中获取排名前 num 的元素及其分数。
        /// </summary>
        /// <param name="rankName">有序集合的名称。</param>
        /// <param name="num">要获取的元素数量。</param>
        /// <returns>包含排名前 num 的元素及其分数的数组，如果出现异常则返回 null。</returns>
        public SortedSetEntry[] SortedSetRangeByRankWithScores(string rankName, int num)
        {
            try
            {
                // 创建 Redis 连接的配置选项
                ConfigurationOptions configurationOptions = new ConfigurationOptions
                {
                    // 将从配置文件中读取的 IP 地址和端口号添加到配置选项的终结点列表中
                    EndPoints = { { this.redisIpAddr.ConnectionString, int.Parse(this.redisPort.ConnectionString) } },
                };
                // 根据配置选项建立与 Redis 服务器的连接
                ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(configurationOptions);

                // 获取 Redis 数据库的第 3 个数据库实例
                IDatabase database = connection.GetDatabase(3);
                // 从指定名称的有序集合中获取排名前 num 的元素及其分数，结果按分数降序排列
                SortedSetEntry[] rankValueWithScore = database.SortedSetRangeByRankWithScores(rankName, 0, num, Order.Descending);

                // 关闭与 Redis 服务器的连接
                connection.Close();
                // 释放连接占用的资源
                connection.Dispose();

                // 返回获取到的元素及其分数的数组
                return rankValueWithScore;
            }
            catch (Exception)
            {
                // 捕获异常，但此处未做具体处理，可根据实际需求添加日志记录等操作
                // 出现异常时返回 null
                return null;
            }
        }
    }
}