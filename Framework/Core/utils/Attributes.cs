using System;

namespace Framework.Core.Utils
{
    /// <summary>
    /// 该特性用于标记一个类为控制器类。
    /// 控制器类通常负责处理业务逻辑，接收请求并返回响应。
    /// 此特性只能应用于类，不允许在同一个类上多次使用，且可以被继承。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class Controller : Attribute
    {
    }

    /// <summary>
    /// 该特性用于标记一个方法，用于将特定的请求映射到该方法。
    /// 当接收到符合条件的请求时，会调用该方法进行处理。
    /// 此特性只能应用于方法，不允许在同一个方法上多次使用，且可以被继承。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RequestMapping : Attribute
    {
    }

    /// <summary>
    /// 该特性用于标记消息类，为消息类提供模块号和命令号信息。
    /// 模块号和命令号可用于消息的分类和识别，方便消息的处理和分发。
    /// 此特性只能应用于类，不允许在同一个类上多次使用，且可以被继承。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class MessageMeta : Attribute
    {
        // 消息所属的模块号
        public short module;
        // 消息的命令号
        public short cmd;

        /// <summary>
        /// 构造函数，用于初始化模块号和命令号。
        /// </summary>
        /// <param name="module">模块号</param>
        /// <param name="cmd">命令号</param>
        public MessageMeta(short module, short cmd)
        {
            this.module = module;
            this.cmd = cmd;
        }
    }

    /// <summary>
    /// 该特性用于标记一个类为 HTTP 控制器类。
    /// HTTP 控制器类专门处理 HTTP 请求，接收客户端的 HTTP 请求并返回相应的响应。
    /// 此特性只能应用于类，不允许在同一个类上多次使用，且可以被继承。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class HttpController : Attribute
    {
    }

    /// <summary>
    /// 该特性用于标记一个方法，将特定的 URI 映射到该方法。
    /// 当接收到匹配该 URI 的 HTTP 请求时，会调用该方法进行处理。
    /// 此特性只能应用于方法，不允许在同一个方法上多次使用，且可以被继承。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HttpRequestMapping : Attribute
    {
        // 映射的 URI
        public string uri;

        /// <summary>
        /// 构造函数，用于初始化映射的 URI。
        /// </summary>
        /// <param name="uri">要映射的 URI</param>
        public HttpRequestMapping(string uri)
        {
            this.uri = uri;
        }
    }

    /// <summary>
    /// 该特性用于标记逻辑服务器类，为逻辑服务器类指定服务类型。
    /// 服务类型可用于区分不同的逻辑服务器，便于对逻辑服务器进行管理和调度。
    /// 此特性只能应用于类，不允许在同一个类上多次使用，且可以被继承。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class LogicServerMeta : Attribute
    {
        // 逻辑服务器的服务类型
        public int stype;

        /// <summary>
        /// 构造函数，用于初始化服务类型。
        /// </summary>
        /// <param name="stype">服务类型</param>
        public LogicServerMeta(int stype)
        {
            this.stype = stype;
        }
    }

    /// <summary>
    /// 该特性用于标记一个方法，为消息处理方法指定模块号和命令号。
    /// 该方法将用于处理特定模块和命令号的消息。
    /// 此特性只能应用于方法，不允许在同一个方法上多次使用，且可以被继承。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class LogicMessageProc : Attribute
    {
        // 消息所属的模块号
        public short module;
        // 消息的命令号
        public short cmd;

        /// <summary>
        /// 构造函数，用于初始化模块号和命令号。
        /// </summary>
        /// <param name="module">模块号</param>
        /// <param name="cmd">命令号</param>
        public LogicMessageProc(short module, short cmd)
        {
            this.module = module;
            this.cmd = cmd;
        }
    }
}