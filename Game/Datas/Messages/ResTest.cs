using Framework.Core.Serializer;
using Framework.Core.Utils;
using ProtoBuf;

namespace Game.Datas.Messages
{
    [ProtoContract]
    [MessageMeta((short)Module.PLAYER, (short)Cmd.eTestGetGoodRes)]
    public class ResTestGetGoods : Message
    {
        [ProtoMember(1, IsRequired = true)]
        public int status; 
    }

    [ProtoContract]
    [MessageMeta((short)Module.PLAYER, (short)Cmd.eTestUpdateGoddsRes)]
    public class ResTestUpdateGoods : Message
    {
        [ProtoMember(1, IsRequired = true)]
        public int status;
    }

    [ProtoContract]
    [MessageMeta((short)Module.SCENE, (short)Cmd.eTestLogicCmdEchoRes)]
    public class ResTestLogicCmdEcho : Message
    {
        [ProtoMember(1, IsRequired = true)]
        public string content;
    }
}