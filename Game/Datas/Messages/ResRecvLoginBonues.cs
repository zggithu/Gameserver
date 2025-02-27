using Framework.Core.Serializer;
using Framework.Core.Utils;
using ProtoBuf;


namespace Game.Datas.Messages
{

    [ProtoContract]
    [MessageMeta((short)Module.PLAYER, (short)Cmd.eRecvLoginBonuesRes)]
    public class ResRecvLoginBonues : Message
    {
        [ProtoMember(1, IsRequired = true)]
        public int status;

       
        [ProtoMember(2, IsRequired = true)]
        public int num; 

    }


}

