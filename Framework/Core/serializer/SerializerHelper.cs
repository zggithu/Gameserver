using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Core.Serializer
{
    /// <summary>
    /// 序列化帮助类，提供消息的 Protobuf 序列化和反序列化功能。
    /// </summary>
    public class SerializerHelper
    {
        // 使用 NLog 记录日志，获取当前类的日志记录器
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 使用 Protobuf 对消息对象进行序列化。
        /// </summary>
        /// <param name="msg">要序列化的消息对象。</param>
        /// <returns>序列化后的字节数组，如果序列化过程出错则返回 null。</returns>
        static public byte[] PBEncoder(Message msg)
        {
            // 用于存储序列化后的字节数组
            byte[] body = null;

            try
            {
                // 创建一个内存流，用于临时存储序列化后的数据
                using (var stream = new MemoryStream())
                {
                    // 使用 Protobuf 序列化器将消息对象序列化到内存流中
                    ProtoBuf.Serializer.Serialize(stream, msg);
                    // 将内存流中的数据转换为字节数组
                    body = stream.ToArray();
                }
            }
            catch (IOException e)
            {
                // 若序列化过程中出现输入输出异常，记录错误日志
                logger.Error(e.Message);
            }
            return body;
        }

        /// <summary>
        /// 使用 Protobuf 对字节数组进行反序列化，得到消息对象。
        /// </summary>
        /// <param name="module">消息所属的模块号。</param>
        /// <param name="cmd">消息的命令号。</param>
        /// <param name="body">要反序列化的字节数组。</param>
        /// <param name="offset">字节数组的起始偏移量。</param>
        /// <param name="count">要处理的字节数。</param>
        /// <returns>反序列化后的消息对象，如果反序列化过程出错则返回 null。</returns>
        static public Message PbDecode(short module, short cmd, byte[] body, int offset, int count)
        {
            // 从消息工厂中根据模块号和命令号获取对应的消息类型
            MessageFactory.Instance.GetMessage(module, cmd, out Type msgType);

            // 如果传入的字节数组为空
            if (body == null)
            {
                // 创建该消息类型的实例并返回
                return (Message)Activator.CreateInstance(msgType);
            }

            try
            {
                // 创建一个内存流，从指定的字节数组中读取数据，起始位置为 offset，读取长度为 count
                using (var stream = new MemoryStream(body, offset, count))
                {
                    // 使用 Protobuf 序列化器从内存流中反序列化出消息对象
                    Message _fw = (Message)ProtoBuf.Serializer.Deserialize(msgType, stream);
                    return _fw;
                }
            }
            catch (Exception e)
            {
                // 若反序列化过程中出现异常，记录详细的错误日志，包含模块号、命令号和异常信息
                logger.Error($"读取消息出错,模块号[{module}]，类型[{cmd}],异常:{e.Message}");
            }

            return null;
        }
    }
}