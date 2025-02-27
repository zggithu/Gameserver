using Framework.Core.Utils;
using Game.Core.Db;
using Game.Datas.DBEntities;
using Game.Datas.GMEntities;
using Game.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;

/*
 * Gametask status的说明: // 0, 未开启, 1,正在进行中, 2结束完成了，3,结束已经处理了结束流程
 */
namespace Game.Core.GM_Task
{
    // 定义任务状态的枚举，包含各种可能的任务状态
    public enum TaskStatus
    {
        Invalid = -1,
        UnOpended = 0,
        Starting = 1,
        Complete = 2,
        EndComplete = 3,
    }

    // 自定义特性，用于标记任务类型，可应用于类，且每个类只能有一个该特性
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class TaskType : Attribute
    {
        public int mainType;

        public TaskType(int mainType)
        {
            this.mainType = mainType;
        }
    }

    // 表示单个游戏任务的类
    public class GM_Task
    {
        public Gametask dbTaskInst;
        public string taskDesic = null;
        // 存储任务进度数据，不同类型任务该数据类型可能不同
        public object taskProgressData = null;

        // 静态方法，用于创建一个新的任务实例
        public static GM_Task Create(long uid, int tid)
        {
            GM_Task task = new GM_Task();
            task.dbTaskInst = new Gametask();
            // 为任务生成唯一ID
            task.dbTaskInst.id = IdGenerator.GetNextId();
            task.dbTaskInst.endTime = -1;
            // 设置任务开始时间为当前时间戳
            task.dbTaskInst.startTime = (int)UtilsHelper.Timestamp();
            task.dbTaskInst.tid = tid;
            task.dbTaskInst.uid = uid;
            // 初始状态设置为正在进行
            task.dbTaskInst.status = (int)TaskStatus.Starting;

            return task;
        }
    }

    // 表示任务线的类，包含任务线类型和该任务线下的所有任务
    public class GM_TaskLine
    {
        public int taskLineType;
        public List<GM_Task> taskSet = new List<GM_Task>();
    }

    // 游戏任务管理类，采用单例模式
    public class GM_TaskMgr
    {
        public static GM_TaskMgr Instance = new GM_TaskMgr();

        private List<int> taskTypes = new List<int>();

        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        // 初始化方法，扫描所有带有TaskType特性的类，获取任务类型
        public void Init()
        {
            IEnumerable<Type> taskTypes = TypeScanner.ListTypesWithAttribute(typeof(TaskType));
            foreach (Type taskType in taskTypes)
            {
                TaskType taskTypeAttr = taskType.GetCustomAttribute<TaskType>();
                this.taskTypes.Add(taskTypeAttr.mainType);
            }
        }

        // 从数据库加载指定任务线的任务到玩家任务系统
        private void LoadTaskFromDb(GM_PlayerEntity entity, int taskLineType, Gametask[] taskSet)
        {
            if (taskSet == null || taskSet.Length <= 0)
            {
                return;
            }

            GM_TaskLine taskLine = new GM_TaskLine();
            taskLine.taskLineType = taskLineType;
            taskLine.taskSet = new List<GM_Task>();

            for (int i = 0; i < taskSet.Length; i++)
            {
                // 过滤不在当前任务线范围内的任务
                if (taskSet[i].tid < taskLineType || taskSet[i].tid > taskLineType + 99999)
                {
                    continue;
                }

                // 只处理未开启和正在进行的任务
                if (taskSet[i].status != (int)TaskStatus.UnOpended &&
                    taskSet[i].status != (int)TaskStatus.Starting)
                {
                    continue;
                }

                GM_Task task = new GM_Task();
                task.dbTaskInst = taskSet[i];
                task.taskProgressData = null;
                task.taskDesic = "";

                // 通过规则处理器工厂获取任务数据解码方法
                MethodInfo method = RuleProcesserFactory.GetProcesser((int)RuleType.Task, "TaskDataDecoder", taskSet[i].tid, (taskSet[i].tid / 100000) * 100000);
                if (method != null)
                {
                    // 调用解码方法对任务数据进行解码
                    method.Invoke(null, new object[] { task });
                }

                taskLine.taskSet.Add(task);
            }

            // 将任务线添加到玩家的任务系统中
            entity.uTask.taskLineSet.TryAdd(taskLineType, taskLine);
        }

