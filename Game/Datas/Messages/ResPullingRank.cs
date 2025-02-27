using Framework.Core.Serializer;
using Framework.Core.Utils;
using ProtoBuf;

namespace Game.Datas.Messages
{

    [ProtoContract]
    public class RankItem
    {
        [ProtoMember(1, IsRequired = true)]
        public string unick;

        [ProtoMember(2, IsRequired = true)]
        public int value;


        [ProtoMember(3)]
        public int uface;

        // 其它的，你可以自己再添加，比如需要开始时间与结束时间
    }


    [ProtoContract]
    [MessageMeta((short)Module.PLAYER, (short)Cmd.ePullingRankRes)]
    public class ResPullingRank : Message
    {
        [ProtoMember(1, IsRequired = true)]
        public int status;

        [ProtoMember(2, IsRequired = true)]
        public int selfIndex; // 自己的排名，-1为未上榜

        [ProtoMember(3)]
        public RankItem[] ranks = null;


    }
}
