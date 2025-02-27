using System;
using System.Collections.Generic;
using System.Threading;

namespace Framework.Core.task
{
    /// <summary>
    /// 任务工作者类，代表一个工作线程，包含任务队列和同步事件。
    /// </summary>
    class TaskWorker
    {
        // 任务队列，用于存储待执行的任务，操作是线程安全的
        public Queue<AbstractDistributeTask> taskQueue = new Queue<AbstractDistributeTask>();
        // 自动重置事件，用于线程间同步，当队列中有新任务时触发
        public AutoResetEvent taskEvent = new AutoResetEvent(true);
    }

    /// <summary>
    /// 任务工作者线程池类，采用单例模式，负责管理多个任务工作者，处理任务的调度和执行。
    /// </summary>
    public class TaskWorkerPool
    {
        // 单例实例
        public static TaskWorkerPool Instance = new TaskWorkerPool();

        // 存储任务工作者的列表
        private List<TaskWorker> workerPool = new List<TaskWorker>();
        // 表示线程池是否正在运行的标志
        private bool Running = false;

        // 线程池中的线程总数
        public int ThreadCount = 0;
        // 当前正在调度任务的线程数量
        public int ActiveThreadCount = 0;

        // 是否抛出异常的标志
        public bool ThrowException = false;

        // 使用 NLog 记录日志，获取当前类的日志记录器
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 启动线程池，根据传入的线程数量创建相应的任务工作者并启动工作线程。
        /// </summary>
        /// <param name="ThreadNum">要创建的线程数量</param>
        public void Start(int ThreadNum)
        {
            // 设置线程数量，确保其在 1 到 1000 之间
            this.ThreadCount = ThreadNum;
            this.ThreadCount = (this.ThreadCount < 1) ? 1 : this.ThreadCount;
            this.ThreadCount = (this.ThreadCount > 1000) ? 1000 : this.ThreadCount;

            // 标记线程池为运行状态
            Running = true;

            // 创建并启动指定数量的工作线程
            for (int i = 0; i < this.ThreadCount; i++)
            {
                var workerData = new TaskWorker();
                this.workerPool.Add(workerData);
                // 将线程任务添加到线程池执行
                ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadRun), workerData);
            }
        }

        /// <summary>
        /// 执行单个任务，并捕获和记录执行过程中的异常。
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
                // 记录任务执行过程中的异常信息
                this.logger.Error("Message handler exception:{0}, {1}, {2}, {3}", ex.InnerException, ex.Message, ex.Source, ex.StackTrace);
            }
        }

        /// <summary>
        /// 工作线程的主循环方法，不断从任务队列中取出任务并执行。
        /// </summary>
        /// <param name="stateInfo">任务工作者实例</param>
        private void ThreadRun(Object stateInfo)
        {
            TaskWorker worker = (TaskWorker)stateInfo;
            try
            {
                // 增加活动线程计数
                ActiveThreadCount = Interlocked.Increment(ref ActiveThreadCount);

                // 当线程池正在运行时，持续执行任务
                while (Running)
                {
                    if (worker.taskQueue.Count == 0)
                    {
                        // 如果任务队列为空，等待新任务到来
                        worker.taskEvent.WaitOne();
                        continue;
                    }

                    // 从任务队列中取出一个任务
                    AbstractDistributeTask task = worker.taskQueue.Dequeue();
                    // 执行任务
                    TaskWorkerPool.Instance.ExceTask(task);
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
        /// 停止线程池的运行，清空任务队列并等待所有工作线程退出。
        /// </summary>
        public void Stop()
        {
            // 标记线程池为停止状态
            Running = false;

            // 等待所有活动线程退出
            while (ActiveThreadCount > 0)
            {
                this.logger.Info("广播消息处理线程退出...");
                for (int i = 0; i < this.workerPool.Count; i++)
                {
                    // 清空任务队列
                    this.workerPool[i].taskQueue.Clear();
                    // 触发事件，唤醒可能正在等待的线程
                    this.workerPool[i].taskEvent.Set();
                }
                // 线程休眠 1 秒
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// 将任务添加到线程池的任务队列中，根据任务的分发键选择合适的工作线程。
        /// </summary>
        /// <param name="t">要添加的任务</param>
        public void AddTask(AbstractDistributeTask t)
        {
            if (this.Running == false)
            {
                // 如果线程池未运行，直接返回
                return;
            }

            // 根据任务的分发键计算要添加到的工作线程索引
            int index = (int)(t.distributeKey % this.workerPool.Count);

            // 将任务添加到指定工作线程的任务队列中
            this.workerPool[index].taskQueue.Enqueue(t);
            // 触发事件，通知工作线程有新任务
            this.workerPool[index].taskEvent.Set();
        }
    }
}