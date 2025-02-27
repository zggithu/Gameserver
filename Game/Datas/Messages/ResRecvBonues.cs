using Framework.Core.Serializer;
using Framework.Core.Utils;
using ProtoBuf;


namespace Game.Datas.Messages
{

    [ProtoContract]
    [MessageMeta((short)Module.PLAYER, (short)Cmd.eRecvBonuesRes)]
    public class ResRecvBonues : Message
    {
        [ProtoMember(1, IsRequired = true)]
        public int status;

        [ProtoMember(2)]
        public int typeId;


        [ProtoMember(3)]
        public int b1;

        [ProtoMember(4)]
        public int b2;

        [ProtoMember(5)]
        public int b3;

        [ProtoMember(6)]
        public int b4;

        [ProtoMember(7)]
        public int b5;

    }


}
