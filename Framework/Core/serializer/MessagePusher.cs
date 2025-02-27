// 引入自定义的网络相关命名空间，可能包含与网络连接、会话管理等相关的类和方法
using Framework.Core.Net;
// 引入自定义的工具类命名空间，可能包含一些通用的工具方法，如字节序处理、编码转换等
using Framework.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Core.Serializer
{
    /// <summary>
    /// 消息推送器类，负责将消息对象序列化并添加元数据后推送给指定会话。
    /// </summary>
    public class MessagePusher
    {
        /// <summary>
        /// 向指定会话推送消息的静态方法。
        /// </summary>
        /// <param name="s">目标会话对象，代表消息要发送到的客户端会话。</param>
        /// <param name="m">要推送的消息对象，包含具体的业务数据。</param>
        public static void PushMessage(IdSession s, Message m)
        {
            // 使用序列化帮助类的 PBEncoder 方法对消息对象进行 Protobuf 编码，
            // 将消息对象转换为字节数组，以便在网络中传输。
            byte[] msgBody = SerializerHelper.PBEncoder(m);

            // 检查编码后的消息体是否为空，如果为空，说明编码过程可能出现问题，
            // 直接返回，不进行后续的消息发送操作。
            if (msgBody == null)
            {
                return;
            }

            // 定义消息元数据的大小，元数据包含三部分：
            // 2 字节的模块号、2 字节的命令号和 4 字节的预留字段，总共 8 字节。
            int metaSize = 8;

            // 创建一个新的字节数组，用于存储完整的消息，
            // 其长度为元数据长度与编码后的消息体长度之和。
            byte[] body = new byte[metaSize + msgBody.Length];

            // 调用工具类的 WriteShortLE 方法，将消息的模块号以小端字节序写入到字节数组的起始位置（偏移量为 0）。
            // 小端字节序表示低位字节存储在低地址，高位字节存储在高地址。
            UtilsHelper.WriteShortLE(body, 0, m.GetModule());

            // 同样使用 WriteShortLE 方法，将消息的命令号以小端字节序写入到字节数组的偏移量为 2 的位置。
            UtilsHelper.WriteShortLE(body, 2, m.GetCmd());

            // 调用工具类的 WriteUintLE 方法，将预留字段（这里初始值设为 0）以小端字节序写入到字节数组的偏移量为 4 的位置。
            UtilsHelper.WriteUintLE(body, 4, 0);

            // 调用工具类的 WriteBytes 方法，将编码后的消息体写入到字节数组中，
            // 起始偏移位置为元数据的长度，即偏移量为 8 的位置。
            UtilsHelper.WriteBytes(body, metaSize, msgBody);

            // 调用会话对象的 Send 方法，将包含元数据和消息体的完整字节数组发送给客户端。
            // 这里的 Send 方法可能是基于 TCP 或 WebSocket 等网络协议实现的发送逻辑。
            s.Send(body);
        }
    }
}