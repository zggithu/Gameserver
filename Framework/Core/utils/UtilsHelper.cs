namespace Framework.Core.Utils
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// ���߰����࣬�ṩһϵ��ͨ�õĹ��߷���������Ŀ¼�����ֽڲ��������ܡ�ʱ�������ȹ��ܡ�
    /// </summary>
    public class UtilsHelper
    {
        /// <summary>
        /// ��ȡ��ǰ�����ִ��Ŀ¼��
        /// ���ݲ�ͬ�� .NET �汾��ѡ����ʵķ�ʽ��ȡĿ¼��
        /// </summary>
        public static string ProcessDirectory
        {
            get
            {
#if NETSTANDARD2_0 || NETCOREAPP3_1_OR_GREATER || NET5_0_OR_GREATER
                // �� .NET Standard 2.0 �����ϡ�.NET Core 3.1 �����ϡ�.NET 5.0 �����ϰ汾�У�ʹ�� AppContext.BaseDirectory
                return AppContext.BaseDirectory;
#else
                // �������汾�У�ʹ�� AppDomain.CurrentDomain.BaseDirectory
                return AppDomain.CurrentDomain.BaseDirectory;
#endif
            }
        }

        /// <summary>
        /// ��һ�� short ���͵�ֵ��С���ֽ���д���ֽ������ָ��λ�á�
        /// </summary>
        /// <param name="data">Ҫд����ֽ����顣</param>
        /// <param name="offset">д�����ʼƫ������</param>
        /// <param name="value">Ҫд��� short ���͵�ֵ��</param>
        public static void WriteShortLE(byte[] data, int offset, short value)
        {
            // С���ֽ��򣺵�λ�ֽ���ǰ
            data[offset + 0] = (byte)((value & 0x00ff));
            data[offset + 1] = (byte)((value & 0xff00) >> 8);
        }

        /// <summary>
        /// ��һ�� uint ���͵�ֵ��С���ֽ���д���ֽ������ָ��λ�á�
        /// </summary>
        /// <param name="data">Ҫд����ֽ����顣</param>
        /// <param name="offset">д�����ʼƫ������</param>
        /// <param name="value">Ҫд��� uint ���͵�ֵ��</param>
        public static void WriteUintLE(byte[] data, int offset, uint value)
        {
            // С���ֽ��򣺵�λ�ֽ���ǰ
            data[offset + 0] = (byte)((value & 0x000000ff));
            data[offset + 1] = (byte)((value & 0x0000ff00) >> 8);
            data[offset + 2] = (byte)((value & 0x00ff0000) >> 16);
            data[offset + 3] = (byte)((value & 0xff000000) >> 24);
        }

        /// <summary>
        /// ��һ���ֽ����鸴�Ƶ���һ���ֽ������ָ��λ�á�
        /// </summary>
        /// <param name="dst">Ŀ���ֽ����顣</param>
        /// <param name="offset">Ŀ���������ʼƫ������</param>
        /// <param name="src">Դ�ֽ����顣</param>
        public static void WriteBytes(byte[] dst, int offset, byte[] src)
        {
            // ʹ�� Array.Copy ���������ֽ�����ĸ���
            Array.Copy(src, 0, dst, offset, src.Length);
        }

        /// <summary>
        /// ���ֽ������ָ��λ����С���ֽ����ȡһ�� short ���͵�ֵ��
        /// </summary>
        /// <param name="data">Ҫ��ȡ���ֽ����顣</param>
        /// <param name="offset">��ȡ����ʼƫ������</param>
        /// <returns>��ȡ�� short ���͵�ֵ��</returns>
        public static short ReadShortLE(byte[] data, int offset)
        {
            // С���ֽ��򣺵�λ�ֽ���ǰ
            short value = (short)((data[offset + 1] << 8) | (data[offset + 0]));
            return value;
        }

        /// <summary>
        /// ���ֽ������ָ��λ����С���ֽ����ȡһ�� uint ���͵�ֵ��
        /// </summary>
        /// <param name="data">Ҫ��ȡ���ֽ����顣</param>
        /// <param name="offset">��ȡ����ʼƫ������</param>
        /// <returns>��ȡ�� uint ���͵�ֵ��</returns>
        public static uint ReadUintLE(byte[] data, int offset)
        {
            // С���ֽ��򣺵�λ�ֽ���ǰ
            uint value = (uint)((data[offset + 3] << 24) | (data[offset + 2] << 16) | (data[offset + 1] << 8) | (data[offset + 0]));
            return value;
        }

        /// <summary>
        /// ��������ַ������� MD5 ���ܣ������ؼ��ܺ��ʮ�������ַ�����
        /// </summary>
        /// <param name="str">Ҫ���ܵ��ַ�����</param>
        /// <returns>���ܺ��ʮ�������ַ�����</returns>
        public static string Md5(string str)
        {
            string cl = str;
            StringBuilder md5_builder = new StringBuilder();
            // ʵ����һ�� MD5 ����
            MD5 md5 = MD5.Create();
            // �������ַ���ת��Ϊ UTF-8 ������ֽ����飬������ MD5 ����
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(cl));
            // �������ܺ���ֽ����飬��ÿ���ֽ�ת��Ϊʮ�������ַ�����׷�ӵ� StringBuilder ��
            for (int i = 0; i < s.Length; i++)
            {
                // ʹ�� "X2" ��ʽ���ֽ�ת��Ϊ��λʮ�������ַ���
                md5_builder.Append(s[i].ToString("X2"));
            }
            return md5_builder.ToString();
        }

        /// <summary>
        /// ���ص�ǰ�� Unix ʱ���������Ϊ��λ����
        /// </summary>
        /// <returns>��ǰ�� Unix ʱ�����</returns>
        public static long Timestamp()
        {
            // ��ȡ��ǰ UTC ʱ��� Unix ʱ���
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return timestamp;
        }

        /// <summary>
        /// ���ص������� Unix ʱ�����
        /// </summary>
        /// <returns>�������� Unix ʱ�����</returns>
        public static long TimestampToday()
        {
            // ���� Unix ʱ�������ʼʱ�䣨1970 �� 1 �� 1 �� 0 ʱ 0 �� 0 �� UTC��
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            // ��ȡ���������
            DateTime today = DateTime.Today;
            // ��������� Unix ʱ�����ʼʱ���ʱ���
            TimeSpan offset = today - epoch;
            // ����ʱ����������
            return (long)offset.TotalSeconds;
        }

        /// <summary>
        /// ������������ Unix ʱ�����
        /// </summary>
        /// <returns>�������� Unix ʱ�����</returns>
        public static long TimestampYesterday()
        {
            // ��ȡ�������� Unix ʱ���
            long time = TimestampToday();
            // ��ȥһ���������24 * 60 * 60���õ��������� Unix ʱ���
            return (time - 24 * 60 * 60);
        }

        /// <summary>
        /// �� Unix ʱ���ת��Ϊ����ʱ���� DateTime ����
        /// </summary>
        /// <param name="timestamp">Ҫת���� Unix ʱ�����</param>
        /// <returns>����ʱ���� DateTime ����</returns>
        public static DateTime TimestampToLocalDateTime(long timestamp)
        {
            // �� Unix ʱ���ת��Ϊ DateTimeOffset ����
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(timestamp);
            // ����ת��Ϊ����ʱ���� DateTime ����
            return dateTimeOffset.LocalDateTime;
        }
    }
}