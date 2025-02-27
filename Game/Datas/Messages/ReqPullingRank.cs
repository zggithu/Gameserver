using Framework.Core.Serializer;
using Framework.Core.Utils;
using ProtoBuf;

namespace Game.Datas.Messages
{
    [ProtoContract]
    [MessageMeta((short)Module.PLAYER, (short)Cmd.ePullingRankReq)]
    public class ReqPullingRank : Message
    {
        [ProtoMember(1, IsRequired = true)]
        public int typeId; // 拉取的排行榜类型;
    }
}
