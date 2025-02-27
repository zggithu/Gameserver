using System;

namespace LitJson
{
    /// <summary>
    /// JsonException ���� LitJSON ���ڽ��� JSON ���ݹ����з�������ʱ�׳����쳣�Ļ��ࡣ
    /// ���̳��Բ�ͬ���쳣���ͣ�����ȡ����Ŀ���ܣ�.NET Standard 1.5 �����ϼ̳��� Exception�������̳��� ApplicationException����
    /// �����ṩ�˶�����캯�������ڸ��ݲ�ͬ�Ĵ�����������쳣ʵ����
    /// </summary>
    public class JsonException :
#if NETSTANDARD1_5
        Exception
#else
        ApplicationException
#endif
    {
        /// <summary>
        /// Ĭ�Ϲ��캯��������һ��û�д�����Ϣ�� JsonException ʵ����
        /// ����Ҫ�׳�һ��û���ض����������� JSON �����쳣ʱʹ�á�
        /// </summary>
        public JsonException() : base()
        {
        }

        /// <summary>
        /// �ڲ����캯�������ݽ�����������������Ч���������ƴ��� JsonException ʵ����
        /// </summary>
        /// <param name="token">������������������Ч���������ơ�</param>
        internal JsonException(ParserToken token) :
            base(String.Format(
                    "Invalid token '{0}' in input string", token))
        {
        }

        /// <summary>
        /// �ڲ����캯�������ݽ�����������������Ч�����������Լ��ڲ��쳣���� JsonException ʵ����
        /// ��������������Ϊĳ���ڲ��쳣����������Ч����ʱʹ�á�
        /// </summary>
        /// <param name="token">������������������Ч���������ơ�</param>
        /// <param name="inner_exception">���½���������ڲ��쳣��</param>
        internal JsonException(ParserToken token,
                                Exception inner_exception) :
            base(String.Format(
                    "Invalid token '{0}' in input string", token),
                inner_exception)
        {
        }

        /// <summary>
        /// �ڲ����캯�������ݽ�����������������Ч�ַ����� JsonException ʵ����
        /// </summary>
        /// <param name="c">������������������Ч�ַ��� ASCII ��ֵ��</param>
        internal JsonException(int c) :
            base(String.Format(
                    "Invalid character '{0}' in input string", (char)c))
        {
        }

        /// <summary>
        /// �ڲ����캯�������ݽ�����������������Ч�ַ��Լ��ڲ��쳣���� JsonException ʵ����
        /// ��������������Ϊĳ���ڲ��쳣����������Ч�ַ�ʱʹ�á�
        /// </summary>
        /// <param name="c">������������������Ч�ַ��� ASCII ��ֵ��</param>
        /// <param name="inner_exception">���½���������ڲ��쳣��</param>
        internal JsonException(int c, Exception inner_exception) :
            base(String.Format(
                    "Invalid character '{0}' in input string", (char)c),
                inner_exception)
        {
        }

        /// <summary>
        /// ���캯���������Զ���Ĵ�����Ϣ���� JsonException ʵ����
        /// ����Ҫ�׳�һ�������ض����������� JSON �����쳣ʱʹ�á�
        /// </summary>
        /// <param name="message">�Զ���Ĵ�����Ϣ��</param>
        public JsonException(string message) : base(message)
        {
        }

        /// <summary>
        /// ���캯���������Զ���Ĵ�����Ϣ�Լ��ڲ��쳣���� JsonException ʵ����
        /// ��������������Ϊĳ���ڲ��쳣���´��󣬲�����Ҫ�ṩ�Զ����������ʱʹ�á�
        /// </summary>
        /// <param name="message">�Զ���Ĵ�����Ϣ��</param>
        /// <param name="inner_exception">���½���������ڲ��쳣��</param>
        public JsonException(string message, Exception inner_exception) :
            base(message, inner_exception)
        {
        }
    }
}