using Framework.Core.Serializer;
using Framework.Core.Utils;
using ProtoBuf;

namespace Game.Datas.Messages
{
    [ProtoContract]
    [MessageMeta((short)Module.PLAYER, (short)Cmd.ePullingBonuesListReq)]
    public class ReqPullingBonuesList : Message
    {
        [ProtoMember(1, IsRequired = true)]
        public int typeId; // 奖励的主类型

    }
}
