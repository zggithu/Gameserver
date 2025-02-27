using Framework.Core.Serializer;
using Framework.Core.Utils;
using ProtoBuf;
using System.Collections.Generic;

namespace Game.Datas.Messages
{

    [ProtoContract]
    public class GoodsItem
    {
        [ProtoMember(1, IsRequired = true)]
        public int typeId;

        [ProtoMember(2, IsRequired = true)]
        public int num;

        [ProtoMember(3)]
        public byte[] strengData = null;

    }

    [ProtoContract]
    public class DicGoodsItem   
    {
        [ProtoMember(1)]
        public int mainType;

        [ProtoMember(2)]
        public List<GoodsItem> Value;
    }

    [ProtoContract]
    [MessageMeta((short)Module.PLAYER, (short)Cmd.ePullingPackDataRes)]
    public class ResPullingPackData : Message
    {
        [ProtoMember(1, IsRequired = true)]
        public int status;


        [ProtoMember(2)]
        public List<DicGoodsItem> packGoods = null;
    }
}
