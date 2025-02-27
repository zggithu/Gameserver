using Framework.Core.Serializer;
using Framework.Core.Utils;
using ProtoBuf;


namespace Game.Datas.Messages
{

    [ProtoContract]
    [MessageMeta((short)Module.PLAYER, (short)Cmd.eRecvLoginBonuesReq)]
    public class ReqRecvLoginBonues : Message
    {
        [ProtoMember(1)]
        public int type;
    }


}
