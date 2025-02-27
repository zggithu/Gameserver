using Framework.Core.Serializer;
using Framework.Core.Utils;
using ProtoBuf;


namespace Game.Datas.Messages
{
    [ProtoContract]
    [MessageMeta((short)Module.PLAYER, (short)Cmd.eSelectPlayerReq)]
    public class ReqSelectPlayer: Message
    {
        [ProtoMember(1, IsRequired = true)]
        public int job;// 玩家的职业，具体的根据游戏需求来制定; -1

        [ProtoMember(2)]
        public string uname; // 玩家在游戏中的名字

        [ProtoMember(3)]
        public int usex; // 玩家在游戏中的性别;

        [ProtoMember(4)]
        public int charactorId; // 玩家的角色Id;

        [ProtoMember(5)]
        public string ptxt = string.Empty; // 设置默认值
        // ... 注意:我们这里是通用模板，如果游戏设定中没有，可以自行修改改;
    }
}
