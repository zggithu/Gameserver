using System;
using System.Collections.Generic;
using System.Threading;

namespace Framework.Core.task
{
    /// <summary>
    /// ���������࣬����һ�������̣߳�����������к�ͬ���¼���
    /// </summary>
    class TaskWorker
    {
        // ������У����ڴ洢��ִ�е����񣬲������̰߳�ȫ��
        public Queue<AbstractDistributeTask> taskQueue = new Queue<AbstractDistributeTask>();
        // �Զ������¼��������̼߳�ͬ��������������������ʱ����
        public AutoResetEvent taskEvent = new AutoResetEvent(true);
    }

    /// <summary>
    /// ���������̳߳��࣬���õ���ģʽ������������������ߣ���������ĵ��Ⱥ�ִ�С�
    /// </summary>
    public class TaskWorkerPool
    {
        // ����ʵ��
        public static TaskWorkerPool Instance = new TaskWorkerPool();

        // �洢�������ߵ��б�
        private List<TaskWorker> workerPool = new List<TaskWorker>();
        // ��ʾ�̳߳��Ƿ��������еı�־
        private bool Running = false;

        // �̳߳��е��߳�����
        public int ThreadCount = 0;
        // ��ǰ���ڵ���������߳�����
        public int ActiveThreadCount = 0;

        // �Ƿ��׳��쳣�ı�־
        public bool ThrowException = false;

        // ʹ�� NLog ��¼��־����ȡ��ǰ�����־��¼��
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// �����̳߳أ����ݴ�����߳�����������Ӧ���������߲����������̡߳�
        /// </summary>
        /// <param name="ThreadNum">Ҫ�������߳�����</param>
        public void Start(int ThreadNum)
        {
            // �����߳�������ȷ������ 1 �� 1000 ֮��
            this.ThreadCount = ThreadNum;
            this.ThreadCount = (this.ThreadCount < 1) ? 1 : this.ThreadCount;
            this.ThreadCount = (this.ThreadCount > 1000) ? 1000 : this.ThreadCount;

            // ����̳߳�Ϊ����״̬
            Running = true;

            // ����������ָ�������Ĺ����߳�
            for (int i = 0; i < this.ThreadCount; i++)
            {
                var workerData = new TaskWorker();
                this.workerPool.Add(workerData);
                // ���߳�������ӵ��̳߳�ִ��
                ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadRun), workerData);
            }
        }

        /// <summary>
        /// ִ�е������񣬲�����ͼ�¼ִ�й����е��쳣��
        /// </summary>
        /// <param name="task">Ҫִ�е�����</param>
        private void ExceTask(AbstractDistributeTask task)
        {
            try
            {
                // ���������ִ�з���
                task.DoAction();
            }
            catch (Exception ex)
            {
                // ��¼����ִ�й����е��쳣��Ϣ
                this.logger.Error("Message handler exception:{0}, {1}, {2}, {3}", ex.InnerException, ex.Message, ex.Source, ex.StackTrace);
            }
        }

        /// <summary>
        /// �����̵߳���ѭ�����������ϴ����������ȡ������ִ�С�
        /// </summary>
        /// <param name="stateInfo">��������ʵ��</param>
        private void ThreadRun(Object stateInfo)
        {
            TaskWorker worker = (TaskWorker)stateInfo;
            try
            {
                // ���ӻ�̼߳���
                ActiveThreadCount = Interlocked.Increment(ref ActiveThreadCount);

                // ���̳߳���������ʱ������ִ������
                while (Running)
                {
                    if (worker.taskQueue.Count == 0)
                    {
                        // ����������Ϊ�գ��ȴ���������
                        worker.taskEvent.WaitOne();
                        continue;
                    }

                    // �����������ȡ��һ������
                    AbstractDistributeTask task = worker.taskQueue.Dequeue();
                    // ִ������
                    TaskWorkerPool.Instance.ExceTask(task);
                }
            }
            catch
            {
                // �����쳣������������
            }
            finally
            {
                // ���ٻ�̼߳���
                ActiveThreadCount = Interlocked.Decrement(ref ActiveThreadCount);
            }
        }

        /// <summary>
        /// ֹͣ�̳߳ص����У����������в��ȴ����й����߳��˳���
        /// </summary>
        public void Stop()
        {
            // ����̳߳�Ϊֹͣ״̬
            Running = false;

            // �ȴ����л�߳��˳�
            while (ActiveThreadCount > 0)
            {
                this.logger.Info("�㲥��Ϣ�����߳��˳�...");
                for (int i = 0; i < this.workerPool.Count; i++)
                {
                    // ����������
                    this.workerPool[i].taskQueue.Clear();
                    // �����¼������ѿ������ڵȴ����߳�
                    this.workerPool[i].taskEvent.Set();
                }
                // �߳����� 1 ��
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// ��������ӵ��̳߳ص���������У���������ķַ���ѡ����ʵĹ����̡߳�
        /// </summary>
        /// <param name="t">Ҫ��ӵ�����</param>
        public void AddTask(AbstractDistributeTask t)
        {
            if (this.Running == false)
            {
                // ����̳߳�δ���У�ֱ�ӷ���
                return;
            }

            // ��������ķַ�������Ҫ��ӵ��Ĺ����߳�����
            int index = (int)(t.distributeKey % this.workerPool.Count);

            // ��������ӵ�ָ�������̵߳����������
            this.workerPool[index].taskQueue.Enqueue(t);
            // �����¼���֪ͨ�����߳���������
            this.workerPool[index].taskEvent.Set();
        }
    }
}