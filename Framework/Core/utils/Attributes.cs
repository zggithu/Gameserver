using System;

namespace Framework.Core.Utils
{
    /// <summary>
    /// ���������ڱ��һ����Ϊ�������ࡣ
    /// ��������ͨ��������ҵ���߼����������󲢷�����Ӧ��
    /// ������ֻ��Ӧ�����࣬��������ͬһ�����϶��ʹ�ã��ҿ��Ա��̳С�
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class Controller : Attribute
    {
    }

    /// <summary>
    /// ���������ڱ��һ�����������ڽ��ض�������ӳ�䵽�÷�����
    /// �����յ���������������ʱ������ø÷������д���
    /// ������ֻ��Ӧ���ڷ�������������ͬһ�������϶��ʹ�ã��ҿ��Ա��̳С�
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RequestMapping : Attribute
    {
    }

    /// <summary>
    /// ���������ڱ����Ϣ�࣬Ϊ��Ϣ���ṩģ��ź��������Ϣ��
    /// ģ��ź�����ſ�������Ϣ�ķ����ʶ�𣬷�����Ϣ�Ĵ���ͷַ���
    /// ������ֻ��Ӧ�����࣬��������ͬһ�����϶��ʹ�ã��ҿ��Ա��̳С�
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class MessageMeta : Attribute
    {
        // ��Ϣ������ģ���
        public short module;
        // ��Ϣ�������
        public short cmd;

        /// <summary>
        /// ���캯�������ڳ�ʼ��ģ��ź�����š�
        /// </summary>
        /// <param name="module">ģ���</param>
        /// <param name="cmd">�����</param>
        public MessageMeta(short module, short cmd)
        {
            this.module = module;
            this.cmd = cmd;
        }
    }

    /// <summary>
    /// ���������ڱ��һ����Ϊ HTTP �������ࡣ
    /// HTTP ��������ר�Ŵ��� HTTP ���󣬽��տͻ��˵� HTTP ���󲢷�����Ӧ����Ӧ��
    /// ������ֻ��Ӧ�����࣬��������ͬһ�����϶��ʹ�ã��ҿ��Ա��̳С�
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class HttpController : Attribute
    {
    }

    /// <summary>
    /// ���������ڱ��һ�����������ض��� URI ӳ�䵽�÷�����
    /// �����յ�ƥ��� URI �� HTTP ����ʱ������ø÷������д���
    /// ������ֻ��Ӧ���ڷ�������������ͬһ�������϶��ʹ�ã��ҿ��Ա��̳С�
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HttpRequestMapping : Attribute
    {
        // ӳ��� URI
        public string uri;

        /// <summary>
        /// ���캯�������ڳ�ʼ��ӳ��� URI��
        /// </summary>
        /// <param name="uri">Ҫӳ��� URI</param>
        public HttpRequestMapping(string uri)
        {
            this.uri = uri;
        }
    }

    /// <summary>
    /// ���������ڱ���߼��������࣬Ϊ�߼���������ָ���������͡�
    /// �������Ϳ��������ֲ�ͬ���߼������������ڶ��߼����������й���͵��ȡ�
    /// ������ֻ��Ӧ�����࣬��������ͬһ�����϶��ʹ�ã��ҿ��Ա��̳С�
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class LogicServerMeta : Attribute
    {
        // �߼��������ķ�������
        public int stype;

        /// <summary>
        /// ���캯�������ڳ�ʼ���������͡�
        /// </summary>
        /// <param name="stype">��������</param>
        public LogicServerMeta(int stype)
        {
            this.stype = stype;
        }
    }

    /// <summary>
    /// ���������ڱ��һ��������Ϊ��Ϣ������ָ��ģ��ź�����š�
    /// �÷��������ڴ����ض�ģ�������ŵ���Ϣ��
    /// ������ֻ��Ӧ���ڷ�������������ͬһ�������϶��ʹ�ã��ҿ��Ա��̳С�
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class LogicMessageProc : Attribute
    {
        // ��Ϣ������ģ���
        public short module;
        // ��Ϣ�������
        public short cmd;

        /// <summary>
        /// ���캯�������ڳ�ʼ��ģ��ź�����š�
        /// </summary>
        /// <param name="module">ģ���</param>
        /// <param name="cmd">�����</param>
        public LogicMessageProc(short module, short cmd)
        {
            this.module = module;
            this.cmd = cmd;
        }
    }
}