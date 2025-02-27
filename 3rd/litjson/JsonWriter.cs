using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace LitJson
{
    // 内部枚举，定义写入过程中的各种条件
    internal enum Condition
    {
        InArray,        // 在数组中
        InObject,       // 在对象中
        NotAProperty,   // 不是对象属性
        Property,       // 对象属性
        Value           // 值
    }

    // 内部类，用于存储写入上下文信息
    internal class WriterContext
    {
        public int Count;           // 当前上下文中的元素数量
        public bool InArray;         // 是否在数组中
        public bool InObject;        // 是否在对象中
        public bool ExpectingValue;  // 是否期待一个值
        public int Padding;         // 用于格式化输出的填充长度
    }

    // 公共类，用于将数据以 JSON 格式写入文本流
    public class JsonWriter
    {
        #region Fields
        // 静态只读的数字格式信息，使用不变文化信息
        private static readonly NumberFormatInfo number_format;

        private WriterContext context;         // 当前写入上下文
        private Stack<WriterContext> ctx_stack;       // 上下文栈，用于嵌套结构
        private bool has_reached_end; // 是否已经到达写入结束状态
        private char[] hex_seq;         // 用于存储十六进制字符序列
        private int indentation;     // 当前缩进量
        private int indent_value;    // 每次缩进的空格数
        private StringBuilder inst_string_builder; // 内部使用的字符串构建器
        private bool pretty_print;    // 是否进行格式化输出
        private bool validate;        // 是否进行输入验证
        private bool lower_case_properties; // 是否将属性名转换为小写
        private TextWriter writer;          // 用于写入数据的文本写入器
        #endregion

        #region Properties
        // 获取或设置每次缩进的空格数
        public int IndentValue
        {
            get { return indent_value; }
            set
            {
                indentation = (indentation / indent_value) * value;
                indent_value = value;
            }
        }

        // 获取或设置是否进行格式化输出
        public bool PrettyPrint
        {
            get { return pretty_print; }
            set { pretty_print = value; }
        }

        // 获取当前使用的文本写入器
        public TextWriter TextWriter
        {
            get { return writer; }
        }

        // 获取或设置是否进行输入验证
        public bool Validate
        {
            get { return validate; }
            set { validate = value; }
        }

        // 获取或设置是否将属性名转换为小写
        public bool LowerCaseProperties
        {
            get { return lower_case_properties; }
            set { lower_case_properties = value; }
        }
        #endregion

        #region Constructors
        // 静态构造函数，初始化数字格式信息
        static JsonWriter()
        {
            number_format = NumberFormatInfo.InvariantInfo;
        }

        // 无参构造函数，使用内部的字符串构建器
        public JsonWriter()
        {
            inst_string_builder = new StringBuilder();
            writer = new StringWriter(inst_string_builder);

            Init();
        }

        // 接受 StringBuilder 参数的构造函数
        public JsonWriter(StringBuilder sb) :
            this(new StringWriter(sb))
        {
        }

        // 接受 TextWriter 参数的构造函数
        public JsonWriter(TextWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            this.writer = writer;

            Init();
        }
        #endregion

        #region Private Methods
        // 进行输入验证的私有方法
        private void DoValidation(Condition cond)
        {
            if (!context.ExpectingValue)
                context.Count++;

            if (!validate)
                return;

            if (has_reached_end)
                throw new JsonException(
                    "A complete JSON symbol has already been written");

            switch (cond)
            {
                case Condition.InArray:
                    if (!context.InArray)
                        throw new JsonException(
                            "Can't close an array here");
                    break;

                case Condition.InObject:
                    if (!context.InObject || context.ExpectingValue)
                        throw new JsonException(
                            "Can't close an object here");
                    break;

                case Condition.NotAProperty:
                    if (context.InObject && !context.ExpectingValue)
                        throw new JsonException(
                            "Expected a property");
                    break;

                case Condition.Property:
                    if (!context.InObject || context.ExpectingValue)
                        throw new JsonException(
                            "Can't add a property here");
                    break;

                case Condition.Value:
                    if (!context.InArray &&
                        (!context.InObject || !context.ExpectingValue))
                        throw new JsonException(
                            "Can't add a value here");

                    break;
            }
        }

        // 初始化写入器的私有方法
        private void Init()
        {
            has_reached_end = false;
            hex_seq = new char[4];
            indentation = 0;
            indent_value = 4;
            pretty_print = false;
            validate = true;
            lower_case_properties = false;

            ctx_stack = new Stack<WriterContext>();
            context = new WriterContext();
            ctx_stack.Push(context);
        }

        // 将整数转换为十六进制字符序列的私有方法
        private static void IntToHex(int n, char[] hex)
        {
            int num;

            for (int i = 0; i < 4; i++)
            {
                num = n % 16;

                if (num < 10)
                    hex[3 - i] = (char)('0' + num);
                else
                    hex[3 - i] = (char)('A' + (num - 10));

                n >>= 4;
            }
        }

        // 增加缩进量的私有方法
        private void Indent()
        {
            if (pretty_print)
                indentation += indent_value;
        }

        // 向文本写入器写入字符串的私有方法，会根据格式化选项添加缩进
        private void Put(string str)
        {
            if (pretty_print && !context.ExpectingValue)
                for (int i = 0; i < indentation; i++)
                    writer.Write(' ');

            writer.Write(str);
        }

        // 写入换行符的私有方法，可选择是否添加逗号
        private void PutNewline()
        {
            PutNewline(true);
        }

        private void PutNewline(bool add_comma)
        {
            if (add_comma && !context.ExpectingValue &&
                context.Count > 1)
                writer.Write(',');

            if (pretty_print && !context.ExpectingValue)
                writer.Write(Environment.NewLine);
        }

        // 写入字符串的私有方法，会处理特殊字符
        private void PutString(string str)
        {
            Put(String.Empty);

            writer.Write('"');

            int n = str.Length;
            for (int i = 0; i < n; i++)
            {
                switch (str[i])
                {
                    case '\n':
                        writer.Write("\\n");
                        continue;

                    case '\r':
                        writer.Write("\\r");
                        continue;

                    case '\t':
                        writer.Write("\\t");
                        continue;

                    case '"':
                    case '\\':
                        writer.Write('\\');
                        writer.Write(str[i]);
                        continue;

                    case '\f':
                        writer.Write("\\f");
                        continue;

                    case '\b':
                        writer.Write("\\b");
                        continue;
                }

                if ((int)str[i] >= 32 && (int)str[i] <= 126)
                {
                    writer.Write(str[i]);
                    continue;
                }

                // 默认情况，将字符转换为 \uXXXX 序列
                IntToHex((int)str[i], hex_seq);
                writer.Write("\\u");
                writer.Write(hex_seq);
            }

            writer.Write('"');
        }

        // 减少缩进量的私有方法
        private void Unindent()
        {
            if (pretty_print)
                indentation -= indent_value;
        }
        #endregion

        // 重写 ToString 方法，返回内部字符串构建器的内容
        public override string ToString()
        {
            if (inst_string_builder == null)
                return String.Empty;

            return inst_string_builder.ToString();
        }

        // 重置写入器状态的公共方法
        public void Reset()
        {
            has_reached_end = false;

            ctx_stack.Clear();
            context = new WriterContext();
            ctx_stack.Push(context);

            if (inst_string_builder != null)
                inst_string_builder.Remove(0, inst_string_builder.Length);
        }

        // 写入布尔值的公共方法
        public void Write(bool boolean)
        {
            DoValidation(Condition.Value);
            PutNewline();

            Put(boolean ? "true" : "false");

            context.ExpectingValue = false;
        }

        // 写入十进制数的公共方法
        public void Write(decimal number)
        {
            DoValidation(Condition.Value);
            PutNewline();

            Put(Convert.ToString(number, number_format));

            context.ExpectingValue = false;
        }

        // 写入双精度浮点数的公共方法
        public void Write(double number)
        {
            DoValidation(Condition.Value);
            PutNewline();

            string str = Convert.ToString(number, number_format);
            Put(str);

            if (str.IndexOf('.') == -1 &&
                str.IndexOf('E') == -1)
                writer.Write(".0");

            context.ExpectingValue = false;
        }

        // 写入单精度浮点数的公共方法
        public void Write(float number)
        {
            DoValidation(Condition.Value);
            PutNewline();

            string str = Convert.ToString(number, number_format);
            Put(str);

            context.ExpectingValue = false;
        }

        // 写入整数的公共方法
        public void Write(int number)
        {
            DoValidation(Condition.Value);
            PutNewline();

            Put(Convert.ToString(number, number_format));

            context.ExpectingValue = false;
        }

        // 写入长整数的公共方法
        public void Write(long number)
        {
            DoValidation(Condition.Value);
            PutNewline();

            Put(Convert.ToString(number, number_format));

            context.ExpectingValue = false;
        }

        // 写入字符串的公共方法
        public void Write(string str)
        {
            DoValidation(Condition.Value);
            PutNewline();

            if (str == null)
                Put("null");
            else
                PutString(str);

            context.ExpectingValue = false;
        }

        // 写入无符号长整数的公共方法
        [CLSCompliant(false)]
        public void Write(ulong number)
        {
            DoValidation(Condition.Value);
            PutNewline();

            Put(Convert.ToString(number, number_format));

            context.ExpectingValue = false;
        }

        // 写入数组结束符号的公共方法
        public void WriteArrayEnd()
        {
            DoValidation(Condition.InArray);
            PutNewline(false);

            ctx_stack.Pop();
            if (ctx_stack.Count == 1)
                has_reached_end = true;
            else
            {
                context = ctx_stack.Peek();
                context.ExpectingValue = false;
            }

            Unindent();
            Put("]");
        }

        // 写入数组开始符号的公共方法
        public void WriteArrayStart()
        {
            DoValidation(Condition.NotAProperty);
            PutNewline();

            Put("[");

            context = new WriterContext();
            context.InArray = true;
            ctx_stack.Push(context);

            Indent();
        }

        // 写入对象结束符号的公共方法
        public void WriteObjectEnd()
        {
            DoValidation(Condition.InObject);
            PutNewline(false);

            ctx_stack.Pop();
            if (ctx_stack.Count == 1)
                has_reached_end = true;
            else
            {
                context = ctx_stack.Peek();
                context.ExpectingValue = false;
            }

            Unindent();
            Put("}");
        }

        // 写入对象开始符号的公共方法
        public void WriteObjectStart()
        {
            DoValidation(Condition.NotAProperty);
            PutNewline();

            Put("{");

            context = new WriterContext();
            context.InObject = true;
            ctx_stack.Push(context);

            Indent();
        }

        // 写入对象属性名的公共方法
        public void WritePropertyName(string property_name)
        {
            DoValidation(Condition.Property);
            PutNewline();
            string propertyName = (property_name == null || !lower_case_properties)
                ? property_name
                : property_name.ToLowerInvariant();

            PutString(propertyName);

            if (pretty_print)
            {
                if (propertyName.Length > context.Padding)
                    context.Padding = propertyName.Length;

                for (int i = context.Padding - propertyName.Length;
                     i >= 0; i--)
                    writer.Write(' ');

                writer.Write(": ");
            }
            else
                writer.Write(':');

            context.ExpectingValue = true;
        }
    }
}