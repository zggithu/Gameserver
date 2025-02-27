using DotNetty.Buffers;
using DotNetty.Codecs.Http.WebSockets;
using DotNetty.Transport.Channels;
using System.Net;

namespace Framework.Core.Net
{
    /// <summary>
    /// IdSession 类用于表示一个客户端会话，存储了与客户端连接相关的信息，
    /// 并提供了发送数据和获取客户端地址信息的方法。
    /// </summary>
    public class IdSession
    {
        // 客户端通道，用于与客户端进行通信
        public IChannel client = null;

        // 线程池分发器的索引，每个 IdSession 都对应唯一的编号
        // 可用于将该会话分配到特定的线程池进行处理
        public long distributeKey = 0;

        // 玩家对应的游戏 ID 号
        public long playerId = 0;

        // 玩家对应的账号 ID
        public long accountId = 0;

        // 玩家 ID 与职业所对应的 key
        public long accountIdAndJob = 0;

        // 当前的 session 是 WebSocket 还是 TcpSocket
        public bool isWebSocket = false;

        /// <summary>
        /// 获取客户端的 IP 地址。
        /// </summary>
        /// <returns>客户端的 IP 地址字符串。</returns>
        public string GetIp()
        {
            // 获取客户端远程地址的 IP 部分，并去除 IPv6 前缀
            return ((IPEndPoint)this.client.RemoteAddress).Address.ToString().Substring(7);
        }

        /// <summary>
        /// 获取客户端的远程地址，包括 IP 地址和端口号。
        /// </summary>
        /// <returns>客户端的远程地址字符串。</returns>
        public string GetRemoteAddress()
        {
            // 返回客户端的远程地址字符串
            return (this.client.RemoteAddress.ToString());
        }

        /// <summary>
        /// 向客户端发送数据。
        /// 根据 isWebSocket 标志决定是使用 WebSocket 还是 TcpSocket 发送数据。
        /// </summary>
        /// <param name="data">要发送的字节数组数据。</param>
        public void Send(byte[] data)
        {
            // 创建一个新的字节缓冲区，并将数据写入缓冲区
            IByteBuffer buffer = Unpooled.Buffer();
            buffer.WriteBytes(data);

            // 如果当前会话是 WebSocket 连接
            if (this.isWebSocket)
            {
                // 创建一个二进制 WebSocket 帧，包含要发送的数据
                BinaryWebSocketFrame f = new BinaryWebSocketFrame(buffer);
                // 异步地将二进制 WebSocket 帧写入并刷新到客户端通道
                this.client.WriteAndFlushAsync(f);
                /*
                 * 以下代码注释部分展示了另一种在 eventLoop 线程中执行写入操作的方式
                 * 可以根据具体需求决定是否使用
                 * this.client.EventLoop.Execute(()=> {
                 *     this.client.WriteAsync(f);
                 * });
                 */
            }
            else
            {
                // 如果是 TcpSocket 连接，直接将字节缓冲区写入并刷新到客户端通道
                this.client.WriteAndFlushAsync(buffer);
            }
        }
    }
}