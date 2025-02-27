using Framework.Core.Serializer;
using Framework.Core.Utils;
using ProtoBuf;


namespace Game.Datas.Messages
{
    [ProtoContract]
    [MessageMeta((short)Module.PLAYER, (short)Cmd.ePullingPlayerDataRes)]
    public class ResPullingPlayerData : Message
    {
        [ProtoMember(1, IsRequired = true)]
        public int status = 0;


        // ... PlayerInfo后面在定义;
        [ProtoMember(2)]
        public PlayerInfo pInfo = null;

    }
}