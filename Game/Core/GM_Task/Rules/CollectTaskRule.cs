using Framework.Core.Utils;
using Game.Datas.DBEntities;
using Game.Datas.Excels;
using Game.Datas.GMEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using System.IO;
using System.Reflection;
using Game.Utils;
using Game.Core.Caches;

namespace Game.Core.GM_Task
{
    /// <summary>
    /// 收集任务进度数据类，用于存储收集任务的进度信息。
    /// 使用 ProtoContract 特性，以便使用 ProtoBuf 进行序列化和反序列化。
    /// </summary>
    [ProtoContract]
    class CollectTaskProgressData
    {
        /// <summary>
        /// 收集的钻石数量，ProtoMember 标记为 1 且为必需字段。
        /// </summary>
        [ProtoMember(1, IsRequired = true)]
        public int damond = 0;

        /// <summary>
        /// 收集的书籍数量，ProtoMember 标记为 2 且为必需字段。
        /// </summary>
        [ProtoMember(2, IsRequired = true)]
        public int book = 0;

        /// <summary>
        /// 多个任务完成条件的键值对，键为条件名称，值为条件所需数量。
        /// </summary>
        public Dictionary<string, int> conds = new Dictionary<string, int>();
        /// <summary>
        /// 任务完成后的奖励键值对，键为奖励类型，值为奖励数量。
        /// </summary>
        public Dictionary<string, int> completeBonues = new Dictionary<string, int>();
    }

    /// <summary>
    /// 收集任务规则类，处理收集任务的各种规则逻辑。
    /// 使用 TaskType 特性标记任务类型为 100000，RuleModule 特性标记为任务规则模块，编号为 100000。
    /// </summary>
    [TaskType(100000)]
    [RuleModule((int)RuleType.Task, 100000)]  //  任务线的ID号; 
    public class CollectTaskRule
    {
        /// <summary>
        /// 默认的任务数据编码方法，将任务进度数据编码为字节数组。
        /// 使用 RuleProcesser 特性标记为任务数据编码规则，规则编号为 -1。
        /// </summary>
        /// <param name="taskData">要编码的任务进度数据对象。</param>
        /// <returns>编码后的字节数组。</returns>
        [RuleProcesser("TaskDataEncoder", -1)]
        public static byte[] DefaultEncodeTaskData(object taskData)
        {
            // 调用 GameUtils 的 PbObjToBytes 方法，将 CollectTaskProgressData 对象转换为字节数组
            return GameUtils.PbObjToBytes<CollectTaskProgressData>(taskData as CollectTaskProgressData);
        }

        /// <summary>
        /// 默认的任务数据解码方法，将数据库中的任务数据解码并更新任务信息。
        /// 使用 RuleProcesser 特性标记为任务数据解码规则，规则编号为 -1。
        /// </summary>
        /// <param name="task">要解码数据的任务对象。</param>
        [RuleProcesser("TaskDataDecoder", -1)]
        public static void DefaultDecoderTaskData(GM_Task task)
        {
            // 调用 GameUtils 的 BytesToPbObj 方法，将数据库中的任务数据字节数组转换为 CollectTaskProgressData 对象
            task.taskProgressData = GameUtils.BytesToPbObj<CollectTaskProgressData>(task.dbTaskInst.TaskData);

            // 从 Excel 配置文件中获取当前任务的配置信息
            CollectTask config = (CollectTask)ExcelUtils.GetConfigData<CollectTask>(task.dbTaskInst.tid.ToString());
            if (config != null)
            {
                // 设置任务的描述信息
                task.taskDesic = config.desicFormat;
            }

            // 获取任务进度数据对象
            CollectTaskProgressData data = (CollectTaskProgressData)task.taskProgressData;
            // 解析任务完成条件配置字符串为键值对
            data.conds = GameUtils.ParseStringWithKeyIntValue(config.DoneCond);
            // 解析任务完成奖励配置字符串为键值对
            data.completeBonues = GameUtils.ParseStringWithKeyIntValue(config.Bonues);
        }

        /// <summary>
        /// 默认的任务进度更新方法，根据传入的键和值更新任务进度。
        /// 使用 RuleProcesser 特性标记为任务进度更新规则，规则编号为 -1。
        /// </summary>
        /// <param name="curTask">当前要更新进度的任务对象。</param>
        /// <param name="key">要更新的任务进度字段的键。</param>
        /// <param name="value">要更新的值。</param>
        [RuleProcesser("UpdateProgress", -1)]
        public static void DefaultTaskUpdateProgress(GM_Task curTask, string key, object value)
        {
            // 获取当前任务的进度数据对象
            CollectTaskProgressData item = (CollectTaskProgressData)curTask.taskProgressData;
            // 获取当前字段的旧值，若不存在则默认为 0
            int lastValue = (int)GameUtils.GetFiled(item, key, 0);
            // 增加新的值
            lastValue += ((int)value);
            // 更新字段的值
            GameUtils.SetFiled(item, key, lastValue);
        }

