using Framework.Core.Serializer;
using Framework.Core.Utils;
using ProtoBuf;

namespace Game.Datas.Messages {

    [ProtoContract]
    [MessageMeta((short)Module.AUTH, (short)Cmd.eRegisterUserReq)]
    public class ReqRegisterUser : Message {
        [ProtoMember(1, IsRequired = true)]
        public string uname;

        [ProtoMember(2, IsRequired = true)]
        public string upwd;

        [ProtoMember(3, IsRequired = true)]
        public int channal;
    }
}
