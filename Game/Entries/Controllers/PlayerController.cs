// 引入框架的网络相关命名空间，提供网络会话等功能支持
using Framework.Core.Net;
// 引入框架的序列化命名空间，用于实现数据的序列化和反序列化
using Framework.Core.Serializer;
// 引入框架的工具命名空间，包含一些通用工具类和方法
using Framework.Core.Utils;
// 引入游戏数据消息的命名空间，包含各种玩家相关的请求消息类
using Game.Datas.Messages;
// 引入游戏模块的命名空间，包含具体处理业务逻辑的模块
using Game.Entries.Modules;

namespace Game.Entries.Controllers
{
    /// <summary>
    /// 玩家控制器类，负责处理与玩家相关的各种请求。
    /// 通过 [Controller] 特性标记为控制器类，将玩家请求转发给 PlayerModule 处理。
    /// </summary>
    [Controller]
    public class PlayerController
    {
        /// <summary>
        /// NLog 日志记录器实例，用于记录该控制器类的日志信息。
        /// 通过 NLog.LogManager.GetCurrentClassLogger() 方法获取当前类的日志记录器。
        /// </summary>
        static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 处理拉取玩家数据请求的方法。
        /// 使用 [RequestMapping] 特性标记该方法为请求处理方法。
        /// 接收一个 IdSession 对象和一个 ReqPullingPlayerData 请求对象，
        /// 将请求委托给 PlayerModule 实例的 HandlerReqPullingPlayerData 方法进行处理，并返回处理结果。
        /// </summary>
        /// <param name="s">网络会话对象，包含客户端的连接信息。</param>
        /// <param name="req">拉取玩家数据的请求对象，包含拉取数据所需的信息。</param>
        /// <returns>处理拉取玩家数据请求的结果对象。</returns>
        [RequestMapping]
        public object DoReqPullingPlayerData(IdSession s, ReqPullingPlayerData req) {
            return PlayerModule.Instance.HandlerReqPullingPlayerData(s, req);
        }

        /// <summary>
        /// 处理选择玩家请求的方法。
        /// 使用 [RequestMapping] 特性标记该方法为请求处理方法。
        /// 接收一个 IdSession 对象和一个 ReqSelectPlayer 请求对象，
        /// 将请求委托给 PlayerModule 实例的 HandlerReqSelectPlayer 方法进行处理，并返回处理结果。
        /// </summary>
        /// <param name="s">网络会话对象，包含客户端的连接信息。</param>
        /// <param name="req">选择玩家的请求对象，包含选择玩家所需的信息。</param>
        /// <returns>处理选择玩家请求的结果对象。</returns>
        [RequestMapping]
        public object DoReqSelectPlayer(IdSession s, ReqSelectPlayer req) {
            return PlayerModule.Instance.HandlerReqSelectPlayer(s, req);
        }

        /// <summary>
        /// 处理领取登录奖励请求的方法。
        /// 使用 [RequestMapping] 特性标记该方法为请求处理方法。
        /// 接收一个 IdSession 对象和一个 ReqRecvLoginBonues 请求对象，
        /// 将请求委托给 PlayerModule 实例的 HandlerRecvLoginBonues 方法进行处理，并返回处理结果。
        /// </summary>
        /// <param name="s">网络会话对象，包含客户端的连接信息。</param>
        /// <param name="req">领取登录奖励的请求对象，包含领取奖励所需的信息。</param>
        /// <returns>处理领取登录奖励请求的结果对象。</returns>
        [RequestMapping]
        public object DoReqRecvLoginBonues(IdSession s, ReqRecvLoginBonues req) {
            return PlayerModule.Instance.HandlerRecvLoginBonues(s, req);
        }

        /// <summary>
        /// 处理拉取奖励列表请求的方法。
        /// 使用 [RequestMapping] 特性标记该方法为请求处理方法。
        /// 接收一个 IdSession 对象和一个 ReqPullingBonuesList 请求对象，
        /// 将请求委托给 PlayerModule 实例的 HandlerReqPullingBonuesList 方法进行处理，并返回处理结果。
        /// </summary>
        /// <param name="s">网络会话对象，包含客户端的连接信息。</param>
        /// <param name="req">拉取奖励列表的请求对象，包含拉取奖励列表所需的信息。</param>
        /// <returns>处理拉取奖励列表请求的结果对象。</returns>
        [RequestMapping]
        public object DoReqPullingBonuesList(IdSession s, ReqPullingBonuesList req) {
            return PlayerModule.Instance.HandlerReqPullingBonuesList(s, req);
        }

