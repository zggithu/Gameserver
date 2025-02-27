// 引入 ProtoBuf 命名空间，该命名空间提供了 Protocol Buffers 序列化所需的类和特性
using ProtoBuf;

namespace Game.Datas.Messages
{
    /// <summary>
    /// 该类用于表示游戏中玩家的详细信息，利用 Protocol Buffers 进行序列化和反序列化操作。
    /// 它包含了玩家的基本属性、资源信息以及一些奖励相关的状态。
    /// </summary>
    [ProtoContract]
    public class PlayerInfo
    {
        /// <summary>
        /// 玩家的昵称，在 Protocol Buffers 序列化中编号为 1，且该字段是必需的。
        /// </summary>
        [ProtoMember(1, IsRequired = true)]
        public string unick;

        /// <summary>
        /// 玩家的生命值，在 Protocol Buffers 序列化中编号为 2，且该字段是必需的。
        /// </summary>
        [ProtoMember(2, IsRequired = true)]
        public int hp;

        /// <summary>
        /// 玩家的经验值，在 Protocol Buffers 序列化中编号为 3，且该字段是必需的。
        /// </summary>
        [ProtoMember(3, IsRequired = true)]
        public int exp;

        /// <summary>
        /// 玩家的魔法值，在 Protocol Buffers 序列化中编号为 4，且该字段是必需的。
        /// </summary>
        [ProtoMember(4, IsRequired = true)]
        public int mp;

        /// <summary>
        /// 玩家拥有的金钱数量，在 Protocol Buffers 序列化中编号为 5，且该字段是必需的。
        /// </summary>
        [ProtoMember(5, IsRequired = true)]
        public int umoney;

        /// <summary>
        /// 玩家拥有的某种代币数量，在 Protocol Buffers 序列化中编号为 6，且该字段是必需的。
        /// </summary>
        [ProtoMember(6, IsRequired = true)]
        public int ucion;

        /// <summary>
        /// 玩家的性别，在 Protocol Buffers 序列化中编号为 7，且该字段是必需的。
        /// </summary>
        [ProtoMember(7, IsRequired = true)]
        public int usex;

        /// <summary>
        /// 标识玩家是否有奖励，0 通常表示没有，非 0 表示有，默认值为 0。
        /// 在 Protocol Buffers 序列化中编号为 8，且该字段是必需的。
        /// </summary>
        [ProtoMember(8, IsRequired = true)]
        public int hasBonues = 0;

        /// <summary>
        /// 表示某种与天数相关的信息，默认值为 0。
        /// 在 Protocol Buffers 序列化中编号为 9，且该字段是必需的。
        /// </summary>
        [ProtoMember(9, IsRequired = true)]
        public int days = 0;

        /// <summary>
        /// 玩家的登录奖励状态，默认值为 0。
        /// 在 Protocol Buffers 序列化中编号为 10，且该字段是必需的。
        /// </summary>
        [ProtoMember(10, IsRequired = true)]
        public int loginBonues = 0;

        // ... 根据自己的游戏来加;
        // end...
    }
}