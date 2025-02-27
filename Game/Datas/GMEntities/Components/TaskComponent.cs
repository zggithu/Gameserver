// 引入游戏核心任务相关的命名空间，该命名空间可能包含任务相关的核心类和功能
using Game.Core.GM_Task;
// 引入 System.Collections.Concurrent 命名空间，它提供了线程安全的集合类
using System.Collections.Concurrent;

namespace Game.Datas.GMEntities
{
    /// <summary>
    /// 任务组件结构体，用于管理游戏中玩家的任务数据。
    /// 该结构体包含一个线程安全的字典来存储任务行信息，并提供初始化方法。
    /// </summary>
    public struct TaskComponent
    {

        /// <summary>
        /// 线程安全的字典，用于存储任务行数据。
        /// 键为任务行的唯一标识（long 类型），值为 GM_TaskLine 对象，代表具体的任务行。
        /// </summary>
        public ConcurrentDictionary<long, GM_TaskLine> taskLineSet;

        /// <summary>
        /// 静态方法，用于初始化玩家实体的任务组件。
        /// 该方法会为任务行集合分配内存，并从数据库加载任务数据到玩家实体。
        /// </summary>
        /// <param name="entity">需要初始化任务组件的玩家实体对象。</param>
        public static void Init(GM_PlayerEntity entity) {
            // 为玩家实体的任务组件中的任务行集合分配一个新的线程安全字典实例
            entity.uTask.taskLineSet = new ConcurrentDictionary<long, GM_TaskLine>();
            // 调用 GM_TaskMgr 单例的 LoadTasksFormDb 方法，从数据库加载任务数据到玩家实体
            // 注意：这里的方法名可能存在拼写错误，正确的可能是 LoadTasksFromDb
            GM_TaskMgr.Instance.LoadTasksFormDb(entity);
        }
    }
}