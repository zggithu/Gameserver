using Framework.Core.Serializer;
using Framework.Core.Utils;
using ProtoBuf;

namespace Game.Datas.Messages
{
    [ProtoContract]
    [MessageMeta((short)Module.PLAYER, (short)Cmd.ePullingPackDataReq)]
    public class ReqPullingPackData : Message
    {
        [ProtoMember(1, IsRequired = true)]
        public int typeId; // 拉取的背包数据的类型, -1标识拉取所有的背包数据;

        /*
        [ProtoMember(1, IsRequired = true)]
        public int startIndex; // 开始的索引 

        [ProtoMember(2, IsRequired = true)]
        public int num; // -1 表示拉取全部
        */

    }
}