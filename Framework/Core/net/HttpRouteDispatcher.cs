using DotNetty.Buffers;
using DotNetty.Codecs.Http;
using DotNetty.Common;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using Framework.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Core.Net
{
    class HttpRouteDispatcher
    {
        // 线程本地缓存，用于存储日期信息
        static readonly ThreadLocalCache Cache = new ThreadLocalCache();

        // 线程本地缓存类，继承自 FastThreadLocal，用于获取初始日期字符串
        sealed class ThreadLocalCache : FastThreadLocal<AsciiString>
        {
            protected override AsciiString GetInitialValue()
            {
                // 获取当前的 UTC 时间
                DateTime dateTime = DateTime.UtcNow;
                // 格式化日期并缓存为 AsciiString
                return AsciiString.Cached($"{dateTime.DayOfWeek}, {dateTime:dd MMM yyyy HH:mm:ss z}");
            }
        }

        // NLog 日志记录器，用于记录日志
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        // HttpRouteDispatcher 的单例实例
        public static HttpRouteDispatcher Instance = new HttpRouteDispatcher();

        // 存储 HTTP 路由和对应的 CmdExecutor 的字典，键为 URI，值为 CmdExecutor
        private static Dictionary<string, CmdExecutor> HTTP_ROUTE_CMD_HANDLERS = new();

        // 定义 HTTP 响应头相关的常量
        // 头
        static readonly AsciiString ContentTypeEntity = HttpHeaderNames.ContentType;
        static readonly AsciiString ServerEntity = HttpHeaderNames.Server;
        static readonly AsciiString DateEntity = HttpHeaderNames.Date;
        static readonly AsciiString ContentLengthEntity = HttpHeaderNames.ContentLength;
        static readonly AsciiString TypePlain = AsciiString.Cached("text/plain");
        static readonly AsciiString TypeJson = AsciiString.Cached("application/json");
        // end

        // 定义 HTTP 响应头的值相关的常量和变量
        // value
        volatile ICharSequence date = Cache.Value;
        static readonly AsciiString ServerName = AsciiString.Cached("Bycw");
        // end

        // 初始化方法，用于扫描带有 HttpController 特性的类，并注册路由
        public void Init()
        {
            // 获取所有带有 HttpController 特性的类
            IEnumerable<Type> controllers = TypeScanner.ListTypesWithAttribute(typeof(HttpController));
            foreach (Type controller in controllers)
            {
                try
                {
                    // 创建控制器类的实例
                    object handler = Activator.CreateInstance(controller);
                    // 获取控制器类的所有方法
                    MethodInfo[] methods = controller.GetMethods();

                    foreach (MethodInfo method in methods)
                    {
                        // 获取方法上的 HttpRequestMapping 特性
                        HttpRequestMapping mapperAttribute = method.GetCustomAttribute<HttpRequestMapping>();
                        if (mapperAttribute == null)
                        {
                            // 如果方法没有该特性，则跳过
                            continue;
                        }

                        string key = mapperAttribute.uri;
                        // 检查该 URI 是否已经注册过处理器
                        HTTP_ROUTE_CMD_HANDLERS.TryGetValue(key, out CmdExecutor cmdExecutor);
                        if (cmdExecutor != null)
                        {
                            // 如果已经注册过，则记录警告日志并返回
                            logger.Warn($"Http Route[{key}] 重复注册处理器");
                            return;
                        }

                        // 创建 CmdExecutor 实例
                        cmdExecutor = CmdExecutor.Create(method, method.GetParameters().Select(parameterInfo => parameterInfo.ParameterType).ToArray(), handler);
                        // 将 URI 和对应的 CmdExecutor 存入字典
                        HTTP_ROUTE_CMD_HANDLERS.Add(key, cmdExecutor);
                    }
                }
                catch (Exception e)
                {
                    // 捕获异常并记录错误日志
                    logger.Error(e.Message);
                }
            }
        }

        // 将 HTTP 请求转换为方法参数的方法
        private object[] ConvertToMethodParams(IHttpRequest request, Type[] methodParams)
        {
            // 创建一个与方法参数数量相同的对象数组
            object[] result = new object[methodParams == null ? 0 : methodParams.Length];

            for (int i = 0; i < result.Length; i++)
            {
                Type param = methodParams[i];
                if (param.IsAssignableTo(typeof(IHttpRequest)))
                {
                    // 如果参数类型是 IHttpRequest，则将请求对象赋值给该参数
                    result[i] = request;
                }
            }

            return result;
        }

        // 写入 HTTP 响应的方法
        void WriteResponse(IChannelHandlerContext ctx, IByteBuffer buf, ICharSequence contentType, ICharSequence contentLength)
        {
            // 构建 HTTP 响应对象
            var response = new DefaultFullHttpResponse(HttpVersion.Http11, HttpResponseStatus.OK, buf, false);
            HttpHeaders headers = response.Headers;
            // 设置响应头的内容类型
            headers.Set(ContentTypeEntity, contentType);
            // 设置响应头的服务器名称
            headers.Set(ServerEntity, ServerName);
            // 设置响应头的日期
            headers.Set(DateEntity, this.date);
            // 设置响应头的内容长度
            headers.Set(ContentLengthEntity, contentLength);

            // 异步写入响应
            ctx.WriteAsync(response);
        }

        // 处理 HTTP 请求的方法
        public void OnHttpRequrestProcess(IChannelHandlerContext ctx, IHttpRequest request)
        {
            // 获取请求的 URI
            string uri = request.Uri;

            // 从字典中查找该 URI 对应的 CmdExecutor
            HTTP_ROUTE_CMD_HANDLERS.TryGetValue(uri, out CmdExecutor cmdExecutor);
            if (cmdExecutor == null)
            {
                // 如果未找到对应的处理器，则记录警告日志，返回 404 响应并关闭连接
                logger.Warn($"Http uri executor missed, {uri}");

                var response = new DefaultFullHttpResponse(HttpVersion.Http11, HttpResponseStatus.NotFound, Unpooled.Empty, false);
                ctx.WriteAndFlushAsync(response);
                ctx.CloseAsync();

                return;
            }

            // 目前这里暂时直接调用，不走我们的线程池Mask
            try
            {
                // 将请求转换为方法参数
                object[] @params = ConvertToMethodParams(request, cmdExecutor.@params);
                // 调用处理方法并获取响应结果
                string response = (string)(cmdExecutor.method.Invoke(cmdExecutor.handler, @params));
                if (response != null)
                {
                    // 将响应结果转换为字节数组
                    byte[] jsonBody = Encoding.UTF8.GetBytes(response);
                    // 写入响应
                    this.WriteResponse(ctx, Unpooled.WrappedBuffer(jsonBody), TypeJson, AsciiString.Cached($"{jsonBody.Length}"));
                }
            }
            catch (Exception e)
            {
                // 捕获异常并记录警告日志
                logger.Warn("message task execute failed" + e.Message);
            }
            // end
        }
    }
}