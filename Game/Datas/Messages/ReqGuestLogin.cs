// 引入 Framework 框架的序列化命名空间，提供序列化相关的功能和工具类
using Framework.Core.Serializer;
// 引入 Framework 框架的工具命名空间，可能包含一些通用的工具方法和实用类
using Framework.Core.Utils;
// 引入 ProtoBuf 命名空间，用于支持 Protocol Buffers 序列化协议
using ProtoBuf;

namespace Game.Datas.Messages
{
    /// <summary>
    /// 表示游客登录请求的消息类。
    /// 该类使用 Protocol Buffers 进行序列化，并通过消息元数据指定所属模块和命令，
    /// 包含游客登录所需的关键信息，用于在游戏系统中发起游客登录请求。
    /// </summary>
    [ProtoContract]
    /// <summary>
    /// 标记该消息所属的模块为认证模块（Module.AUTH），具体命令为游客登录请求（Cmd.eGuestLoginReq）。
    /// 这有助于消息在系统中进行准确的路由和处理。
    /// </summary>
    [MessageMeta((short)Module.AUTH, (short)Cmd.eGuestLoginReq)]
    public class ReqGuestLogin : Message
    {
        /// <summary>
        /// 游客登录的密钥，用于验证游客身份。
        /// 在 Protocol Buffers 序列化中，该字段编号为 1，并且是必需字段。
        /// </summary>
        [ProtoMember(1, IsRequired = true)]
        public string guestKey;

        /// <summary>
        /// 游客登录的渠道编号，用于标识游客是通过哪个渠道发起的登录请求。
        /// 在 Protocol Buffers 序列化中，该字段编号为 2，并且是必需字段。
        /// </summary>
        [ProtoMember(2, IsRequired = true)]
        public int channal; // 注：此处可能拼写错误，正确应为 "channel"
    }
}