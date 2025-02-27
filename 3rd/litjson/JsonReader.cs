using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

// ����LitJson�����ռ䣬������֯��JSON������ص����ö��
namespace LitJson
{
    // ����JsonTokenö�٣����ڱ�ʾ��ͬ���͵�JSON����
    public enum JsonToken
    {
        None,  // ����Ч����

        ObjectStart,  // ����ʼ����Ӧ '{'
        PropertyName, // ����������
        ObjectEnd,    // �����������Ӧ '}'

        ArrayStart,   // ���鿪ʼ����Ӧ '['
        ArrayEnd,     // �����������Ӧ ']'

        Int,          // ��������
        Long,         // ����������
        Double,       // ˫���ȸ���������

        String,       // �ַ�������

        Boolean,      // ��������
        Null          // ��ֵ����
    }

    // ����JsonReader�࣬���ڽ���JSON����
    public class JsonReader
    {
        #region Fields
        // ��ֻ̬���ֵ䣬�洢����������״̬���Ľ���
        private static readonly IDictionary<int, IDictionary<int, int[]>> parse_table;

        private Stack<int> automaton_stack;  // ״̬��ջ�����ڸ��ٽ���״̬
        private int current_input;    // ��ǰ���������
        private int current_symbol;   // ��ǰ����ķ���
        private bool end_of_json;      // ��ʾ�Ƿ񵽴�JSON���ݵ�ĩβ
        private bool end_of_input;     // ��ʾ�Ƿ񵽴��������ݵ�ĩβ
        private Lexer lexer;            // �ʷ������������ڽ��������JSON�ı�
        private bool parser_in_string; // ��ʾ�Ƿ����ڴ����ַ���
        private bool parser_return;    // ��ʾ�������Ƿ���Ҫ����
        private bool read_started;     // ��ʾ�Ƿ��Ѿ���ʼ��ȡJSON����
        private TextReader reader;           // ���ڶ�ȡ������ı���ȡ��
        private bool reader_is_owned;  // ��ʾ�Ƿ�ӵ���ı���ȡ��������Ȩ
        private bool skip_non_members; // ��ʾ�Ƿ������ǳ�Ա����
        private object token_value;      // ��ǰ���Ƶ�ֵ
        private JsonToken token;            // ��ǰ���Ƶ�����
        #endregion

        #region Public Properties
        // ��ȡ�������Ƿ�������JSON��ʹ��ע��
        public bool AllowComments
        {
            get { return lexer.AllowComments; }
            set { lexer.AllowComments = value; }
        }

        // ��ȡ�������Ƿ�������JSON��ʹ�õ������ַ���
        public bool AllowSingleQuotedStrings
        {
            get { return lexer.AllowSingleQuotedStrings; }
            set { lexer.AllowSingleQuotedStrings = value; }
        }

        // ��ȡ�������Ƿ������ǳ�Ա����
        public bool SkipNonMembers
        {
            get { return skip_non_members; }
            set { skip_non_members = value; }
        }

        // ��ȡ�Ƿ񵽴��������ݵ�ĩβ
        public bool EndOfInput
        {
            get { return end_of_input; }
        }

        // ��ȡ�Ƿ񵽴�JSON���ݵ�ĩβ
        public bool EndOfJson
        {
            get { return end_of_json; }
        }

        // ��ȡ��ǰ���Ƶ�����
        public JsonToken Token
        {
            get { return token; }
        }

        // ��ȡ��ǰ���Ƶ�ֵ
        public object Value
        {
            get { return token_value; }
        }
        #endregion

        #region Constructors
        // ��̬���캯������ʼ��������
        static JsonReader()
        {
            parse_table = PopulateParseTable();
        }

        // ���캯��������JSON�ı��ַ�����Ϊ����
        public JsonReader(string json_text) :
            this(new StringReader(json_text), true)
        {
        }

        // ���캯���������ı���ȡ����Ϊ����
        public JsonReader(TextReader reader) :
            this(reader, false)
        {
        }

        // ˽�й��캯������ʼ��JsonReader�ĸ�������
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
        // ��������ľ�̬����
        private static IDictionary<int, IDictionary<int, int[]>> PopulateParseTable()
        {
            // ��ʼ��������
            IDictionary<int, IDictionary<int, int[]>> parse_table = new Dictionary<int, IDictionary<int, int[]>>();

            // ��ӽ�������к��У�����JSON�﷨����
            TableAddRow(parse_table, ParserToken.Array);
            TableAddCol(parse_table, ParserToken.Array, '[',
                            '[',
                            (int)ParserToken.ArrayPrime);

            TableAddRow(parse_table, ParserToken.ArrayPrime);
            TableAddCol(parse_table, ParserToken.ArrayPrime, '"',
                            (int)ParserToken.Value,
                            (int)ParserToken.ValueRest,
                            ']');
            // ʡ�Բ�������к��еĴ��룬ԭ����ͬ

            return parse_table;
        }

        // �������������еľ�̬����
        private static void TableAddCol(IDictionary<int, IDictionary<int, int[]>> parse_table, ParserToken row, int col,
                                         params int[] symbols)
        {
            parse_table[(int)row].Add(col, symbols);
        }

        // �������������еľ�̬����
        private static void TableAddRow(IDictionary<int, IDictionary<int, int[]>> parse_table, ParserToken rule)
        {
            parse_table.Add((int)rule, new Dictionary<int, int[]>());
        }
        #endregion

        #region Private Methods
        // �����������͵�˽�з���
        private void ProcessNumber(string number)
        {
            // �ж������Ƿ����С���㡢e��E
            if (number.IndexOf('.') != -1 ||
                number.IndexOf('e') != -1 ||
                number.IndexOf('E') != -1)
            {

                double n_double;
                // ���Խ����ֽ���Ϊ˫���ȸ�����
                if (double.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out n_double))
                {
                    token = JsonToken.Double;
                    token_value = n_double;

                    return;
                }
            }

            int n_int32;
            // ���Խ����ֽ���Ϊ32λ����
            if (int.TryParse(number, NumberStyles.Integer, CultureInfo.InvariantCulture, out n_int32))
            {
                token = JsonToken.Int;
                token_value = n_int32;

                return;
            }

            long n_int64;
            // ���Խ����ֽ���Ϊ64λ����
            if (long.TryParse(number, NumberStyles.Integer, CultureInfo.InvariantCulture, out n_int64))
            {
                token = JsonToken.Long;
                token_value = n_int64;

                return;
            }

            ulong n_uint64;
            // ���Խ����ֽ���Ϊ�޷���64λ����
            if (ulong.TryParse(number, NumberStyles.Integer, CultureInfo.InvariantCulture, out n_uint64))
            {
                token = JsonToken.Long;
                token_value = n_uint64;

                return;
            }

            // ������޷�������Ĭ������Ϊ�������ͣ�ֵΪ0
            token = JsonToken.Int;
            token_value = 0;
        }

        // ������ŵ�˽�з���
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

        // ��ȡ��һ�����Ƶ�˽�з���
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

        // �رն�ȡ���Ĺ�������
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

        // ��ȡJSON���ݵĹ�������
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