using Framework.Core.Serializer;
using Framework.Core.Utils;
using ProtoBuf;


namespace Game.Datas.Messages
{

    [ProtoContract]
    [MessageMeta((short)Module.PLAYER, (short)Cmd.eRecvBonuesReq)]
    public class ReqRecvBonues : Message
    {
        [ProtoMember(1)]
        public long bonuesId;
    }


}
