// 引入 Framework 框架中用于序列化的命名空间，提供序列化相关的功能和工具
using Framework.Core.Serializer;
// 引入 Framework 框架中的工具命名空间，可能包含一些通用的工具类和方法
using Framework.Core.Utils;
// 引入 ProtoBuf 命名空间，用于支持 Protocol Buffers 序列化
using ProtoBuf;

namespace Game.Datas.Messages
{
    /// <summary>
    /// 表示请求交换产品的消息类。
    /// 该类使用 Protocol Buffers 进行序列化，并且带有消息元数据，用于指定消息所属模块和命令。
    /// </summary>
    [ProtoContract]
    // 指定消息所属的模块和具体的命令，方便消息的路由和处理
    [MessageMeta((short)Module.PLAYER, (short)Cmd.eExchangeProductReq)]
    public class ReqExchangeProduct : Message
    {
        /// <summary>
        /// 要交换的产品的 ID。
        /// 在 Protocol Buffers 序列化中，该字段编号为 1，并且是必需字段。
        /// </summary>
        [ProtoMember(1, IsRequired = true)]
        public int productId; // 产品id;
    }
}