using Framework.Core.Net;
using Framework.Core.task;
using Framework.Core.Utils;
using Game.Datas.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Game.LogicServer
{
    /// <summary>
    /// 使用 LogicServerMeta 特性标记 LogicServer 类，将其与 Module.SCENE 模块关联。
    /// 该特性可能用于标识逻辑服务器所属的模块类型，方便进行模块化管理。
    /// </summary>
    [LogicServerMeta((int)Module.SCENE)]
    public partial class LogicServer : BaseLogicServer
    {
        /// <summary>
        /// 处理 ReqTestLogicCmdEcho 请求的方法。
        /// 使用 RequestMapping 特性标记，表明这是一个请求处理方法。
        /// </summary>
        /// <param name="s">会话对象，包含与客户端连接的相关信息，如账号 ID、玩家 ID 等。</param>
        /// <param name="req">请求对象，包含客户端发送的具体请求内容。</param>
        /// <returns>响应对象，将请求中的内容原样返回给客户端。</returns>
        [RequestMapping]
        public ResTestLogicCmdEcho OnTestLogicCmdEchoReq(IdSession s, ReqTestLogicCmdEcho req) {
            // 记录日志，表明接收到了 OnTestLogicCmdEchoReq 请求
            logger.Info("OnTestLogicCmdEchoReq");

            // 创建响应对象
            ResTestLogicCmdEcho res = new ResTestLogicCmdEcho();
            // 将请求中的内容复制到响应对象中
            res.content = req.content;
            return res;
        }

        /// <summary>
        /// 服务器启动时调用的方法。
        /// 通常用于进行一些初始化操作，如加载配置、初始化数据库连接、启动定时任务等。
        /// </summary>
        public void OnStart() {
            // 记录服务器启动的日志信息
            logger.Debug("OnUpdate LogicServer OnStart");
        }

        /// <summary>
        /// 服务器更新时调用的方法，会定期执行。
        /// 用于处理一些需要定时更新的逻辑，如游戏场景中的物体移动、状态更新等。
        /// </summary>
        /// <param name="dt">距离上一次更新所经过的时间间隔，单位通常为秒。</param>
        public void OnUpdate(float dt) {
            // 可以根据需要取消注释，记录服务器更新的日志信息，包含时间间隔
            // logger.Debug($"OnUpdate LogicServer Update {dt}");
        }
    }
}