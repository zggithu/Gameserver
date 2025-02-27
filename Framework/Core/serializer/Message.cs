using Framework.Core.Utils;
using System.Reflection;

namespace Framework.Core.Serializer
{
    /// <summary>
    /// ������ Message����Ϊ��Ϣ��Ļ��࣬�ṩ��ȡ��Ϣģ�顢�����Լ�������ϢΨһ���ķ�����
    /// �������÷�����ƴ���Ϣ����Զ������� MessageMeta ����ȡģ���������Ϣ��
    /// �����ǳ����࣬����ֱ��ʵ���������ɾ������Ϣ��̳�ʹ�á�
    /// </summary>
    public abstract class Message
    {
        /// <summary>
        /// ��ȡ��Ϣ������ģ���š�
        /// ͨ�������ȡ��ǰ��Ϣ��� MessageMeta ���ԣ������Դ����򷵻����е�ģ���ţ����򷵻� 0��
        /// </summary>
        /// <returns>��Ϣ��ģ���š�</returns>
        public short GetModule()
        {
            // ��ȡ��ǰ��Ϣ��� MessageMeta ����
            MessageMeta attribute = GetType().GetCustomAttribute<MessageMeta>();
            // �����Դ��ڣ��򷵻����е�ģ����
            if (attribute != null)
            {
                return attribute.module;
            }
            // �����Բ����ڣ�����Ĭ��ֵ 0
            return 0;
        }


        /// <summary>
        /// ��ȡ��Ϣ�������š�
        /// ͨ�������ȡ��ǰ��Ϣ��� MessageMeta ���ԣ������Դ����򷵻����е������ţ����򷵻� 0��
        /// </summary>
        /// <returns>��Ϣ�������š�</returns>
        public short GetCmd()
        {
            // ��ȡ��ǰ��Ϣ��� MessageMeta ����
            MessageMeta attribute = GetType().GetCustomAttribute<MessageMeta>();
            // �����Դ��ڣ��򷵻����е�������
            if (attribute != null)
            {
                return attribute.cmd;
            }
            // �����Բ����ڣ�����Ĭ��ֵ 0
            return 0;
        }

        /// <summary>
        /// ������Ϣ��Ψһ����
        /// �ü�����Ϣ��ģ���ź���������϶��ɣ���ʽΪ "ģ����_������"��
        /// </summary>
        /// <returns>��Ϣ��Ψһ����</returns>
        public string key()
        {
            // ��ģ���ź���������ϳ�Ψһ��
            return GetModule() + "_" + GetCmd();
        }
    }
}