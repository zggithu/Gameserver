using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

// 定义LitJson命名空间，用于组织与JSON处理相关的类和枚举
namespace LitJson
{
    // 定义JsonToken枚举，用于表示不同类型的JSON令牌
    public enum JsonToken
    {
        None,  // 无有效令牌

        ObjectStart,  // 对象开始，对应 '{'
        PropertyName, // 对象属性名
        ObjectEnd,    // 对象结束，对应 '}'

        ArrayStart,   // 数组开始，对应 '['
        ArrayEnd,     // 数组结束，对应 ']'

        Int,          // 整数类型
        Long,         // 长整数类型
        Double,       // 双精度浮点数类型

        String,       // 字符串类型

        Boolean,      // 布尔类型
        Null          // 空值类型
    }

    // 定义JsonReader类，用于解析JSON数据
    public class JsonReader
    {
        #region Fields
        // 静态只读字典，存储解析表，用于状态机的解析
        private static readonly IDictionary<int, IDictionary<int, int[]>> parse_table;

        private Stack<int> automaton_stack;  // 状态机栈，用于跟踪解析状态
        private int current_input;    // 当前输入的令牌
        private int current_symbol;   // 当前处理的符号
        private bool end_of_json;      // 表示是否到达JSON数据的末尾
        private bool end_of_input;     // 表示是否到达输入数据的末尾
        private Lexer lexer;            // 词法分析器，用于解析输入的JSON文本
        private bool parser_in_string; // 表示是否正在处理字符串
        private bool parser_return;    // 表示解析器是否需要返回
        private bool read_started;     // 表示是否已经开始读取JSON数据
        private TextReader reader;           // 用于读取输入的文本读取器
        private bool reader_is_owned;  // 表示是否拥有文本读取器的所有权
        private bool skip_non_members; // 表示是否跳过非成员数据
        private object token_value;      // 当前令牌的值
        private JsonToken token;            // 当前令牌的类型
        #endregion

        #region Public Properties
        // 获取或设置是否允许在JSON中使用注释
        public bool AllowComments
        {
            get { return lexer.AllowComments; }
            set { lexer.AllowComments = value; }
        }

        // 获取或设置是否允许在JSON中使用单引号字符串
        public bool AllowSingleQuotedStrings
        {
            get { return lexer.AllowSingleQuotedStrings; }
            set { lexer.AllowSingleQuotedStrings = value; }
        }

        // 获取或设置是否跳过非成员数据
        public bool SkipNonMembers
        {
            get { return skip_non_members; }
            set { skip_non_members = value; }
        }

        // 获取是否到达输入数据的末尾
        public bool EndOfInput
        {
            get { return end_of_input; }
        }

        // 获取是否到达JSON数据的末尾
        public bool EndOfJson
        {
            get { return end_of_json; }
        }

        // 获取当前令牌的类型
        public JsonToken Token
        {
            get { return token; }
        }

        // 获取当前令牌的值
        public object Value
        {
            get { return token_value; }
        }
        #endregion

        #region Constructors
        // 静态构造函数，初始化解析表
        static JsonReader()
        {
            parse_table = PopulateParseTable();
        }

        // 构造函数，接受JSON文本字符串作为输入
        public JsonReader(string json_text) :
            this(new StringReader(json_text), true)
        {
        }

        // 构造函数，接受文本读取器作为输入
        public JsonReader(TextReader reader) :
            this(reader, false)
        {
        }

        // 私有构造函数，初始化JsonReader的各种属性
        private JsonReader(TextReader reader, bool owned)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            parser_in_string = false;
            parser_return = false;

            read_started = false;
            automaton_stack = new Stack<int>();
            automaton_stack.Push((int)ParserToken.End);
            automaton_stack.Push((int)ParserToken.Text);

            lexer = new Lexer(reader);

            end_of_input = false;
            end_of_json = false;

            skip_non_members = true;

            this.reader = reader;
            reader_is_owned = owned;
        }
        #endregion

        #region Static Methods
        // 填充解析表的静态方法
        private static IDictionary<int, IDictionary<int, int[]>> PopulateParseTable()
        {
            // 初始化解析表
            IDictionary<int, IDictionary<int, int[]>> parse_table = new Dictionary<int, IDictionary<int, int[]>>();

            // 添加解析表的行和列，定义JSON语法规则
            TableAddRow(parse_table, ParserToken.Array);
            TableAddCol(parse_table, ParserToken.Array, '[',
                            '[',
                            (int)ParserToken.ArrayPrime);

            TableAddRow(parse_table, ParserToken.ArrayPrime);
            TableAddCol(parse_table, ParserToken.ArrayPrime, '"',
                            (int)ParserToken.Value,
                            (int)ParserToken.ValueRest,
                            ']');
            // 省略部分添加行和列的代码，原理相同

            return parse_table;
        }

        // 向解析表中添加列的静态方法
        private static void TableAddCol(IDictionary<int, IDictionary<int, int[]>> parse_table, ParserToken row, int col,
                                         params int[] symbols)
        {
            parse_table[(int)row].Add(col, symbols);
        }

        // 向解析表中添加行的静态方法
        private static void TableAddRow(IDictionary<int, IDictionary<int, int[]>> parse_table, ParserToken rule)
        {
            parse_table.Add((int)rule, new Dictionary<int, int[]>());
        }
        #endregion

