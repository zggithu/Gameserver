namespace Framework.Core.Net
{
    // ���봦�� HTTP �������ص������ռ�
    using DotNetty.Codecs.Http;
    // ���� DotNetty ͨ�ù����������ռ�
    using DotNetty.Common.Utilities;
    // ���� DotNetty ͨ��������ص������ռ�
    using DotNetty.Transport.Channels;
    using System;

    /// <summary>
    /// �ܷ��� HttpServerHandler���̳��� ChannelHandlerAdapter��
    /// ���ڴ��� Netty ͨ�����յ��� HTTP �����ܷ�����ζ�Ÿ��಻�ܱ��̳С�
    /// </summary>
    sealed class HttpServerHandler : ChannelHandlerAdapter
    {
        // ʹ�� NLog ��¼��־����ȡ��ǰ�����־��¼��
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// ��д ChannelRead ��������ͨ�����յ�����Ϣʱ����ô˷�����
        /// </summary>
        /// <param name="ctx">ͨ�����������ģ�����ͨ���������Ϣ�Ͳ���������</param>
        /// <param name="message">���յ�����Ϣ����</param>
        public override void ChannelRead(IChannelHandlerContext ctx, object message)
        {
            // �жϽ��յ�����Ϣ�Ƿ�Ϊ HTTP ����
            if (message is IHttpRequest request)
            {
                try
                {
                    // ���� Process �������� HTTP ����
                    this.Process(ctx, request);
                }
                finally
                {
                    // �ͷ���Ϣ��������ü����������ڴ�й©
                    ReferenceCountUtil.Release(message);
                }
            }
            else
            {
                // �����Ϣ���� HTTP ���󣬽���Ϣ���ݸ���һ��ͨ��������
                ctx.FireChannelRead(message);
            }
        }

        /// <summary>
        /// ���� HTTP ����ķ�����
        /// </summary>
        /// <param name="ctx">ͨ�����������ġ�</param>
        /// <param name="request">HTTP �������</param>
        void Process(IChannelHandlerContext ctx, IHttpRequest request)
        {
            // ���� HttpRouteDispatcher �ĵ���ʵ���� OnHttpRequrestProcess ��������������
            HttpRouteDispatcher.Instance.OnHttpRequrestProcess(ctx, request);
        }

        /// <summary>
        /// ��д ExceptionCaught ��������ͨ����������г����쳣ʱ����ô˷�����
        /// </summary>
        /// <param name="context">ͨ�����������ġ�</param>
        /// <param name="exception">���񵽵��쳣����</param>
        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            // ��¼�쳣��־��ԭ����δ��¼���ɸ���������ӣ�
            logger.Error(exception, "ͨ����������г����쳣");
            // �ر�ͨ�����ͷ������Դ
            context.CloseAsync();
        }

        /// <summary>
        /// ��д ChannelReadComplete ��������ͨ����ȡ���ʱ����ô˷�����
        /// </summary>
        /// <param name="context">ͨ�����������ġ�</param>
        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            // ˢ��ͨ����ȷ�����д����͵����ݶ������ͳ�ȥ
            context.Flush();
        }
    }
}