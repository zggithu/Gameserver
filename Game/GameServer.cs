using Framework.Core.Net;
using Framework.Core.Serializer;
using Framework.Core.task;
using Game.Core.Db;
using Game.Core.Configs;
using Game.Core.GM_Bonues;
using NLog;
using NLog.Config;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using Game.Core.EntityMgr;
using Game.Entries.Modules;
using Game.Core.GM_Task;
using Game.Core.GM_MailMessage;
using Game.Core.GM_Rank;
using Game.Core.GM_Backpack;
using Game.Core.GM_Trading;
using Game.Utils;

namespace Game
{
    /// <summary>
    /// 游戏服务器类，负责游戏服务器的初始化、启动和关闭操作。
    /// </summary>
    public class GameServer
    {
        // 使用 NLog 记录日志
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        // 单例模式，确保游戏服务器只有一个实例
        public static GameServer Instance = new GameServer();

        // 网络服务器实例
        NettySocketServer server = null;

        /// <summary>
        /// 框架初始化方法，初始化框架相关组件和启动网络服务器。
        /// </summary>
        private void FrameworkInit() {
            // 加载我们的配置文件，如果默认是nlog.config,会被自动加载
            LogManager.Configuration = new XmlLoggingConfiguration("nlog.config");
            // 初始化消息工厂的消息池
            MessageFactory.Instance.InitMeesagePool();

            // 初始化消息分发器
            MessageDispatcher.Instance.Init();
            // 初始化 HTTP 路由分发器
            HttpRouteDispatcher.Instance.Init();

            // 初始化会话管理器
            SessionMgr.Instance.Init();
            // 启动任务工作池，设置线程数量为 5
            TaskWorkerPool.Instance.Start(5);

            // 创建网络服务器实例
            this.server = new NettySocketServer();
            // 从配置文件中获取 TCP 端口号
            int tcpPort = int.Parse(ConfigurationManager.AppSettings["port"]);
            // 从配置文件中获取 WebSocket 端口号
            int wsPort = int.Parse(ConfigurationManager.AppSettings["wsPort"]);
            // 从配置文件中获取 HTTP 端口号
            int httpPort = int.Parse(ConfigurationManager.AppSettings["httpPort"]);
            // 从配置文件中获取是否使用 SSL 的标志
            bool IsSsl = bool.Parse(ConfigurationManager.AppSettings["IsSsl"]);
            // 启动 TCP 和 WebSocket 服务器，并等待启动完成
            this.server.StartSterverAt(tcpPort, wsPort, IsSsl).Wait();

            // 启动 HTTP 服务器，并等待启动完成
            this.server.StartHttpServer(httpPort, IsSsl).Wait();
        }

        /// <summary>
        /// 游戏逻辑初始化方法，初始化游戏相关的逻辑组件。
        /// </summary>
        private void GameLogicInit() {
            // 初始化规则处理工厂
            RuleProcesserFactory.Init();

            // 初始化数据库服务
            DBService.Instance.Init();
            RedisService.Instance.Init();
            // 数据库服务初始化结束

            // 初始化配置管理器
            ConfigMgr.Instance.Init();
            // 初始化实体管理器
            GM_EntityMgr.Instance.Init();

            // 初始化邮件消息管理器
            GM_MailMsgMgr.Instance.Init();
            // 初始化奖励管理器
            GM_BonuesMgr.Instance.Init();
            // 初始化任务管理器
            GM_TaskMgr.Instance.Init();
            // 初始化排行榜管理器
            GM_RankMgr.Instance.Init();
            // 初始化背包管理器
            GM_BackpackMgr.Instance.Init();
            // 初始化交易管理器
            GM_TradingMgr.Instance.Init();

            // 初始化认证模块
            AuthModule.Instance.Init();
            // 初始化玩家模块
            PlayerModule.Instance.Init();

            // 启动逻辑工作池
            LogicWorkerPool.Instance.Start();
        }

        /// <summary>
        /// 启动游戏服务器，调用框架初始化和游戏逻辑初始化方法，并记录启动耗时。
        /// </summary>
        public void Start() {
            // 创建一个 Stopwatch 实例，用于记录服务器启动耗时
            Stopwatch stopWatch = new Stopwatch();

            // 开始计时
            stopWatch.Start();

            // 调用框架初始化方法
            this.FrameworkInit();

            // 调用游戏逻辑初始化方法
            this.GameLogicInit();

            // 停止计时
            stopWatch.Stop();

            // 记录服务器启动耗时
            logger.Info($"游戏服务启动，耗时[{stopWatch.ElapsedMilliseconds}]毫秒");
        }

        /// <summary>
        /// 异步关闭游戏服务器，关闭网络服务器和工作池。
        /// </summary>
        /// <returns>表示异步操作的任务。</returns>
        public async Task Shutdown() {
            // 异步关闭网络服务器
            await this.server.Shutdown();

            // 停止逻辑工作池
            LogicWorkerPool.Instance.Stop();
            // 停止任务工作池
            TaskWorkerPool.Instance.Stop();
        }
    }
}