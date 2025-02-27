using System;

namespace Framework.Core.task
{
    /// <summary>
    /// 抽象类 AbstractDistributeTask 作为分布式任务的基类，
    /// 为具体的任务类提供了基础结构和公共方法。
    /// 所有具体的分布式任务类都应继承自此类并实现 DoAction 方法。
    /// </summary>
    public abstract class AbstractDistributeTask
    {
        /// <summary>
        /// 任务的分发键，与 Session 中的唯一键对应，
        /// 用于将任务分发到特定的处理线程或队列。
        /// </summary>
        public long distributeKey;

        /// <summary>
        /// 每个任务的起始时间戳，记录任务开始执行的时间。
        /// </summary>
        private long startMillis;

        /// <summary>
        /// 每个任务结束的时间戳，记录任务执行完成的时间。
        /// </summary>
        private long endMillis;

        /// <summary>
        /// 抽象方法，具体的任务逻辑应由子类实现。
        /// 该方法定义了任务执行时要完成的具体操作。
        /// </summary>
        public abstract void DoAction();

        /// <summary>
        /// 获取任务类的名称。
        /// </summary>
        /// <returns>任务类的名称。</returns>
        public string getName() => GetType().Name;

        /// <summary>
        /// 获取任务的起始时间戳。
        /// </summary>
        /// <returns>任务的起始时间戳（毫秒）。</returns>
        public long getStartMillis() => startMillis;

        /// <summary>
        /// 获取任务的结束时间戳。
        /// </summary>
        /// <returns>任务的结束时间戳（毫秒）。</returns>
        public long getEndMillis() => endMillis;

        /// <summary>
        /// 标记任务的起始时间戳。
        /// 使用 UTC 时间计算从 1970 年 1 月 1 日开始到当前时间的毫秒数。
        /// </summary>
        public void markStartMillis() => startMillis = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

        /// <summary>
        /// 标记任务的结束时间戳。
        /// 使用 UTC 时间计算从 1970 年 1 月 1 日开始到当前时间的毫秒数。
        /// </summary>
        public void markEndMillis() => endMillis = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
    }
}