        /// <summary>
        /// 处理领取奖励请求的方法。
        /// 使用 [RequestMapping] 特性标记该方法为请求处理方法。
        /// 接收一个 IdSession 对象和一个 ReqRecvBonues 请求对象，
        /// 将请求委托给 PlayerModule 实例的 HandlerReqRecvBonues 方法进行处理，并返回处理结果。
        /// </summary>
        /// <param name="s">网络会话对象，包含客户端的连接信息。</param>
        /// <param name="req">领取奖励的请求对象，包含领取奖励所需的信息。</param>
        /// <returns>处理领取奖励请求的结果对象。</returns>
        [RequestMapping]
        public object DoReqRecvBonues(IdSession s, ReqRecvBonues req) {
            return PlayerModule.Instance.HandlerReqRecvBonues(s, req);
        }

        /// <summary>
        /// 处理拉取任务列表请求的方法。
        /// 使用 [RequestMapping] 特性标记该方法为请求处理方法。
        /// 接收一个 IdSession 对象和一个 ReqPullingTaskList 请求对象，
        /// 将请求委托给 PlayerModule 实例的 HandlerReqPullingTaskList 方法进行处理，并返回处理结果。
        /// </summary>
        /// <param name="s">网络会话对象，包含客户端的连接信息。</param>
        /// <param name="req">拉取任务列表的请求对象，包含拉取任务列表所需的信息。</param>
        /// <returns>处理拉取任务列表请求的结果对象。</returns>
        [RequestMapping]
        public object DoReqPullingTaskList(IdSession s, ReqPullingTaskList req) {
            return PlayerModule.Instance.HandlerReqPullingTaskList(s, req);
        }

        /// <summary>
        /// 处理测试领取奖励物品请求的方法。
        /// 使用 [RequestMapping] 特性标记该方法为请求处理方法。
        /// 接收一个 IdSession 对象和一个 ReqTestGetGoods 请求对象，
        /// 将请求委托给 PlayerModule 实例的 HandlerReqTestGetGoods 方法进行处理，并返回处理结果。
        /// </summary>
        /// <param name="s">网络会话对象，包含客户端的连接信息。</param>
        /// <param name="req">测试领取奖励物品的请求对象，包含领取物品所需的信息。</param>
        /// <returns>处理测试领取奖励物品请求的结果对象。</returns>
        [RequestMapping]
        public object DoReqTestGetBonuesGoods(IdSession s, ReqTestGetGoods req) {
            return PlayerModule.Instance.HandlerReqTestGetGoods(s, req);
        }

        /// <summary>
        /// 处理测试更新物品请求的方法。
        /// 注意：方法中存在拼写错误，参数类名 ReqTestUpdateGooods 可能应为 ReqTestUpdateGoods，
        /// 调用的处理方法 HandlerReqTestUpdatesGoods 可能应为 HandlerReqTestUpdateGoods。
        /// 使用 [RequestMapping] 特性标记该方法为请求处理方法。
        /// 接收一个 IdSession 对象和一个 ReqTestUpdateGooods 请求对象，
        /// 将请求委托给 PlayerModule 实例的 HandlerReqTestUpdatesGoods 方法进行处理，并返回处理结果。
        /// </summary>
        /// <param name="s">网络会话对象，包含客户端的连接信息。</param>
        /// <param name="req">测试更新物品的请求对象，包含更新物品所需的信息。</param>
        /// <returns>处理测试更新物品请求的结果对象。</returns>
        [RequestMapping]
        public object DoReqTestUpdateGoods(IdSession s, ReqTestUpdateGooods req) {
            return PlayerModule.Instance.HandlerReqTestUpdatesGoods(s, req);
        }

