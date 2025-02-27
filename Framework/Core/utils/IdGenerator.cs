using System;
using System.Threading;

namespace Framework.Core.Utils
{
    /// <summary>
    /// 该类用于生成唯一的 long 类型 ID。
    /// 生成的 ID 采用特定格式，包含服务器 ID、系统秒数和自增长号，以确保 ID 的唯一性。
    /// </summary>
    public class IdGenerator
    {
        // 用于生成自增长号的静态变量，初始值为 0
        private static long i = 0;

        /// <summary>
        /// 生成下一个唯一的 long 类型 ID。
        /// </summary>
        /// <returns>生成的唯一 ID。</returns>
        public static long GetNextId()
        {
            //----------------id格式 -------------------------
            //----------long类型8个字节64个比特位----------------
            // 高16位          	| 中32位          |  低16位
            // serverId        系统秒数          自增长号

            // 定义服务器 ID，这里设置为 1001
            long serverId = 1001;

            // 计算自 1970 年 1 月 1 日 0 时 0 分 0 秒（UTC 时间）起经过的秒数
            long secondsSinceEpoch = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds / 1000;

            // 使用 Interlocked.Increment 方法以线程安全的方式增加自增长号
            long incrementedValue = Interlocked.Increment(ref i);

            // 通过位运算将服务器 ID、系统秒数和自增长号组合成一个唯一的 long 类型 ID
            // 服务器 ID 左移 48 位，占据 ID 的高 16 位
            // 系统秒数与 0xFFFFFFFF 进行按位与操作，确保只取低 32 位，然后左移 16 位，占据 ID 的中 32 位
            // 自增长号与 0xFFFF 进行按位与操作，确保只取低 16 位，占据 ID 的低 16 位
            return (serverId << 48)
                   | ((secondsSinceEpoch & 0xFFFFFFFF) << 16)
                   | (incrementedValue & 0xFFFF);
        }
    }
}