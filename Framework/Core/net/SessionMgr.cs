using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using System.Net;
using System.Threading;

namespace Framework.Core.Net
{
    /// <summary>
    /// SessionMgr 类是一个会话管理器，采用单例模式，负责管理 IdSession 对象。
    /// 它提供了创建、获取和初始化会话的功能，确保每个客户端连接都有唯一的会话标识。
    /// </summary>
    public class SessionMgr
    {
        // 单例实例，确保整个应用程序中只有一个 SessionMgr 实例
        public static SessionMgr Instance = new SessionMgr();

        // AttributeKey 用于在 IChannel 上存储和获取 IdSession 对象
        // 每个 IChannel 可以关联一个 IdSession，通过这个键来标识
        private AttributeKey<IdSession> SESSION_KEY = AttributeKey<IdSession>.ValueOf("session");

        // 用于生成唯一的分布式键（distributeKey）
        // 使用 Interlocked 类保证在多线程环境下该变量的原子性递增
        private long autoId = 0;

        /// <summary>
        /// 初始化会话管理器，将 autoId 重置为 0。
        /// 在某些情况下，可能需要重新初始化会话管理器，调用此方法重置 autoId。
        /// </summary>
        public void Init()
        {
            this.autoId = 0;
        }

        /// <summary>
        /// 创建一个新的 IdSession 对象，并将其关联到指定的 IChannel 上。
        /// </summary>
        /// <param name="channel">与新会话关联的 IChannel。</param>
        /// <param name="isWebSocket">指示该会话是否为 WebSocket 会话，默认为 false。</param>
        /// <returns>新创建的 IdSession 对象。</returns>
        public IdSession CreateIdSession(IChannel channel, bool isWebSocket = false)
        {
            // 创建一个新的 IdSession 实例
            IdSession session = new IdSession();
            // 初始化账号 ID 为 0
            session.accountId = 0;
            // 初始化玩家 ID 为 0
            session.playerId = 0;

            // 使用 Interlocked 类原子性地递增 autoId，并将递增后的值赋给 distributeKey
            // 确保每个会话有唯一的分布式键
            long id = Interlocked.Increment(ref autoId);
            session.distributeKey = id;

            // 将传入的 IChannel 赋值给 session 的 client 属性
            session.client = channel;
            // 根据传入的参数设置是否为 WebSocket 会话
            session.isWebSocket = isWebSocket;

            // 通过 SESSION_KEY 获取 IChannel 上的属性
            IAttribute<IdSession> sessionAttr = channel.GetAttribute(SESSION_KEY);
            // 使用 CompareAndSet 方法将新创建的 IdSession 对象存储到 IChannel 上
            // 如果该属性已经存在，则不进行设置，确保每个 IChannel 只关联一个 IdSession
            sessionAttr.CompareAndSet(null, session);

            // 返回新创建的 IdSession 对象
            return session;
        }

        /// <summary>
        /// 根据指定的 IChannel 获取关联的 IdSession 对象。
        /// </summary>
        /// <param name="channel">用于查找关联 IdSession 的 IChannel。</param>
        /// <returns>关联的 IdSession 对象，如果不存在则返回 null。</returns>
        public IdSession GetSessionBy(IChannel channel)
        {
            // 通过 SESSION_KEY 获取 IChannel 上的属性
            IAttribute<IdSession> sessionAttr = channel.GetAttribute(SESSION_KEY);
            // 返回存储在该属性中的 IdSession 对象，如果不存在则返回 null
            return sessionAttr.Get();
        }
    }
}