        /// <summary>
        /// 处理拉取邮件消息请求的方法。
        /// 使用 [RequestMapping] 特性标记该方法为请求处理方法。
        /// 接收一个 IdSession 对象和一个 ReqPullingMailMsg 请求对象，
        /// 将请求委托给 PlayerModule 实例的 HandlerReqPullingMailMsg 方法进行处理，并返回处理结果。
        /// </summary>
        /// <param name="s">网络会话对象，包含客户端的连接信息。</param>
        /// <param name="req">拉取邮件消息的请求对象，包含拉取邮件消息所需的信息。</param>
        /// <returns>处理拉取邮件消息请求的结果对象。</returns>
        [RequestMapping]
        public object DoReqPullingMailMsg(IdSession s, ReqPullingMailMsg req) {
            return PlayerModule.Instance.HandlerReqPullingMailMsg(s, req);
        }

        /// <summary>
        /// 处理更新邮件消息请求的方法。
        /// 使用 [RequestMapping] 特性标记该方法为请求处理方法。
        /// 接收一个 IdSession 对象和一个 ReqUpdateMailMsg 请求对象，
        /// 将请求委托给 PlayerModule 实例的 HandlerReqUpdateMailMsg 方法进行处理，并返回处理结果。
        /// </summary>
        /// <param name="s">网络会话对象，包含客户端的连接信息。</param>
        /// <param name="req">更新邮件消息的请求对象，包含更新邮件消息所需的信息。</param>
        /// <returns>处理更新邮件消息请求的结果对象。</returns>
        [RequestMapping]
        public object DoReqUpdateMailMsg(IdSession s, ReqUpdateMailMsg req) {
            return PlayerModule.Instance.HandlerReqUpdateMailMsg(s, req);
        }

        /// <summary>
        /// 处理拉取排行榜数据请求的方法。
        /// 使用 [RequestMapping] 特性标记该方法为请求处理方法。
        /// 接收一个 IdSession 对象和一个 ReqPullingRank 请求对象，
        /// 将请求委托给 PlayerModule 实例的 HandlerReqPullingRank 方法进行处理，并返回处理结果。
        /// </summary>
        /// <param name="s">网络会话对象，包含客户端的连接信息。</param>
        /// <param name="req">拉取排行榜数据的请求对象，包含拉取排行榜数据所需的信息。</param>
        /// <returns>处理拉取排行榜数据请求的结果对象。</returns>
        [RequestMapping]
        public object DoReqPullingRank(IdSession s, ReqPullingRank req) {
            return PlayerModule.Instance.HandlerReqPullingRank(s, req);
        }

        /// <summary>
        /// 处理拉取背包数据请求的方法。
        /// 使用 [RequestMapping] 特性标记该方法为请求处理方法。
        /// 接收一个 IdSession 对象和一个 ReqPullingPackData 请求对象，
        /// 将请求委托给 PlayerModule 实例的 HandlerReqPullingPackData 方法进行处理，并返回处理结果。
        /// </summary>
        /// <param name="s">网络会话对象，包含客户端的连接信息。</param>
        /// <param name="req">拉取背包数据的请求对象，包含拉取背包数据所需的信息。</param>
        /// <returns>处理拉取背包数据请求的结果对象。</returns>
        [RequestMapping]
        public object DoReqPullingPackData(IdSession s, ReqPullingPackData req) {
            return PlayerModule.Instance.HandlerReqPullingPackData(s, req);
        }

        /// <summary>
        /// 处理产品交换请求的方法。
        /// 使用 [RequestMapping] 特性标记该方法为请求处理方法。
        /// 接收一个 IdSession 对象和一个 ReqExchangeProduct 请求对象，
        /// 将请求委托给 PlayerModule 实例的 HandlerReqExchangeProduct 方法进行处理，并返回处理结果。
        /// </summary>
        /// <param name="s">网络会话对象，包含客户端的连接信息。</param>
        /// <param name="req">产品交换的请求对象，包含产品交换所需的信息。</param>
        /// <returns>处理产品交换请求的结果对象。</returns>
        [RequestMapping]
        public object DoReqExchangeProduct(IdSession s, ReqExchangeProduct req) {
            return PlayerModule.Instance.HandlerReqExchangeProduct(s, req);
        }
    }
}