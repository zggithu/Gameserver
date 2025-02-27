using Framework.Core.Serializer;
using Framework.Core.Utils;
using ProtoBuf;

namespace Game.Datas.Messages
{
    [ProtoContract]
    [MessageMeta((short)Module.PLAYER, (short)Cmd.eTestGetGoodReq)]
    public class ReqTestGetGoods : Message
    {
        [ProtoMember(1, IsRequired = true)]
        public int typeId; // 拉取的任务类型，-1所有任务，默认-1;

        [ProtoMember(2, IsRequired = true)]
        public int num; // 物品数目
    }


    [ProtoContract]
    [MessageMeta((short)Module.PLAYER, (short)Cmd.eTestUpdateGoodsReq)]
    public class ReqTestUpdateGooods : Message {
        [ProtoMember(1, IsRequired = true)]
        public int typeId; // 拉取的任务类型，-1所有任务，默认-1;

        [ProtoMember(2, IsRequired = true)]
        public int num; // 物品数目, > 0 增加， < 0是减少;

    }

    [ProtoContract]
    [MessageMeta((short)Module.SCENE, (short)Cmd.eTestLogicCmdEchoReq)]
    public class ReqTestLogicCmdEcho : Message
    {
        [ProtoMember(1, IsRequired = true)]
        public string content;
    }
}