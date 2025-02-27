using SqlSugar;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core.Db
{
    /// <summary>
    /// 数据库服务类，采用单例模式，负责管理数据库连接和提供数据库客户端实例。
    /// </summary>
    class DBService
    {
        // 单例实例，确保整个应用程序中只有一个 DBService 实例
        public static DBService Instance = new DBService();

        // 认证用户数据库连接字符串设置对象
        private ConnectionStringSettings connAuthUserStr;
        // 游戏数据数据库连接字符串设置对象
        private ConnectionStringSettings connGameDataStr;

        /// <summary>
        /// 初始化方法，从配置文件中读取数据库连接字符串。
        /// </summary>
        public void Init()
        {
            // 从配置文件的 ConnectionStrings 节中读取认证用户数据库的连接字符串
            connAuthUserStr = ConfigurationManager.ConnectionStrings["connAuthUserStr"];
            // 从配置文件的 ConnectionStrings 节中读取游戏数据数据库的连接字符串
            connGameDataStr = ConfigurationManager.ConnectionStrings["connGameDataStr"];
        }

        /// <summary>
        /// 获取游戏数据数据库的 SqlSugarClient 实例。
        /// </summary>
        /// <returns>游戏数据数据库的 SqlSugarClient 实例。</returns>
        public SqlSugarClient GetGameInstance()
        {
            // 创建数据库对象
            SqlSugarClient db = new SqlSugarClient(new ConnectionConfig()
            {
                // 设置数据库连接字符串为游戏数据数据库的连接字符串
                ConnectionString = connGameDataStr.ConnectionString,
                // 指定数据库类型为 MySQL
                DbType = DbType.MySql,
                // 开启自动关闭连接功能
                IsAutoCloseConnection = true,
                // 从特性读取主键自增信息
                InitKeyType = InitKeyType.Attribute
            });

            // 添加 Sql 打印事件，在开发中可以删掉此代码
            // 该事件会在 SQL 语句执行时触发，可用于调试
            db.Aop.OnLogExecuting = (sql, pars) =>
            {
                // Console.WriteLine(sql);
            };

            return db;
        }

        /// <summary>
        /// 获取认证用户数据库的 SqlSugarClient 实例。
        /// </summary>
        /// <returns>认证用户数据库的 SqlSugarClient 实例。</returns>
        public SqlSugarClient GetAuthInstance()
        {
            // 创建数据库对象
            SqlSugarClient db = new SqlSugarClient(new ConnectionConfig()
            {
                // 设置数据库连接字符串为认证用户数据库的连接字符串
                ConnectionString = connAuthUserStr.ConnectionString,
                // 指定数据库类型为 MySQL
                DbType = DbType.MySql,
                // 开启自动关闭连接功能
                IsAutoCloseConnection = true,
                // 从特性读取主键自增信息
                InitKeyType = InitKeyType.Attribute
            });

            // 添加 Sql 打印事件，在开发中可以删掉此代码
            // 该事件会在 SQL 语句执行时触发，可用于调试
            db.Aop.OnLogExecuting = (sql, pars) =>
            {
                // Console.WriteLine(sql);
            };

            return db;
        }
    }
}