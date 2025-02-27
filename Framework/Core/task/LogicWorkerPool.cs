using Framework.Core.Net;
using Framework.Core.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace Framework.Core.task
{
    /// <summary>
    /// 逻辑工作者类，代表一个工作线程的数据结构。
    /// 包含一个任务队列和一个自动重置事件，用于线程间的同步。
    /// </summary>
    class LogicWorker
    {
        // 任务队列，用于存储待执行的任务，该队列的操作是线程安全的
        public Queue<AbstractDistributeTask> taskQueue = new Queue<AbstractDistributeTask>();
        // 自动重置事件，用于线程间的同步，当有新任务加入队列时触发
        public AutoResetEvent taskEvent = new AutoResetEvent(true);
    }

    /// <summary>
    /// 逻辑工作者池类，是一个单例类，用于管理多个逻辑工作者实例。
    /// 负责扫描逻辑服务器类型、创建任务、分配任务到工作线程、启动和停止线程等操作。
    /// </summary>
    public class LogicWorkerPool
    {
        // 单例实例
        public static LogicWorkerPool Instance = new LogicWorkerPool();

        // 存储逻辑工作者实例的列表
        private List<LogicWorker> workerPool = new List<LogicWorker>();
        // 表示工作者池是否正在运行的标志
        public bool Running = false;

        // 存储逻辑任务的字典，键为服务类型，值为逻辑任务实例
        private Dictionary<int, LogicTask> logicServers = new Dictionary<int, LogicTask>();

        // 工作线程的总数
        public int ThreadCount = 0;
        // 当前正在调度任务的线程数量
        public int ActiveThreadCount = 0;

        // 是否抛出异常的标志
        public bool ThrowException = false;

        // 使用 NLog 记录日志，获取当前类的日志记录器
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 启动逻辑工作者池，扫描带有 LogicServerMeta 特性的类型，创建逻辑任务并分配到工作线程中。
        /// </summary>
        public void Start()
        {
            // 扫描所有带有 LogicServerMeta 特性的类型
            IEnumerable<Type> servers = TypeScanner.ListTypesWithAttribute(typeof(LogicServerMeta));

            int index = 0;
            // 遍历扫描到的类型
            foreach (Type server in servers)
            {
                // 创建该类型的实例
                BaseLogicServer handler = (BaseLogicServer)Activator.CreateInstance(server);
                // 获取该类型的 MainLoop 方法
                MethodInfo method = server.GetMethod("MainLoop");
                // 获取该类型的 LogicServerMeta 特性
                LogicServerMeta meta = server.GetCustomAttribute<LogicServerMeta>();
                // 创建逻辑任务实例
                LogicTask task = LogicTask.Create(index, meta.stype, handler, method);
                // 将逻辑任务添加到 logicServers 字典中
                this.logicServers.Add(meta.stype, task);
                index++;
            }

            // 如果没有找到逻辑任务，则直接返回
            if (this.logicServers.Count <= 0)
            {
                return;
            }

            // 设置线程总数为逻辑任务的数量
            this.ThreadCount = this.logicServers.Count;
            // 设置工作者池为运行状态
            Running = true;

            // 创建并启动工作线程
            for (int i = 0; i < this.ThreadCount; i++)
            {
                var workerData = new LogicWorker();
                this.workerPool.Add(workerData);
                // 将线程任务添加到线程池中执行
                ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadRun), workerData);
            }

            // 将所有逻辑任务添加到工作线程的任务队列中
            foreach (var key in this.logicServers.Keys)
            {
                this.AddTask(this.logicServers[key]);
            }
        }

        /// <summary>
        /// 执行一个任务，并捕获可能的异常。
        /// </summary>
        /// <param name="task">要执行的任务</param>
        private void ExceTask(AbstractDistributeTask task)
        {
            try
            {
                // 调用任务的执行方法
                task.DoAction();
            }
            catch (Exception ex)
            {
                // 记录任务执行异常信息
                this.logger.Error("Message handler exception:{0}, {1}, {2}, {3}", ex.InnerException, ex.Message, ex.Source, ex.StackTrace);
            }
        }

        /// <summary>
        /// 工作线程的执行方法，不断从任务队列中取出任务并执行。
        /// </summary>
        /// <param name="stateInfo">线程状态信息，即 LogicWorker 实例</param>
        private void ThreadRun(Object stateInfo)
        {
            LogicWorker worker = (LogicWorker)stateInfo;
            try
            {
                // 增加活动线程计数
                ActiveThreadCount = Interlocked.Increment(ref ActiveThreadCount);

                // 当工作者池正在运行时，持续执行任务
                while (Running)
                {
                    if (worker.taskQueue.Count == 0)
                    {
                        // 如果任务队列为空，则等待任务事件
                        worker.taskEvent.WaitOne();
                        continue;
                    }

                    // 从任务队列中取出一个任务
                    AbstractDistributeTask task = worker.taskQueue.Dequeue();
                    // 执行任务
                    LogicWorkerPool.Instance.ExceTask(task);
                }
            }
            catch
            {
                // 捕获异常，但不做处理
            }
            finally
            {
                // 减少活动线程计数
                ActiveThreadCount = Interlocked.Decrement(ref ActiveThreadCount);
            }
        }

        /// <summary>
        /// 停止逻辑工作者池，清空任务队列并等待所有线程退出。
        /// </summary>
        public void Stop()
        {
            // 设置工作者池为停止状态
            Running = false;

            // 等待所有活动线程退出
            while (ActiveThreadCount > 0)
            {
                this.logger.Info("广播逻辑服线程退出...");
                for (int i = 0; i < this.workerPool.Count; i++)
                {
                    // 清空工作线程的任务队列
                    this.workerPool[i].taskQueue.Clear();
                    // 触发任务事件，唤醒可能正在等待的线程
                    this.workerPool[i].taskEvent.Set();
                }
                // 线程休眠 1 秒
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// 向工作者池添加一个任务，并根据任务的分发键分配到相应的工作线程中。
        /// </summary>
        /// <param name="t">要添加的任务</param>
        public void AddTask(AbstractDistributeTask t)
        {
            if (this.Running == false)
            {
                return;
            }

            // 根据任务的分发键计算要分配到的工作线程索引
            int index = (int)(t.distributeKey % this.workerPool.Count);

            // 将任务添加到指定工作线程的任务队列中
            this.workerPool[index].taskQueue.Enqueue(t);
            // 触发任务事件，通知工作线程有新任务
            this.workerPool[index].taskEvent.Set();
        }

        /// <summary>
        /// 将消息推送到指定的逻辑服务器。
        /// </summary>
        /// <param name="s">会话对象</param>
        /// <param name="module">模块号</param>
        /// <param name="cmd">命令号</param>
        /// <param name="msg">消息内容</param>
        public void PushMsgToLogicServer(IdSession s, short module, short cmd, object msg)
        {
            if (this.logicServers.ContainsKey(module))
            {
                // 获取指定模块的逻辑任务
                LogicTask task = this.logicServers[module];
                // 向逻辑任务中添加事件
                task.PushEvent(s, module, cmd, msg);
            }
        }
    }
}