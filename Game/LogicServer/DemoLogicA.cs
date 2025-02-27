using Framework.Core.task;
using Framework.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Game.LogicServer
{
    /// <summary>
    /// 使用 LogicServerMeta 特性标记 DemoLogicA 类，传入参数 (int)200，
    /// 该特性可能用于标识逻辑服务器的某种类型或优先级等信息。
    /// </summary>
    [LogicServerMeta((int)200)]
    public partial class DemoLogicA : BaseLogicServer
    {
        /// <summary>
        /// 此方法在逻辑服务器启动时被调用。
        /// 通常用于进行一些初始化操作，例如初始化数据、启动任务等。
        /// 当前方法中注释掉了日志输出代码，实际未执行具体逻辑。
        /// </summary>
        public void OnStart() {
            // 可以使用日志记录服务器启动信息
            // logger.Debug("DemoLogicA OnStart");
        }

        /// <summary>
        /// 此方法在逻辑服务器更新时被调用，每次更新会传入一个时间间隔 duration。
        /// 一般用于处理服务器的定时更新逻辑，例如游戏中角色的移动、状态更新等。
        /// 当前方法中注释掉了日志输出代码，实际未执行具体逻辑。
        /// </summary>
        /// <param name="duration">距离上一次更新所经过的时间，单位通常为秒。</param>
        public void OnUpdate(float duration) {
            // 可以使用日志记录服务器更新信息
            // logger.Debug("DemoLogicA OnUpdate");
        }
    }
}