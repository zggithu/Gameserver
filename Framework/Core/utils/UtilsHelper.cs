namespace Framework.Core.Utils
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// 工具帮助类，提供一系列通用的工具方法，包括目录处理、字节操作、加密、时间戳处理等功能。
    /// </summary>
    public class UtilsHelper
    {
        /// <summary>
        /// 获取当前程序的执行目录。
        /// 根据不同的 .NET 版本，选择合适的方式获取目录。
        /// </summary>
        public static string ProcessDirectory
        {
            get
            {
#if NETSTANDARD2_0 || NETCOREAPP3_1_OR_GREATER || NET5_0_OR_GREATER
                // 在 .NET Standard 2.0 及以上、.NET Core 3.1 及以上、.NET 5.0 及以上版本中，使用 AppContext.BaseDirectory
                return AppContext.BaseDirectory;
#else
                // 在其他版本中，使用 AppDomain.CurrentDomain.BaseDirectory
                return AppDomain.CurrentDomain.BaseDirectory;
#endif
            }
        }

        /// <summary>
        /// 将一个 short 类型的值以小端字节序写入字节数组的指定位置。
        /// </summary>
        /// <param name="data">要写入的字节数组。</param>
        /// <param name="offset">写入的起始偏移量。</param>
        /// <param name="value">要写入的 short 类型的值。</param>
        public static void WriteShortLE(byte[] data, int offset, short value)
        {
            // 小端字节序：低位字节在前
            data[offset + 0] = (byte)((value & 0x00ff));
            data[offset + 1] = (byte)((value & 0xff00) >> 8);
        }

        /// <summary>
        /// 将一个 uint 类型的值以小端字节序写入字节数组的指定位置。
        /// </summary>
        /// <param name="data">要写入的字节数组。</param>
        /// <param name="offset">写入的起始偏移量。</param>
        /// <param name="value">要写入的 uint 类型的值。</param>
        public static void WriteUintLE(byte[] data, int offset, uint value)
        {
            // 小端字节序：低位字节在前
            data[offset + 0] = (byte)((value & 0x000000ff));
            data[offset + 1] = (byte)((value & 0x0000ff00) >> 8);
            data[offset + 2] = (byte)((value & 0x00ff0000) >> 16);
            data[offset + 3] = (byte)((value & 0xff000000) >> 24);
        }

        /// <summary>
        /// 将一个字节数组复制到另一个字节数组的指定位置。
        /// </summary>
        /// <param name="dst">目标字节数组。</param>
        /// <param name="offset">目标数组的起始偏移量。</param>
        /// <param name="src">源字节数组。</param>
        public static void WriteBytes(byte[] dst, int offset, byte[] src)
        {
            // 使用 Array.Copy 方法进行字节数组的复制
            Array.Copy(src, 0, dst, offset, src.Length);
        }

        /// <summary>
        /// 从字节数组的指定位置以小端字节序读取一个 short 类型的值。
        /// </summary>
        /// <param name="data">要读取的字节数组。</param>
        /// <param name="offset">读取的起始偏移量。</param>
        /// <returns>读取的 short 类型的值。</returns>
        public static short ReadShortLE(byte[] data, int offset)
        {
            // 小端字节序：低位字节在前
            short value = (short)((data[offset + 1] << 8) | (data[offset + 0]));
            return value;
        }

        /// <summary>
        /// 从字节数组的指定位置以小端字节序读取一个 uint 类型的值。
        /// </summary>
        /// <param name="data">要读取的字节数组。</param>
        /// <param name="offset">读取的起始偏移量。</param>
        /// <returns>读取的 uint 类型的值。</returns>
        public static uint ReadUintLE(byte[] data, int offset)
        {
            // 小端字节序：低位字节在前
            uint value = (uint)((data[offset + 3] << 24) | (data[offset + 2] << 16) | (data[offset + 1] << 8) | (data[offset + 0]));
            return value;
        }

        /// <summary>
        /// 对输入的字符串进行 MD5 加密，并返回加密后的十六进制字符串。
        /// </summary>
        /// <param name="str">要加密的字符串。</param>
        /// <returns>加密后的十六进制字符串。</returns>
        public static string Md5(string str)
        {
            string cl = str;
            StringBuilder md5_builder = new StringBuilder();
            // 实例化一个 MD5 对象
            MD5 md5 = MD5.Create();
            // 将输入字符串转换为 UTF-8 编码的字节数组，并进行 MD5 加密
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(cl));
            // 遍历加密后的字节数组，将每个字节转换为十六进制字符串并追加到 StringBuilder 中
            for (int i = 0; i < s.Length; i++)
            {
                // 使用 "X2" 格式将字节转换为两位十六进制字符串
                md5_builder.Append(s[i].ToString("X2"));
            }
            return md5_builder.ToString();
        }

        /// <summary>
        /// 返回当前的 Unix 时间戳（以秒为单位）。
        /// </summary>
        /// <returns>当前的 Unix 时间戳。</returns>
        public static long Timestamp()
        {
            // 获取当前 UTC 时间的 Unix 时间戳
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return timestamp;
        }

        /// <summary>
        /// 返回当天零点的 Unix 时间戳。
        /// </summary>
        /// <returns>当天零点的 Unix 时间戳。</returns>
        public static long TimestampToday()
        {
            // 定义 Unix 时间戳的起始时间（1970 年 1 月 1 日 0 时 0 分 0 秒 UTC）
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            // 获取今天的日期
            DateTime today = DateTime.Today;
            // 计算今天与 Unix 时间戳起始时间的时间差
            TimeSpan offset = today - epoch;
            // 返回时间差的总秒数
            return (long)offset.TotalSeconds;
        }

        /// <summary>
        /// 返回昨天零点的 Unix 时间戳。
        /// </summary>
        /// <returns>昨天零点的 Unix 时间戳。</returns>
        public static long TimestampYesterday()
        {
            // 获取今天零点的 Unix 时间戳
            long time = TimestampToday();
            // 减去一天的秒数（24 * 60 * 60）得到昨天零点的 Unix 时间戳
            return (time - 24 * 60 * 60);
        }

        /// <summary>
        /// 将 Unix 时间戳转换为本地时区的 DateTime 对象。
        /// </summary>
        /// <param name="timestamp">要转换的 Unix 时间戳。</param>
        /// <returns>本地时区的 DateTime 对象。</returns>
        public static DateTime TimestampToLocalDateTime(long timestamp)
        {
            // 将 Unix 时间戳转换为 DateTimeOffset 对象
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(timestamp);
            // 返回转换为本地时区的 DateTime 对象
            return dateTimeOffset.LocalDateTime;
        }
    }
}