// 引入 Framework 框架的网络相关命名空间，可能包含网络会话、连接等功能
using Framework.Core.Net;
// 引入 Framework 框架的序列化命名空间，提供序列化和反序列化的功能
using Framework.Core.Serializer;
// 引入 Framework 框架的工具命名空间，包含一些通用的工具类和方法
using Framework.Core.Utils;
// 引入游戏数据消息相关的命名空间，包含各种请求和响应消息类
using Game.Datas.Messages;
// 引入游戏模块相关的命名空间，包含具体的业务逻辑模块
using Game.Entries.Modules;

namespace Game.Entries.Controllers
{
    /// <summary>
    /// 认证控制器类，负责处理与认证相关的请求。
    /// 使用 [Controller] 特性标记该类为控制器，用于接收和分发认证相关的请求。
    /// </summary>
    [Controller]
    public class AuthController
    {
        /// <summary>
        /// NLog 日志记录器实例，用于记录该控制器类的日志信息。
        /// 通过 NLog.LogManager.GetCurrentClassLogger() 方法获取当前类的日志记录器。
        /// </summary>
        static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 处理游客登录请求的方法。
        /// 使用 [RequestMapping] 特性标记该方法为请求处理方法。
        /// 接收一个 IdSession 对象和一个 ReqGuestLogin 请求对象，
        /// 将请求委托给 AuthModule 实例的 HandlerReqGuestLogin 方法进行处理，并返回处理结果。
        /// </summary>
        /// <param name="s">网络会话对象，包含客户端的连接信息。</param>
        /// <param name="req">游客登录请求对象，包含游客登录所需的信息。</param>
        /// <returns>处理游客登录请求的结果对象。</returns>
        [RequestMapping]
        public object DoReqGuestLogin(IdSession s, ReqGuestLogin req) {
            return AuthModule.Instance.HandlerReqGuestLogin(s, req);
        }

        /// <summary>
        /// 处理用户注册请求的方法。
        /// 使用 [RequestMapping] 特性标记该方法为请求处理方法。
        /// 接收一个 IdSession 对象和一个 ReqRegisterUser 请求对象，
        /// 将请求委托给 AuthModule 实例的 HandlerReqRegisterUser 方法进行处理，并返回处理结果。
        /// </summary>
        /// <param name="s">网络会话对象，包含客户端的连接信息。</param>
        /// <param name="req">用户注册请求对象，包含用户注册所需的信息。</param>
        /// <returns>处理用户注册请求的结果对象。</returns>
        [RequestMapping]
        public object DoReqRegisterUser(IdSession s, ReqRegisterUser req) {
            return AuthModule.Instance.HandlerReqRegisterUser(s, req);
        }

        /// <summary>
        /// 处理用户登录请求的方法。
        /// 使用 [RequestMapping] 特性标记该方法为请求处理方法。
        /// 接收一个 IdSession 对象和一个 ReqUserLogin 请求对象，
        /// 将请求委托给 AuthModule 实例的 HandlerReqUserLogin 方法进行处理，并返回处理结果。
        /// </summary>
        /// <param name="s">网络会话对象，包含客户端的连接信息。</param>
        /// <param name="req">用户登录请求对象，包含用户登录所需的信息。</param>
        /// <returns>处理用户登录请求的结果对象。</returns>
        [RequestMapping]
        public object DoReqUserLogin(IdSession s, ReqUserLogin req) {
            return AuthModule.Instance.HandlerReqUserLogin(s, req);
        }

        /// <summary>
        /// 处理游客升级请求的方法。
        /// 使用 [RequestMapping] 特性标记该方法为请求处理方法。
        /// 接收一个 IdSession 对象和一个 ReqGuestUpgrade 请求对象，
        /// 将请求委托给 AuthModule 实例的 HandlerReqGuestUpgrade 方法进行处理，并返回处理结果。
        /// </summary>
        /// <param name="s">网络会话对象，包含客户端的连接信息。</param>
        /// <param name="req">游客升级请求对象，包含游客升级所需的信息。</param>
        /// <returns>处理游客升级请求的结果对象。</returns>
        [RequestMapping]
        public object DoReqGuestUpgrade(IdSession s, ReqGuestUpgrade req) {
            return AuthModule.Instance.HandlerReqGuestUpgrade(s, req);
        }
    }
}