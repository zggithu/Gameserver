using Framework.Core.Serializer;
using Framework.Core.Utils;
using ProtoBuf;

namespace Game.Datas.Messages
{
    [ProtoContract]
    [MessageMeta((short)Module.AUTH, (short)Cmd.eUserLoginRes)]
    public class ResUserLogin : Message
    {

        [ProtoMember(1, IsRequired = true)]
        public int status;

        [ProtoMember(2)]
        public AccountInfo uinfo = null;
    }
}
