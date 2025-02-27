using Game.Datas.GMEntities;
using Game.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core.GM_Task
{
    /// <summary>
    /// DemoATaskRule 类用于定义类型为 200000 的任务规则。
    /// 通过特性标记任务类型和规则模块，包含多个任务处理方法。
    /// </summary>
    [TaskType(200000)]
    [RuleModule((int)RuleType.Task, 200000)]
    class DemoATaskRule
    {
        /// <summary>
        /// 默认的任务数据编码方法，将任务数据编码为字节数组。
        /// 目前只是简单返回 null，需要根据实际需求实现具体编码逻辑。
        /// </summary>
        /// <param name="taskData">需要编码的任务数据对象。</param>
        /// <returns>编码后的字节数组，当前返回 null。</returns>
        [RuleProcesser("TaskDataEncoder", -1)]
        public static byte[] DefaultEncodeTaskData(object taskData)
        {
            return null;
        }

        /// <summary>
        /// 默认的任务数据解码方法，将存储的任务数据解码到任务对象中。
        /// 目前方法体为空，需要根据实际需求实现具体解码逻辑。
        /// </summary>
        /// <param name="task">需要解码数据的任务对象。</param>
        [RuleProcesser("TaskDataDecoder", -1)]
        public static void DefaultDecoderTaskData(GM_Task task)
        {
        }

        /// <summary>
        /// 默认的任务开启检查方法，判断是否可以开启指定任务。
        /// 目前只是简单返回 null，需要根据实际需求实现具体检查逻辑。
        /// </summary>
        /// <param name="entity">玩家实体对象，包含玩家的相关信息。</param>
        /// <param name="taskLineType">任务线类型。</param>
        /// <param name="nextTaskId">下一个任务的 ID。</param>
        /// <returns>如果可以开启任务，返回对应的任务对象；否则返回 null，当前返回 null。</returns>
        [RuleProcesser("TaskIsStarted", -1)]
        public static GM_Task DefaultCheckIfStartTask(GM_PlayerEntity entity, int taskLineType, int nextTaskId)
        {
            return null;
        }

        /// <summary>
        /// 默认的任务完成检查方法，判断指定任务是否已经完成。
        /// 目前只是简单返回 false，需要根据实际需求实现具体检查逻辑。
        /// </summary>
        /// <param name="entity">玩家实体对象，包含玩家的相关信息。</param>
        /// <param name="task">需要检查是否完成的任务对象。</param>
        /// <returns>如果任务完成返回 true，否则返回 false，当前返回 false。</returns>
        [RuleProcesser("TaskIsCompleted", -1)]
        public static bool DefaultCheckIsCompleted(GM_PlayerEntity entity, GM_Task task)
        {
            return false;
        }

        /// <summary>
        /// 默认的任务完成处理方法，在任务完成后执行相应的操作。
        /// 目前方法体为空，需要根据实际需求实现具体处理逻辑。
        /// </summary>
        /// <param name="entity">玩家实体对象，包含玩家的相关信息。</param>
        /// <param name="task">已经完成的任务对象。</param>
        [RuleProcesser("TaskOnCompleted", -1)]
        public static void DefaultDoTaskCompleted(GM_PlayerEntity entity, GM_Task task)
        {
        }

        /// <summary>
        /// 默认的任务进度更新方法，根据传入的键和值更新任务的进度。
        /// 目前方法体为空，需要根据实际需求实现具体更新逻辑。
        /// </summary>
        /// <param name="curTask">需要更新进度的当前任务对象。</param>
        /// <param name="key">进度更新的键，用于标识更新的具体内容。</param>
        /// <param name="value">进度更新的值。</param>
        [RuleProcesser("UpdateProgress", -1)]
        public static void DefaultTaskUpdateProgress(GM_Task curTask, string key, object value)
        {
        }
    }
}