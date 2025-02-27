using Framework.Core.Serializer;
using Framework.Core.Utils;
using ProtoBuf;

namespace Game.Datas.Messages
{

    [ProtoContract]
    [MessageMeta((short)Module.AUTH, (short)Cmd.eRegisterUserRes)]
    public class ResRegisterUser : Message
    {
        [ProtoMember(1, IsRequired = true)]
        public int status;   // 成功，失败;

        [ProtoMember(2)]
        public string errorStr; // reason;
    }
}