        // 尝试加载下一个任务到玩家的任务系统
        private void LoadNextTask(GM_PlayerEntity entity, int taskLineType, int nextTaskId)
        {
            GM_TaskLine taskLine = null;
            // 尝试从玩家的任务系统中获取当前任务线
            entity.uTask.taskLineSet.TryGetValue((long)taskLineType, out taskLine);

            // 通过规则处理器工厂获取任务开启检查方法
            MethodInfo method = null;
            method = RuleProcesserFactory.GetProcesser((int)RuleType.Task, "TaskIsStarted", taskLineType, (taskLineType / 100000) * 100000);
            if (method == null)
            {
                this.logger.Error($"TaskIsStartedFuncs key is null: {taskLineType}");
                return;
            }

            // 调用任务开启检查方法，判断是否可以开启下一个任务
            GM_Task nextTask = (GM_Task)method.Invoke(null, new object[] { entity, taskLineType, nextTaskId });
            if (nextTask != null)
            {
                if (taskLine == null)
                {
                    taskLine = new GM_TaskLine();
                    taskLine.taskLineType = taskLineType;
                    // 将新任务线添加到玩家任务系统
                    entity.uTask.taskLineSet.TryAdd(taskLineType, taskLine);
                }

                taskLine.taskSet.Add(nextTask);

                // 将新任务更新或插入到数据库
                this.UpdateOrInsertTaskToDb(entity, nextTask, true);
            }
        }

        // 检查并启动任务
        public void CheckAndStartTask(GM_PlayerEntity entity, int taskLineType, Gametask[] taskSet)
        {
            // 初始下一个任务ID为任务线起始ID
            int nextTaskId = taskLineType;

            for (int i = 0; i < taskSet.Length; i++)
            {
                // 过滤不在当前任务线范围内的任务
                if (taskSet[i].tid < taskLineType || taskSet[i].tid > (taskLineType + 99999))
                {
                    continue;
                }

                // 如果有正在进行的任务，直接返回
                if (taskSet[i].status == (int)TaskStatus.Starting)
                {
                    return;
                }
                else if (taskSet[i].status > (int)TaskStatus.Starting)
                {
                    // 更新下一个任务ID
                    nextTaskId = taskSet[i].tid;
                }
            }

            nextTaskId++;

            // 尝试加载下一个任务
            this.LoadNextTask(entity, taskLineType, nextTaskId);
        }

        // 从数据库加载玩家的所有任务
        public void LoadTasksFormDb(GM_PlayerEntity entity)
        {
            // 从数据库查询玩家的所有任务
            Gametask[] taskSet = DBService.Instance.GetGameInstance().Queryable<Gametask>().Where(it => (it.uid == entity.uPlayer.playerInfo.id)).ToArray();
            for (int i = 0; i < this.taskTypes.Count; i++)
            {
                // 加载当前任务线的任务
                this.LoadTaskFromDb(entity, this.taskTypes[i], taskSet);
                // 检查并启动当前任务线的任务
                this.CheckAndStartTask(entity, this.taskTypes[i], taskSet);
            }
        }

        // 获取当前任务线正在进行的任务
        public GM_Task GetCurrectTaskData(GM_PlayerEntity entity, int taskLineType)
        {
            GM_TaskLine taskLine = null;
            // 尝试从玩家任务系统中获取当前任务线
            entity.uTask.taskLineSet.TryGetValue(taskLineType, out taskLine);
            if (taskLine == null)
            {
                return null;
            }

            for (int i = 0; i < taskLine.taskSet.Count; i++)
            {
                // 找到正在进行的任务并返回
                if (taskLine.taskSet[i].dbTaskInst.status == (int)TaskStatus.Starting)
                {
                    return taskLine.taskSet[i];
                }
            }

            return null;
        }

        // 更新任务进度
        public void UpdateTaskProgress(GM_PlayerEntity entity, GM_Task curTask, string key, object value)
        {
            // 通过规则处理器工厂获取任务进度更新方法
            MethodInfo method = null;
            method = RuleProcesserFactory.GetProcesser((int)RuleType.Task, "UpdateProgress", curTask.dbTaskInst.tid, (curTask.dbTaskInst.tid / 100000) * 100000);
            if (method != null)
            {
                // 调用任务进度更新方法
                method.Invoke(null, new object[] { curTask, key, value });
            }

            // 检查任务是否完成
            if (!this.CheckTaskIsCompleted(entity, curTask))
            {
                // 若未完成，更新任务到数据库
                GM_TaskMgr.Instance.UpdateOrInsertTaskToDb(entity, curTask);
            }
        }