        /// <summary>
        /// 默认的任务开启检查方法，检查是否可以开启下一个任务。
        /// 使用 RuleProcesser 特性标记为任务开启检查规则，规则编号为 -1。
        /// </summary>
        /// <param name="entity">玩家实体对象。</param>
        /// <param name="taskLineType">任务线类型。</param>
        /// <param name="nextTaskId">下一个任务的 ID。</param>
        /// <returns>若可以开启任务，返回新创建的任务对象；否则返回 null。</returns>
        [RuleProcesser("TaskIsStarted", -1)]
        public static GM_Task DefaultCheckIfStartTask(GM_PlayerEntity entity, int taskLineType, int nextTaskId)
        {
            // 从 Excel 配置文件中获取下一个任务的配置信息
            CollectTask config = (CollectTask)ExcelUtils.GetConfigData<CollectTask>(nextTaskId.ToString());
            if (config == null)
            {
                return null;
            }

            // 因为我们是自动开启auto
            // end

            // 创建一个新的任务对象
            GM_Task nextTask = GM_Task.Create(entity.uPlayer.playerInfo.id, nextTaskId);

            // 设置任务的描述信息
            nextTask.taskDesic = config.desicFormat;
            // 创建一个新的任务进度数据对象
            CollectTaskProgressData item = new CollectTaskProgressData();
            // 解析任务完成条件配置字符串为键值对
            item.conds = GameUtils.ParseStringWithKeyIntValue(config.DoneCond);
            // 解析任务完成奖励配置字符串为键值对
            item.completeBonues = GameUtils.ParseStringWithKeyIntValue(config.Bonues);
            // 设置任务的进度数据
            nextTask.taskProgressData = item;

            return nextTask;
        }

        /// <summary>
        /// 默认的任务完成检查方法，检查任务是否完成。
        /// 使用 RuleProcesser 特性标记为任务完成检查规则，规则编号为 -1。
        /// </summary>
        /// <param name="entity">玩家实体对象。</param>
        /// <param name="curTask">当前要检查的任务对象。</param>
        /// <returns>若任务完成返回 true，否则返回 false。</returns>
        [RuleProcesser("TaskIsCompleted", -1)]
        public static bool DefaultCheckIsCompleted(GM_PlayerEntity entity, GM_Task curTask)
        {
            // 从 Excel 配置文件中获取当前任务的配置信息
            CollectTask config = (CollectTask)ExcelUtils.GetConfigData<CollectTask>(curTask.dbTaskInst.tid.ToString());
            if (config == null)
            {
                return false;
            }

            // 获取当前任务的进度数据对象
            CollectTaskProgressData item = (CollectTaskProgressData)curTask.taskProgressData;

            // 遍历任务完成条件的键值对
            foreach (var key in item.conds.Keys)
            {
                // 获取当前条件的实际完成值，若不存在则默认为 0
                int value = (int)GameUtils.GetFiled(item, key, 0);
                // 若实际完成值小于条件所需值，则任务未完成
                if (value < item.conds[key])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 默认的任务完成处理方法，在任务完成后给予玩家奖励并更新数据库。
        /// 使用 RuleProcesser 特性标记为任务完成处理规则，规则编号为 -1。
        /// </summary>
        /// <param name="entity">玩家实体对象。</param>
        /// <param name="task">已完成的任务对象。</param>
        [RuleProcesser("TaskOnCompleted", -1)]
        public static void DefaultDoTaskCompleted(GM_PlayerEntity entity, GM_Task task)
        {
            // 获取已完成任务的进度数据对象
            CollectTaskProgressData item = (CollectTaskProgressData)task.taskProgressData;
            // 你可以根据自己的游戏来写规则，奖励
            // 遍历任务完成奖励的键值对
            foreach (var key in item.completeBonues.Keys)
            {
                // 获取玩家当前奖励类型的属性值，若不存在则默认为 0
                int value = (int)GameUtils.GetProp(entity.uPlayer.playerInfo, key, 0);
                // 增加奖励数量
                value += item.completeBonues[key];
                // 更新玩家的属性值
                GameUtils.SetProp(entity.uPlayer.playerInfo, key, value);
            }

            // 将更新后的玩家信息更新到数据库
            PlayerIDCache.Instance.UpdateDataToDb(entity.uPlayer.playerInfo);
            // end
        }
    }
}