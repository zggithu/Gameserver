using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LitJson
{
    /// <summary>
    /// 有限状态机（FSM）的上下文类，用于在状态转换过程中传递和保存相关信息。
    /// </summary>
    internal class FsmContext
    {
        /// <summary>
        /// 指示是否需要返回当前处理结果。
        /// </summary>
        public bool Return;
        /// <summary>
        /// 下一个要进入的状态编号。
        /// </summary>
        public int NextState;
        /// <summary>
        /// 关联的词法分析器实例。
        /// </summary>
        public Lexer L;
        /// <summary>
        /// 状态栈，用于在处理嵌套结构时保存状态信息。
        /// </summary>
        public int StateStack;
    }

    /// <summary>
    /// 词法分析器类，用于将输入的 JSON 文本解析为一个个的词法单元（token）。
    /// </summary>
    internal class Lexer
    {
        #region Fields
        // 定义一个委托类型，用于表示状态处理方法
        private delegate bool StateHandler(FsmContext ctx);

        // 有限状态机的返回表，每个状态对应一个返回的 token 类型
        private static readonly int[] fsm_return_table;
        // 有限状态机的处理方法表，每个状态对应一个处理方法
        private static readonly StateHandler[] fsm_handler_table;

        // 是否允许注释
        private bool allow_comments;
        // 是否允许单引号字符串
        private bool allow_single_quoted_strings;
        // 是否到达输入的末尾
        private bool end_of_input;
        // 有限状态机的上下文实例
        private FsmContext fsm_context;
        // 输入缓冲区，用于保存回退的字符
        private int input_buffer;
        // 当前读取的字符
        private int input_char;
        // 输入文本的读取器
        private TextReader reader;
        // 当前所处的状态编号
        private int state;
        // 字符串缓冲区，用于拼接字符串
        private StringBuilder string_buffer;
        // 当前解析出的字符串值
        private string string_value;
        // 当前解析出的 token 类型
        private int token;
        // 用于处理 Unicode 字符的临时变量
        private int unichar;
        #endregion

        #region Properties
        /// <summary>
        /// 获取或设置是否允许注释。
        /// </summary>
        public bool AllowComments
        {
            get { return allow_comments; }
            set { allow_comments = value; }
        }

        /// <summary>
        /// 获取或设置是否允许单引号字符串。
        /// </summary>
        public bool AllowSingleQuotedStrings
        {
            get { return allow_single_quoted_strings; }
            set { allow_single_quoted_strings = value; }
        }

        /// <summary>
        /// 获取是否到达输入的末尾。
        /// </summary>
        public bool EndOfInput
        {
            get { return end_of_input; }
        }

        /// <summary>
        /// 获取当前解析出的 token 类型。
        /// </summary>
        public int Token
        {
            get { return token; }
        }

        /// <summary>
        /// 获取当前解析出的字符串值。
        /// </summary>
        public string StringValue
        {
            get { return string_value; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// 静态构造函数，初始化有限状态机的处理方法表和返回表。
        /// </summary>
        static Lexer()
        {
            PopulateFsmTables(out fsm_handler_table, out fsm_return_table);
        }

        /// <summary>
        /// 构造函数，初始化词法分析器的相关属性。
        /// </summary>
        /// <param name="reader">输入文本的读取器。</param>
        public Lexer(TextReader reader)
        {
            // 默认允许注释
            allow_comments = true;
            // 默认允许单引号字符串
            allow_single_quoted_strings = true;

            // 清空输入缓冲区
            input_buffer = 0;
            // 初始化字符串缓冲区，初始容量为 128
            string_buffer = new StringBuilder(128);
            // 初始状态为 1
            state = 1;
            // 初始未到达输入末尾
            end_of_input = false;
            // 保存输入文本的读取器
            this.reader = reader;

            // 初始化有限状态机的上下文
            fsm_context = new FsmContext();
            fsm_context.L = this;
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// 获取十六进制字符对应的十进制值。
        /// </summary>
        /// <param name="digit">十六进制字符。</param>
        /// <returns>对应的十进制值。</returns>
        private static int HexValue(int digit)
        {
            switch (digit)
            {
                case 'a':
                case 'A':
                    return 10;
                case 'b':
                case 'B':
                    return 11;
                case 'c':
                case 'C':
                    return 12;
                case 'd':
                case 'D':
                    return 13;
                case 'e':
                case 'E':
                    return 14;
                case 'f':
                case 'F':
                    return 15;
                default:
                    return digit - '0';
            }
        }

        /// <summary>
        /// 填充有限状态机的处理方法表和返回表。
        /// </summary>
        /// <param name="fsm_handler_table">处理方法表。</param>
        /// <param name="fsm_return_table">返回表。</param>
        private static void PopulateFsmTables(out StateHandler[] fsm_handler_table, out int[] fsm_return_table)
        {
            // 参考手册 A.1 节了解有限状态机的详细信息
            fsm_handler_table = new StateHandler[28]
            {
                State1,
                State2,
                State3,
                State4,
                State5,
                State6,
                State7,
                State8,
                State9,
                State10,
                State11,
                State12,
                State13,
                State14,
                State15,
                State16,
                State17,
                State18,
                State19,
                State20,
                State21,
                State22,
                State23,
                State24,
                State25,
                State26,
                State27,
                State28
            };

            fsm_return_table = new int[28]
            {
                (int)ParserToken.Char,
                0,
                (int)ParserToken.Number,
                (int)ParserToken.Number,
                0,
                (int)ParserToken.Number,
                0,
                (int)ParserToken.Number,
                0,
                0,
                (int)ParserToken.True,
                0,
                0,
                0,
                (int)ParserToken.False,
                0,
                0,
                (int)ParserToken.Null,
                (int)ParserToken.CharSeq,
                (int)ParserToken.Char,
                0,
                0,
                (int)ParserToken.CharSeq,
                (int)ParserToken.Char,
                0,
                0,
                0,
                0
            };
        }

        /// <summary>
        /// 处理转义字符，将转义字符转换为对应的实际字符。
        /// </summary>
        /// <param name="esc_char">转义字符。</param>
        /// <returns>转换后的实际字符。</returns>
        private static char ProcessEscChar(int esc_char)
        {
            switch (esc_char)
            {
                case '"':
                case '\'':
                case '\\':
                case '/':
                    return Convert.ToChar(esc_char);
                case 'n':
                    return '\n';
                case 't':
                    return '\t';
                case 'r':
                    return '\r';
                case 'b':
                    return '\b';
                case 'f':
                    return '\f';
                default:
                    // 理论上不会到达这里
                    return '?';
            }
        }

        /// <summary>
        /// 状态 1 的处理方法，处理输入的起始状态。
        /// </summary>
        /// <param name="ctx">有限状态机的上下文。</param>
        /// <returns>是否处理成功。</returns>
        private static bool State1(FsmContext ctx)
        {
            // 循环读取字符
            while (ctx.L.GetChar())
            {
                // 忽略空白字符
                if (ctx.L.input_char == ' ' ||
                    ctx.L.input_char >= '\t' && ctx.L.input_char <= '\r')
                    continue;

                // 处理数字开头的情况
                if (ctx.L.input_char >= '1' && ctx.L.input_char <= '9')
                {
                    ctx.L.string_buffer.Append((char)ctx.L.input_char);
                    ctx.NextState = 3;
                    return true;
                }

                switch (ctx.L.input_char)
                {
                    case '"':
                        ctx.NextState = 19;
                        ctx.Return = true;
                        return true;
                    case ',':
                    case ':':
                    case '[':
                    case ']':
                    case '{':
                    case '}':
                        ctx.NextState = 1;
                        ctx.Return = true;
                        return true;
                    case '-':
                        ctx.L.string_buffer.Append((char)ctx.L.input_char);
                        ctx.NextState = 2;
                        return true;
                    case '0':
                        ctx.L.string_buffer.Append((char)ctx.L.input_char);
                        ctx.NextState = 4;
                        return true;
                    case 'f':
                        ctx.NextState = 12;
                        return true;
                    case 'n':
                        ctx.NextState = 16;
                        return true;
                    case 't':
                        ctx.NextState = 9;
                        return true;
                    case '\'':
                        if (!ctx.L.allow_single_quoted_strings)
                            return false;
                        ctx.L.input_char = '"';
                        ctx.NextState = 23;
                        ctx.Return = true;
                        return true;
                    case '/':
                        if (!ctx.L.allow_comments)
                            return false;
                        ctx.NextState = 25;
                        return true;
                    default:
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 状态 2 的处理方法，处理负数的情况。
        /// </summary>
        /// <param name="ctx">有限状态机的上下文。</param>
        /// <returns>是否处理成功。</returns>
        private static bool State2(FsmContext ctx)
        {
            ctx.L.GetChar();

            if (ctx.L.input_char >= '1' && ctx.L.input_char <= '9')
            {
                ctx.L.string_buffer.Append((char)ctx.L.input_char);
                ctx.NextState = 3;
                return true;
            }

            switch (ctx.L.input_char)
            {
                case '0':
                    ctx.L.string_buffer.Append((char)ctx.L.input_char);
                    ctx.NextState = 4;
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 状态 3 的处理方法，处理整数部分的数字。
        /// </summary>
        /// <param name="ctx">有限状态机的上下文。</param>
        /// <returns>是否处理成功。</returns>
        private static bool State3(FsmContext ctx)
        {
            while (ctx.L.GetChar())
            {
                if (ctx.L.input_char >= '0' && ctx.L.input_char <= '9')
                {
                    ctx.L.string_buffer.Append((char)ctx.L.input_char);
                    continue;
                }

                if (ctx.L.input_char == ' ' ||
                    ctx.L.input_char >= '\t' && ctx.L.input_char <= '\r')
                {
                    ctx.Return = true;
                    ctx.NextState = 1;
                    return true;
                }

                switch (ctx.L.input_char)
                {
                    case ',':
                    case ']':
                    case '}':
                        ctx.L.UngetChar();
                        ctx.Return = true;
                        ctx.NextState = 1;
                        return true;
                    case '.':
                        ctx.L.string_buffer.Append((char)ctx.L.input_char);
                        ctx.NextState = 5;
                        return true;
                    case 'e':
                    case 'E':
                        ctx.L.string_buffer.Append((char)ctx.L.input_char);
                        ctx.NextState = 7;
                        return true;
                    default:
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 状态 4 的处理方法，处理以 0 开头的数字。
        /// </summary>
        /// <param name="ctx">有限状态机的上下文。</param>
        /// <returns>是否处理成功。</returns>
        private static bool State4(FsmContext ctx)
        {
            ctx.L.GetChar();

            if (ctx.L.input_char == ' ' ||
                ctx.L.input_char >= '\t' && ctx.L.input_char <= '\r')
            {
                ctx.Return = true;
                ctx.NextState = 1;
                return true;
            }

            switch (ctx.L.input_char)
            {
                case ',':
                case ']':
                case '}':
                    ctx.L.UngetChar();
                    ctx.Return = true;
                    ctx.NextState = 1;
                    return true;
                case '.':
                    ctx.L.string_buffer.Append((char)ctx.L.input_char);
                    ctx.NextState = 5;
                    return true;
                case 'e':
                case 'E':
                    ctx.L.string_buffer.Append((char)ctx.L.input_char);
                    ctx.NextState = 7;
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 状态 5 的处理方法，处理小数点后的数字。
        /// </summary>
        /// <param name="ctx">有限状态机的上下文。</param>
        /// <returns>是否处理成功。</returns>
        private static bool State5(FsmContext ctx)
        {
            ctx.L.GetChar();

            if (ctx.L.input_char >= '0' && ctx.L.input_char <= '9')
            {
                ctx.L.string_buffer.Append((char)ctx.L.input_char);
                ctx.NextState = 6;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 状态 6 的处理方法，继续处理小数点后的数字。
        /// </summary>
        /// <param name="ctx">有限状态机的上下文。</param>
        /// <returns>是否处理成功。</returns>
        private static bool State6(FsmContext ctx)
        {
            while (ctx.L.GetChar())
            {
                if (ctx.L.input_char >= '0' && ctx.L.input_char <= '9')
                {
                    ctx.L.string_buffer.Append((char)ctx.L.input_char);
                    continue;
                }

                if (ctx.L.input_char == ' ' ||
                    ctx.L.input_char >= '\t' && ctx.L.input_char <= '\r')
                {
                    ctx.Return = true;
                    ctx.NextState = 1;
                    return true;
                }

                switch (ctx.L.input_char)
                {
                    case ',':
                    case ']':
                    case '}':
                        ctx.L.UngetChar();
                        ctx.Return = true;
                        ctx.NextState = 1;
                        return true;
                    case 'e':
                    case 'E':
                        ctx.L.string_buffer.Append((char)ctx.L.input_char);
                        ctx.NextState = 7;
                        return true;
                    default:
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 状态 7 的处理方法，处理科学计数法的指数部分。
        /// </summary>
        /// <param name="ctx">有限状态机的上下文。</param>
        /// <returns>是否处理成功。</returns>
        private static bool State7(FsmContext ctx)
        {
            ctx.L.GetChar();

            if (ctx.L.input_char >= '0' && ctx.L.input_char <= '9')
            {
                ctx.L.string_buffer.Append((char)ctx.L.input_char);
                ctx.NextState = 8;
                return true;
            }

            switch (ctx.L.input_char)
            {
                case '+':
                case '-':
                    ctx.L.string_buffer.Append((char)ctx.L.input_char);
                    ctx.NextState = 8; return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 状态 8 的处理方法，继续处理科学计数法的指数部分数字。
        /// </summary>
        /// <param name="ctx">有限状态机的上下文。</param>
        /// <returns>是否处理成功。</returns>
        private static bool State8(FsmContext ctx)
        {
            while (ctx.L.GetChar())
            {
                if (ctx.L.input_char >= '0' && ctx.L.input_char <= '9')
                {
                    ctx.L.string_buffer.Append((char)ctx.L.input_char);
                    continue;
                }

                if (ctx.L.input_char == ' ' ||
                    ctx.L.input_char >= '\t' && ctx.L.input_char <= '\r')
                {
                    ctx.Return = true;
                    ctx.NextState = 1;
                    return true;
                }

                switch (ctx.L.input_char)
                {
                    case ',':
                    case ']':
                    case '}':
                        ctx.L.UngetChar();
                        ctx.Return = true;
                        ctx.NextState = 1;
                        return true;
                    default:
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 状态 9 的处理方法，开始处理 "true" 布尔值。
        /// </summary>
        /// <param name="ctx">有限状态机的上下文。</param>
        /// <returns>是否处理成功。</returns>
        private static bool State9(FsmContext ctx)
        {
            ctx.L.GetChar();

            switch (ctx.L.input_char)
            {
                case 'r':
                    ctx.NextState = 10;
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 状态 10 的处理方法，继续处理 "true" 布尔值。
        /// </summary>
        /// <param name="ctx">有限状态机的上下文。</param>
        /// <returns>是否处理成功。</returns>
        private static bool State10(FsmContext ctx)
        {
            ctx.L.GetChar();

            switch (ctx.L.input_char)
            {
                case 'u':
                    ctx.NextState = 11;
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 状态 11 的处理方法，完成处理 "true" 布尔值。
        /// </summary>
        /// <param name="ctx">有限状态机的上下文。</param>
        /// <returns>是否处理成功。</returns>
        private static bool State11(FsmContext ctx)
        {
            ctx.L.GetChar();

            switch (ctx.L.input_char)
            {
                case 'e':
                    ctx.Return = true;
                    ctx.NextState = 1;
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 状态 12 的处理方法，开始处理 "false" 布尔值。
        /// </summary>
        /// <param name="ctx">有限状态机的上下文。</param>
        /// <returns>是否处理成功。</returns>
        private static bool State12(FsmContext ctx)
        {
            ctx.L.GetChar();

            switch (ctx.L.input_char)
            {
                case 'a':
                    ctx.NextState = 13;
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 状态 13 的处理方法，继续处理 "false" 布尔值。
        /// </summary>
        /// <param name="ctx">有限状态机的上下文。</param>
        /// <returns>是否处理成功。</returns>
        private static bool State13(FsmContext ctx)
        {
            ctx.L.GetChar();

            switch (ctx.L.input_char)
            {
                case 'l':
                    ctx.NextState = 14;
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 状态 14 的处理方法，继续处理 "false" 布尔值。
        /// </summary>
        /// <param name="ctx">有限状态机的上下文。</param>
        /// <returns>是否处理成功。</returns>
        private static bool State14(FsmContext ctx)
        {
            ctx.L.GetChar();

            switch (ctx.L.input_char)
            {
                case 's':
                    ctx.NextState = 15;
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 状态 15 的处理方法，完成处理 "false" 布尔值。
        /// </summary>
        /// <param name="ctx">有限状态机的上下文。</param>
        /// <returns>是否处理成功。</returns>
        private static bool State15(FsmContext ctx)
        {
            ctx.L.GetChar();

            switch (ctx.L.input_char)
            {
                case 'e':
                    ctx.Return = true;
                    ctx.NextState = 1;
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 状态 16 的处理方法，开始处理 "null" 值。
        /// </summary>
        /// <param name="ctx">有限状态机的上下文。</param>
        /// <returns>是否处理成功。</returns>
        private static bool State16(FsmContext ctx)
        {
            ctx.L.GetChar();

            switch (ctx.L.input_char)
            {
                case 'u':
                    ctx.NextState = 17;
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 状态 17 的处理方法，继续处理 "null" 值。
        /// </summary>
        /// <param name="ctx">有限状态机的上下文。</param>
        /// <returns>是否处理成功。</returns>
        private static bool State17(FsmContext ctx)
        {
            ctx.L.GetChar();

            switch (ctx.L.input_char)
            {
                case 'l':
                    ctx.NextState = 18;
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 状态 18 的处理方法，完成处理 "null" 值。
        /// </summary>
        /// <param name="ctx">有限状态机的上下文。</param>
        /// <returns>是否处理成功。</returns>
        private static bool State18(FsmContext ctx)
        {
            ctx.L.GetChar();

            switch (ctx.L.input_char)
            {
                case 'l':
                    ctx.Return = true;
                    ctx.NextState = 1;
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 状态 19 的处理方法，开始处理双引号字符串。
        /// </summary>
        /// <param name="ctx">有限状态机的上下文。</param>
        /// <returns>是否处理成功。</returns>
        private static bool State19(FsmContext ctx)
        {
            while (ctx.L.GetChar())
            {
                switch (ctx.L.input_char)
                {
                    case '"':
                        ctx.L.UngetChar();
                        ctx.Return = true;
                        ctx.NextState = 20;
                        return true;
                    case '\\':
                        ctx.StateStack = 19;
                        ctx.NextState = 21;
                        return true;
                    default:
                        ctx.L.string_buffer.Append((char)ctx.L.input_char);
                        continue;
                }
            }

            return true;
        }

        /// <summary>
        /// 状态 20 的处理方法，结束处理双引号字符串。
        /// </summary>
        /// <param name="ctx">有限状态机的上下文。</param>
        /// <returns>是否处理成功。</returns>
        private static bool State20(FsmContext ctx)
        {
            ctx.L.GetChar();

            switch (ctx.L.input_char)
            {
                case '"':
                    ctx.Return = true;
                    ctx.NextState = 1;
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 状态 21 的处理方法，处理字符串中的转义字符。
        /// </summary>
        /// <param name="ctx">有限状态机的上下文。</param>
        /// <returns>是否处理成功。</returns>
        private static bool State21(FsmContext ctx)
        {
            ctx.L.GetChar();

            switch (ctx.L.input_char)
            {
                case 'u':
                    ctx.NextState = 22;
                    return true;
                case '"':
                case '\'':
                case '/':
                case '\\':
                case 'b':
                case 'f':
                case 'n':
                case 'r':
                case 't':
                    ctx.L.string_buffer.Append(
                        ProcessEscChar(ctx.L.input_char));
                    ctx.NextState = ctx.StateStack;
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 状态 22 的处理方法，处理字符串中的 Unicode 转义字符。
        /// </summary>
        /// <param name="ctx">有限状态机的上下文。</param>
        /// <returns>是否处理成功。</returns>
        private static bool State22(FsmContext ctx)
        {
            int counter = 0;
            int mult = 4096;

            ctx.L.unichar = 0;

            while (ctx.L.GetChar())
            {

                if (ctx.L.input_char >= '0' && ctx.L.input_char <= '9' ||
                    ctx.L.input_char >= 'A' && ctx.L.input_char <= 'F' ||
                    ctx.L.input_char >= 'a' && ctx.L.input_char <= 'f')
                {

                    ctx.L.unichar += HexValue(ctx.L.input_char) * mult;

                    counter++;
                    mult /= 16;

                    if (counter == 4)
                    {
                        ctx.L.string_buffer.Append(
                            Convert.ToChar(ctx.L.unichar));
                        ctx.NextState = ctx.StateStack;
                        return true;
                    }

                    continue;
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// 状态 23 的处理方法，开始处理单引号字符串（如果允许）。
        /// </summary>
        /// <param name="ctx">有限状态机的上下文。</param>
        /// <returns>是否处理成功。</returns>
        private static bool State23(FsmContext ctx)
        {
            while (ctx.L.GetChar())
            {
                switch (ctx.L.input_char)
                {
                    case '\'':
                        ctx.L.UngetChar();
                        ctx.Return = true;
                        ctx.NextState = 24;
                        return true;
                    case '\\':
                        ctx.StateStack = 23;
                        ctx.NextState = 21;
                        return true;
                    default:
                        ctx.L.string_buffer.Append((char)ctx.L.input_char);
                        continue;
                }
            }

            return true;
        }

        /// <summary>
        /// 状态 24 的处理方法，结束处理单引号字符串（如果允许）。
        /// </summary>
        /// <param name="ctx">有限状态机的上下文。</param>
        /// <returns>是否处理成功。</returns>
        private static bool State24(FsmContext ctx)
        {
            ctx.L.GetChar();

            switch (ctx.L.input_char)
            {
                case '\'':
                    ctx.L.input_char = '"';
                    ctx.Return = true;
                    ctx.NextState = 1;
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 状态 25 的处理方法，开始处理注释。
        /// </summary>
        /// <param name="ctx">有限状态机的上下文。</param>
        /// <returns>是否处理成功。</returns>
        private static bool State25(FsmContext ctx)
        {
            ctx.L.GetChar();

            switch (ctx.L.input_char)
            {
                case '*':
                    ctx.NextState = 27;
                    return true;
                case '/':
                    ctx.NextState = 26;
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 状态 26 的处理方法，处理单行注释。
        /// </summary>
        /// <param name="ctx">有限状态机的上下文。</param>
        /// <returns>是否处理成功。</returns>
        private static bool State26(FsmContext ctx)
        {
            while (ctx.L.GetChar())
            {
                if (ctx.L.input_char == '\n')
                {
                    ctx.NextState = 1;
                    return true;
                }
            }

            return true;
        }

        /// <summary>
        /// 状态 27 的处理方法，处理多行注释开始后的部分。
        /// </summary>
        /// <param name="ctx">有限状态机的上下文。</param>
        /// <returns>是否处理成功。</returns>
        private static bool State27(FsmContext ctx)
        {
            while (ctx.L.GetChar())
            {
                if (ctx.L.input_char == '*')
                {
                    ctx.NextState = 28;
                    return true;
                }
            }

            return true;
        }

        /// <summary>
        /// 状态 28 的处理方法，处理多行注释结束部分。
        /// </summary>
        /// <param name="ctx">有限状态机的上下文。</param>
        /// <returns>是否处理成功。</returns>
        private static bool State28(FsmContext ctx)
        {
            while (ctx.L.GetChar())
            {
                if (ctx.L.input_char == '*')
                    continue;

                if (ctx.L.input_char == '/')
                {
                    ctx.NextState = 1;
                    return true;
                }

                ctx.NextState = 27;
                return true;
            }

            return true;
        }
        #endregion


        /// <summary>
        /// 读取下一个字符，如果到达输入末尾则标记结束。
        /// </summary>
        /// <returns>是否成功读取字符。</returns>
        private bool GetChar()
        {
            if ((input_char = NextChar()) != -1)
                return true;

            end_of_input = true;
            return false;
        }

        /// <summary>
        /// 获取下一个字符，优先从输入缓冲区读取。
        /// </summary>
        /// <returns>读取的字符，如果到达末尾返回 -1。</returns>
        private int NextChar()
        {
            if (input_buffer != 0)
            {
                int tmp = input_buffer;
                input_buffer = 0;

                return tmp;
            }

            return reader.Read();
        }

        /// <summary>
        /// 解析下一个词法单元（token）。
        /// </summary>
        /// <returns>是否成功解析到下一个 token。</returns>
        public bool NextToken()
        {
            StateHandler handler;
            fsm_context.Return = false;

            while (true)
            {
                handler = fsm_handler_table[state - 1];

                if (!handler(fsm_context))
                    throw new JsonException(input_char);

                if (end_of_input)
                    return false;

                if (fsm_context.Return)
                {
                    string_value = string_buffer.ToString();
                    string_buffer.Remove(0, string_buffer.Length);
                    token = fsm_return_table[state - 1];

                    if (token == (int)ParserToken.Char)
                        token = input_char;

                    state = fsm_context.NextState;

                    return true;
                }

                state = fsm_context.NextState;
            }
        }

        /// <summary>
        /// 将当前字符回退到输入缓冲区。
        /// </summary>
        private void UngetChar()
        {
            input_buffer = input_char;
        }
    }
}