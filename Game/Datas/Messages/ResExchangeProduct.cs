using Framework.Core.Serializer;
using Framework.Core.Utils;
using ProtoBuf;

namespace Game.Datas.Messages
{


    [ProtoContract]
    [MessageMeta((short)Module.PLAYER, (short)Cmd.eExchangeProductRes)]
    public class ResExchangeProduct : Message
    {
        [ProtoMember(1, IsRequired = true)]
        public int status;
    }
}
