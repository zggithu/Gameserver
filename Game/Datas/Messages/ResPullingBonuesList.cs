using Framework.Core.Serializer;
using Framework.Core.Utils;
using ProtoBuf;

namespace Game.Datas.Messages
{

    [ProtoContract]
    public class BonuesItem
    {
        [ProtoMember(1, IsRequired = true)]
        public long bonuesId;
        
        [ProtoMember(2, IsRequired = true)]
        public string bonuesDesic;

        [ProtoMember(3, IsRequired = true)]
        public int status;

        [ProtoMember(4)]
        public int typeId;

        // 其它的，你可以自己再添加，比如需要开始时间与结束时间
    }


    [ProtoContract]
    [MessageMeta((short)Module.PLAYER, (short)Cmd.ePullingBonuesListRes)]
    public class ResPullingBonuesList : Message
    {
        [ProtoMember(1, IsRequired = true)]
        public int status;


        [ProtoMember(2)]
        public BonuesItem[] bonues = null;
    }
}
