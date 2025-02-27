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
    /// ��Ϸ�������࣬������Ϸ�������ĳ�ʼ���������͹رղ�����
    /// </summary>
    public class GameServer
    {
        // ʹ�� NLog ��¼��־
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        // ����ģʽ��ȷ����Ϸ������ֻ��һ��ʵ��
        public static GameServer Instance = new GameServer();

        // ���������ʵ��
        NettySocketServer server = null;

        /// <summary>
        /// ��ܳ�ʼ����������ʼ����������������������������
        /// </summary>
        private void FrameworkInit() {
            // �������ǵ������ļ������Ĭ����nlog.config,�ᱻ�Զ�����
            LogManager.Configuration = new XmlLoggingConfiguration("nlog.config");
            // ��ʼ����Ϣ��������Ϣ��
            MessageFactory.Instance.InitMeesagePool();

            // ��ʼ����Ϣ�ַ���
            MessageDispatcher.Instance.Init();
            // ��ʼ�� HTTP ·�ɷַ���
            HttpRouteDispatcher.Instance.Init();

            // ��ʼ���Ự������
            SessionMgr.Instance.Init();
            // �����������أ������߳�����Ϊ 5
            TaskWorkerPool.Instance.Start(5);

            // �������������ʵ��
            this.server = new NettySocketServer();
            // �������ļ��л�ȡ TCP �˿ں�
            int tcpPort = int.Parse(ConfigurationManager.AppSettings["port"]);
            // �������ļ��л�ȡ WebSocket �˿ں�
            int wsPort = int.Parse(ConfigurationManager.AppSettings["wsPort"]);
            // �������ļ��л�ȡ HTTP �˿ں�
            int httpPort = int.Parse(ConfigurationManager.AppSettings["httpPort"]);
            // �������ļ��л�ȡ�Ƿ�ʹ�� SSL �ı�־
            bool IsSsl = bool.Parse(ConfigurationManager.AppSettings["IsSsl"]);
            // ���� TCP �� WebSocket �����������ȴ��������
            this.server.StartSterverAt(tcpPort, wsPort, IsSsl).Wait();

            // ���� HTTP �����������ȴ��������
            this.server.StartHttpServer(httpPort, IsSsl).Wait();
        }

        /// <summary>
        /// ��Ϸ�߼���ʼ����������ʼ����Ϸ��ص��߼������
        /// </summary>
        private void GameLogicInit() {
            // ��ʼ����������
            RuleProcesserFactory.Init();

            // ��ʼ�����ݿ����
            DBService.Instance.Init();
            RedisService.Instance.Init();
            // ���ݿ�����ʼ������

            // ��ʼ�����ù�����
            ConfigMgr.Instance.Init();
            // ��ʼ��ʵ�������
            GM_EntityMgr.Instance.Init();

            // ��ʼ���ʼ���Ϣ������
            GM_MailMsgMgr.Instance.Init();
            // ��ʼ������������
            GM_BonuesMgr.Instance.Init();
            // ��ʼ�����������
            GM_TaskMgr.Instance.Init();
            // ��ʼ�����а������
            GM_RankMgr.Instance.Init();
            // ��ʼ������������
            GM_BackpackMgr.Instance.Init();
            // ��ʼ�����׹�����
            GM_TradingMgr.Instance.Init();

            // ��ʼ����֤ģ��
            AuthModule.Instance.Init();
            // ��ʼ�����ģ��
            PlayerModule.Instance.Init();

            // �����߼�������
            LogicWorkerPool.Instance.Start();
        }

        /// <summary>
        /// ������Ϸ�����������ÿ�ܳ�ʼ������Ϸ�߼���ʼ������������¼������ʱ��
        /// </summary>
        public void Start() {
            // ����һ�� Stopwatch ʵ�������ڼ�¼������������ʱ
            Stopwatch stopWatch = new Stopwatch();

            // ��ʼ��ʱ
            stopWatch.Start();

            // ���ÿ�ܳ�ʼ������
            this.FrameworkInit();

            // ������Ϸ�߼���ʼ������
            this.GameLogicInit();

            // ֹͣ��ʱ
            stopWatch.Stop();

            // ��¼������������ʱ
            logger.Info($"��Ϸ������������ʱ[{stopWatch.ElapsedMilliseconds}]����");
        }

        /// <summary>
        /// �첽�ر���Ϸ���������ر�����������͹����ء�
        /// </summary>
        /// <returns>��ʾ�첽����������</returns>
        public async Task Shutdown() {
            // �첽�ر����������
            await this.server.Shutdown();

            // ֹͣ�߼�������
            LogicWorkerPool.Instance.Stop();
            // ֹͣ��������
            TaskWorkerPool.Instance.Stop();
        }
    }
}