        // 更新或插入任务到数据库
        public void UpdateOrInsertTaskToDb(GM_PlayerEntity entity, GM_Task curTask, bool isInsert = false)
        {
            // 通过规则处理器工厂获取任务数据编码方法
            MethodInfo method = null;
            method = RuleProcesserFactory.GetProcesser((int)RuleType.Task, "TaskDataEncoder", curTask.dbTaskInst.tid, (curTask.dbTaskInst.tid / 100000) * 100000);

            if (method != null)
            {
                // 调用编码方法对任务进度数据进行编码
                curTask.dbTaskInst.TaskData = (byte[])method.Invoke(null, new object[] { curTask.taskProgressData });
                if (isInsert)
                {
                    // 插入任务到数据库
                    InsertToDb(curTask.dbTaskInst);
                }
                else
                {
                    // 更新任务到数据库
                    UpdateToDb(curTask.dbTaskInst);
                }
            }
            else
            {
                this.logger.Error($"TaskMgr: Unknow Encoder Type: {curTask.dbTaskInst.tid}");
            }
        }

        // 将任务更新到数据库
        private void UpdateToDb(Gametask dbInst)
        {
            // 异步更新任务到数据库
            DBService.Instance.GetGameInstance().Updateable(dbInst).Where(it => it.id == dbInst.id).ExecuteCommandAsync();
        }

        // 将任务插入到数据库
        private void InsertToDb(Gametask dbInst)
        {
            // 异步插入任务到数据库
            DBService.Instance.GetGameInstance().Insertable(dbInst).ExecuteCommandAsync();
        }

        // 检查任务是否完成
        public bool CheckTaskIsCompleted(GM_PlayerEntity player, GM_Task curTask)
        {
            // 通过规则处理器工厂获取任务完成检查方法
            MethodInfo method = null;
            method = RuleProcesserFactory.GetProcesser((int)RuleType.Task, "TaskIsCompleted", curTask.dbTaskInst.tid, (curTask.dbTaskInst.tid / 100000) * 100000);
            if (method != null)
            {
                if ((bool)(method.Invoke(null, new object[] { player, curTask })))
                {
                    // 任务完成，更新任务状态
                    curTask.dbTaskInst.status = (int)TaskStatus.Complete;
                    // 执行任务完成后的操作
                    this.DoTaskCompletedAction(player, curTask);
                    return true;
                }
            }
            else
            {
                this.logger.Error($"TaskIsCompletedFuncs key is null: {curTask.dbTaskInst.tid}");
            }

            return false;
        }

        // 执行任务完成后的操作
        private void DoTaskCompletedAction(GM_PlayerEntity player, GM_Task curTask)
        {
            // 通过规则处理器工厂获取任务完成处理方法
            MethodInfo method = null;
            method = RuleProcesserFactory.GetProcesser((int)RuleType.Task, "TaskOnCompleted", curTask.dbTaskInst.tid, (curTask.dbTaskInst.tid / 100000) * 100000);
            if (method != null)
            {
                // 调用任务完成处理方法
                method.Invoke(null, new object[] { player, curTask });
            }
            else
            {
                this.logger.Error($"DoTaskCompletedAction key is null: {curTask.dbTaskInst.tid}");
            }

            // 更新任务状态为已处理完成
            curTask.dbTaskInst.status = (int)TaskStatus.EndComplete;
            // 更新任务到数据库
            this.UpdateToDb(curTask.dbTaskInst);

            // 尝试加载下一个任务
            int defaultKey = (curTask.dbTaskInst.tid / 100000) * 100000;
            this.LoadNextTask(player, defaultKey, curTask.dbTaskInst.tid + 1);
        }

        // 拉取符合条件的任务列表
        public List<GM_Task> PullingTaskList(GM_PlayerEntity entity, int maskTypeId)
        {
            List<GM_Task> tasks = new List<GM_Task>();
            for (int i = 0; i < this.taskTypes.Count; i++)
            {
                GM_TaskLine taskLine = null;
                // 尝试从玩家任务系统中获取当前任务线
                entity.uTask.taskLineSet.TryGetValue(this.taskTypes[i], out taskLine);
                if (taskLine == null)
                {
                    continue;
                }

                for (int j = 0; j < taskLine.taskSet.Count; j++)
                {
                    // 过滤掉已完成和已处理完成的任务
                    if (taskLine.taskSet[j].dbTaskInst.status != (int)TaskStatus.Starting &&
                        taskLine.taskSet[j].dbTaskInst.status != (int)TaskStatus.UnOpended)
                    {
                        continue;
                    }

                    tasks.Add(taskLine.taskSet[j]);
                }
            }

            return tasks;
        }
    }
}