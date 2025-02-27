// 引入 ProtoBuf 命名空间，该命名空间提供了 Protocol Buffers 序列化相关的功能
using ProtoBuf;

namespace Game.Datas.Messages
{
    /// <summary>
    /// 表示游戏中账户信息的类，使用 Protocol Buffers 进行序列化和反序列化。
    /// 该类包含了玩家的昵称、头像编号、VIP 等级以及是否为游客账号的标识。
    /// </summary>
    [ProtoContract]
    public class AccountInfo
    {
        /// <summary>
        /// 玩家的昵称。
        /// 在 Protocol Buffers 序列化中，该字段的编号为 1，并且是必需字段。
        /// </summary>
        [ProtoMember(1, IsRequired = true)]
        public string unick;

        /// <summary>
        /// 玩家的头像编号。
        /// 在 Protocol Buffers 序列化中，该字段的编号为 2，并且是必需字段。
        /// </summary>
        [ProtoMember(2, IsRequired = true)]
        public int uface;

        /// <summary>
        /// 玩家的 VIP 等级。
        /// 在 Protocol Buffers 序列化中，该字段的编号为 3，并且是必需字段。
        /// </summary>
        [ProtoMember(3, IsRequired = true)]
        public int uvip;

        /// <summary>
        /// 标识玩家是否为游客账号，通常 0 表示非游客，1 表示游客。
        /// 在 Protocol Buffers 序列化中，该字段的编号为 4，并且是必需字段。
        /// </summary>
        [ProtoMember(4, IsRequired = true)]
        public int isGuest;
    }
}