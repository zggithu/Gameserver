using System;

namespace LitJson
{
    /// <summary>
    /// JsonException 类是 LitJSON 库在解析 JSON 数据过程中发生错误时抛出的异常的基类。
    /// 它继承自不同的异常类型，具体取决于目标框架（.NET Standard 1.5 及以上继承自 Exception，其他继承自 ApplicationException）。
    /// 该类提供了多个构造函数，用于根据不同的错误情况创建异常实例。
    /// </summary>
    public class JsonException :
#if NETSTANDARD1_5
        Exception
#else
        ApplicationException
#endif
    {
        /// <summary>
        /// 默认构造函数，创建一个没有错误消息的 JsonException 实例。
        /// 当需要抛出一个没有特定错误描述的 JSON 解析异常时使用。
        /// </summary>
        public JsonException() : base()
        {
        }

        /// <summary>
        /// 内部构造函数，根据解析过程中遇到的无效解析器令牌创建 JsonException 实例。
        /// </summary>
        /// <param name="token">解析过程中遇到的无效解析器令牌。</param>
        internal JsonException(ParserToken token) :
            base(String.Format(
                    "Invalid token '{0}' in input string", token))
        {
        }

        /// <summary>
        /// 内部构造函数，根据解析过程中遇到的无效解析器令牌以及内部异常创建 JsonException 实例。
        /// 当解析过程中因为某个内部异常导致遇到无效令牌时使用。
        /// </summary>
        /// <param name="token">解析过程中遇到的无效解析器令牌。</param>
        /// <param name="inner_exception">导致解析错误的内部异常。</param>
        internal JsonException(ParserToken token,
                                Exception inner_exception) :
            base(String.Format(
                    "Invalid token '{0}' in input string", token),
                inner_exception)
        {
        }

        /// <summary>
        /// 内部构造函数，根据解析过程中遇到的无效字符创建 JsonException 实例。
        /// </summary>
        /// <param name="c">解析过程中遇到的无效字符的 ASCII 码值。</param>
        internal JsonException(int c) :
            base(String.Format(
                    "Invalid character '{0}' in input string", (char)c))
        {
        }

        /// <summary>
        /// 内部构造函数，根据解析过程中遇到的无效字符以及内部异常创建 JsonException 实例。
        /// 当解析过程中因为某个内部异常导致遇到无效字符时使用。
        /// </summary>
        /// <param name="c">解析过程中遇到的无效字符的 ASCII 码值。</param>
        /// <param name="inner_exception">导致解析错误的内部异常。</param>
        internal JsonException(int c, Exception inner_exception) :
            base(String.Format(
                    "Invalid character '{0}' in input string", (char)c),
                inner_exception)
        {
        }

        /// <summary>
        /// 构造函数，根据自定义的错误消息创建 JsonException 实例。
        /// 当需要抛出一个带有特定错误描述的 JSON 解析异常时使用。
        /// </summary>
        /// <param name="message">自定义的错误消息。</param>
        public JsonException(string message) : base(message)
        {
        }

        /// <summary>
        /// 构造函数，根据自定义的错误消息以及内部异常创建 JsonException 实例。
        /// 当解析过程中因为某个内部异常导致错误，并且需要提供自定义错误描述时使用。
        /// </summary>
        /// <param name="message">自定义的错误消息。</param>
        /// <param name="inner_exception">导致解析错误的内部异常。</param>
        public JsonException(string message, Exception inner_exception) :
            base(message, inner_exception)
        {
        }
    }
}