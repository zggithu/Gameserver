// ���� Framework ��ܵ����л������ռ䣬�ṩ���л���صĹ��ܺ͹�����
using Framework.Core.Serializer;
// ���� Framework ��ܵĹ��������ռ䣬���ܰ���һЩͨ�õĹ��߷�����ʵ����
using Framework.Core.Utils;
// ���� ProtoBuf �����ռ䣬����֧�� Protocol Buffers ���л�Э��
using ProtoBuf;

namespace Game.Datas.Messages
{
    /// <summary>
    /// ��ʾ�ο͵�¼�������Ϣ�ࡣ
    /// ����ʹ�� Protocol Buffers �������л�����ͨ����ϢԪ����ָ������ģ������
    /// �����ο͵�¼����Ĺؼ���Ϣ����������Ϸϵͳ�з����ο͵�¼����
    /// </summary>
    [ProtoContract]
    /// <summary>
    /// ��Ǹ���Ϣ������ģ��Ϊ��֤ģ�飨Module.AUTH������������Ϊ�ο͵�¼����Cmd.eGuestLoginReq����
    /// ����������Ϣ��ϵͳ�н���׼ȷ��·�ɺʹ���
    /// </summary>
    [MessageMeta((short)Module.AUTH, (short)Cmd.eGuestLoginReq)]
    public class ReqGuestLogin : Message
    {
        /// <summary>
        /// �ο͵�¼����Կ��������֤�ο���ݡ�
        /// �� Protocol Buffers ���л��У����ֶα��Ϊ 1�������Ǳ����ֶΡ�
        /// </summary>
        [ProtoMember(1, IsRequired = true)]
        public string guestKey;

        /// <summary>
        /// �ο͵�¼��������ţ����ڱ�ʶ�ο���ͨ���ĸ���������ĵ�¼����
        /// �� Protocol Buffers ���л��У����ֶα��Ϊ 2�������Ǳ����ֶΡ�
        /// </summary>
        [ProtoMember(2, IsRequired = true)]
        public int channal; // ע���˴�����ƴд������ȷӦΪ "channel"
    }
}