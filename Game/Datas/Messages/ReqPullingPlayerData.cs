using Framework.Core.Serializer;
using Framework.Core.Utils;
using ProtoBuf;


namespace Game.Datas.Messages
{
    [ProtoContract]
    [MessageMeta((short)Module.PLAYER, (short)Cmd.ePullingPlayerDataReq)]
    public class ReqPullingPlayerData : Message
    {
        [ProtoMember(1, IsRequired = true)]
        public int job;// 玩家的职业，具体的根据游戏需求来制定; -1

        // ...
    }
}
