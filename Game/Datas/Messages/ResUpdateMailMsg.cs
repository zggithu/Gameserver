using Framework.Core.Serializer;
using Framework.Core.Utils;
using ProtoBuf;

namespace Game.Datas.Messages
{

    [ProtoContract]
    [MessageMeta((short)Module.PLAYER, (short)Cmd.eUpdategMailMsgRes)]
    public class ResUpdateMailMsg : Message
    {
        [ProtoMember(1, IsRequired = true)]
        public int status;
    }
}