        #region Private Methods
        // 处理数字类型的私有方法
        private void ProcessNumber(string number)
        {
            // 判断数字是否包含小数点、e或E
            if (number.IndexOf('.') != -1 ||
                number.IndexOf('e') != -1 ||
                number.IndexOf('E') != -1)
            {

                double n_double;
                // 尝试将数字解析为双精度浮点数
                if (double.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out n_double))
                {
                    token = JsonToken.Double;
                    token_value = n_double;

                    return;
                }
            }

            int n_int32;
            // 尝试将数字解析为32位整数
            if (int.TryParse(number, NumberStyles.Integer, CultureInfo.InvariantCulture, out n_int32))
            {
                token = JsonToken.Int;
                token_value = n_int32;

                return;
            }

            long n_int64;
            // 尝试将数字解析为64位整数
            if (long.TryParse(number, NumberStyles.Integer, CultureInfo.InvariantCulture, out n_int64))
            {
                token = JsonToken.Long;
                token_value = n_int64;

                return;
            }

            ulong n_uint64;
            // 尝试将数字解析为无符号64位整数
            if (ulong.TryParse(number, NumberStyles.Integer, CultureInfo.InvariantCulture, out n_uint64))
            {
                token = JsonToken.Long;
                token_value = n_uint64;

                return;
            }

            // 如果都无法解析，默认设置为整数类型，值为0
            token = JsonToken.Int;
            token_value = 0;
        }

        // 处理符号的私有方法
        private void ProcessSymbol()
        {
            if (current_symbol == '[')
            {
                token = JsonToken.ArrayStart;
                parser_return = true;

            }
            else if (current_symbol == ']')
            {
                token = JsonToken.ArrayEnd;
                parser_return = true;

            }
            else if (current_symbol == '{')
            {
                token = JsonToken.ObjectStart;
                parser_return = true;

            }
            else if (current_symbol == '}')
            {
                token = JsonToken.ObjectEnd;
                parser_return = true;

            }
            else if (current_symbol == '"')
            {
                if (parser_in_string)
                {
                    parser_in_string = false;

                    parser_return = true;

                }
                else
                {
                    if (token == JsonToken.None)
                        token = JsonToken.String;

                    parser_in_string = true;
                }

            }
            else if (current_symbol == (int)ParserToken.CharSeq)
            {
                token_value = lexer.StringValue;

            }
            else if (current_symbol == (int)ParserToken.False)
            {
                token = JsonToken.Boolean;
                token_value = false;
                parser_return = true;

            }
            else if (current_symbol == (int)ParserToken.Null)
            {
                token = JsonToken.Null;
                parser_return = true;

            }
            else if (current_symbol == (int)ParserToken.Number)
            {
                ProcessNumber(lexer.StringValue);

                parser_return = true;

            }
            else if (current_symbol == (int)ParserToken.Pair)
            {
                token = JsonToken.PropertyName;

            }
            else if (current_symbol == (int)ParserToken.True)
            {
                token = JsonToken.Boolean;
                token_value = true;
                parser_return = true;

            }
        }

        // 读取下一个令牌的私有方法
        private bool ReadToken()
        {
            if (end_of_input)
                return false;

            lexer.NextToken();

            if (lexer.EndOfInput)
            {
                Close();

                return false;
            }

            current_input = lexer.Token;

            return true;
        }
        #endregion

        // 关闭读取器的公共方法
        public void Close()
        {
            if (end_of_input)
                return;

            end_of_input = true;
            end_of_json = true;

            if (reader_is_owned)
            {
                using (reader) { }
            }

            reader = null;
        }

        // 读取JSON数据的公共方法
        public bool Read()
        {
            if (end_of_input)
                return false;

            if (end_of_json)
            {
                end_of_json = false;
                automaton_stack.Clear();
                automaton_stack.Push((int)ParserToken.End);
                automaton_stack.Push((int)ParserToken.Text);
            }

            parser_in_string = false;
            parser_return = false;

            token = JsonToken.None;
            token_value = null;

            if (!read_started)
            {
                read_started = true;

                if (!ReadToken())
                    return false;
            }

            int[] entry_symbols;

            while (true)
            {
                if (parser_return)
                {
                    if (automaton_stack.Peek() == (int)ParserToken.End)
                        end_of_json = true;

                    return true;
                }

                current_symbol = automaton_stack.Pop();

                ProcessSymbol();

                if (current_symbol == current_input)
                {
                    if (!ReadToken())
                    {
                        if (automaton_stack.Peek() != (int)ParserToken.End)
                            throw new JsonException(
                                "Input doesn't evaluate to proper JSON text");

                        if (parser_return)
                            return true;

                        return false;
                    }

                    continue;
                }

                try
                {
                    entry_symbols =
                        parse_table[current_symbol][current_input];

                }
                catch (KeyNotFoundException e)
                {
                    throw new JsonException((ParserToken)current_input, e);
                }

                if (entry_symbols[0] == (int)ParserToken.Epsilon)
                    continue;

                for (int i = entry_symbols.Length - 1; i >= 0; i--)
                    automaton_stack.Push(entry_symbols[i]);
            }
        }
    }
}