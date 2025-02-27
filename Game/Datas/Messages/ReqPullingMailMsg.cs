using Framework.Core.Serializer;
using Framework.Core.Utils;
using ProtoBuf;

namespace Game.Datas.Messages
{
    [ProtoContract]
    [MessageMeta((short)Module.PLAYER, (short)Cmd.ePullingMailMsgReq)]
    public class ReqPullingMailMsg : Message
    {
        [ProtoMember(1, IsRequired = true)]
        public int typeId; // 拉取的任务类型，-1所有任务，默认-1;
    }
}
