using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LitJson
{
    /// <summary>
    /// ����״̬����FSM�����������࣬������״̬ת�������д��ݺͱ��������Ϣ��
    /// </summary>
    internal class FsmContext
    {
        /// <summary>
        /// ָʾ�Ƿ���Ҫ���ص�ǰ��������
        /// </summary>
        public bool Return;
        /// <summary>
        /// ��һ��Ҫ�����״̬��š�
        /// </summary>
        public int NextState;
        /// <summary>
        /// �����Ĵʷ�������ʵ����
        /// </summary>
        public Lexer L;
        /// <summary>
        /// ״̬ջ�������ڴ���Ƕ�׽ṹʱ����״̬��Ϣ��
        /// </summary>
        public int StateStack;
    }

    /// <summary>
    /// �ʷ��������࣬���ڽ������ JSON �ı�����Ϊһ�����Ĵʷ���Ԫ��token����
    /// </summary>
    internal class Lexer
    {
        #region Fields
        // ����һ��ί�����ͣ����ڱ�ʾ״̬������
        private delegate bool StateHandler(FsmContext ctx);

        // ����״̬���ķ��ر�ÿ��״̬��Ӧһ�����ص� token ����
        private static readonly int[] fsm_return_table;
        // ����״̬���Ĵ�������ÿ��״̬��Ӧһ��������
        private static readonly StateHandler[] fsm_handler_table;

        // �Ƿ�����ע��
        private bool allow_comments;
        // �Ƿ����������ַ���
        private bool allow_single_quoted_strings;
        // �Ƿ񵽴������ĩβ
        private bool end_of_input;
        // ����״̬����������ʵ��
        private FsmContext fsm_context;
        // ���뻺���������ڱ�����˵��ַ�
        private int input_buffer;
        // ��ǰ��ȡ���ַ�
        private int input_char;
        // �����ı��Ķ�ȡ��
        private TextReader reader;
        // ��ǰ������״̬���
        private int state;
        // �ַ���������������ƴ���ַ���
        private StringBuilder string_buffer;
        // ��ǰ���������ַ���ֵ
        private string string_value;
        // ��ǰ�������� token ����
        private int token;
        // ���ڴ��� Unicode �ַ�����ʱ����
        private int unichar;
        #endregion

        #region Properties
        /// <summary>
        /// ��ȡ�������Ƿ�����ע�͡�
        /// </summary>
        public bool AllowComments
        {
            get { return allow_comments; }
            set { allow_comments = value; }
        }

        /// <summary>
        /// ��ȡ�������Ƿ����������ַ�����
        /// </summary>
        public bool AllowSingleQuotedStrings
        {
            get { return allow_single_quoted_strings; }
            set { allow_single_quoted_strings = value; }
        }

        /// <summary>
        /// ��ȡ�Ƿ񵽴������ĩβ��
        /// </summary>
        public bool EndOfInput
        {
            get { return end_of_input; }
        }

        /// <summary>
        /// ��ȡ��ǰ�������� token ���͡�
        /// </summary>
        public int Token
        {
            get { return token; }
        }

        /// <summary>
        /// ��ȡ��ǰ���������ַ���ֵ��
        /// </summary>
        public string StringValue
        {
            get { return string_value; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// ��̬���캯������ʼ������״̬���Ĵ�������ͷ��ر�
        /// </summary>
        static Lexer()
        {
            PopulateFsmTables(out fsm_handler_table, out fsm_return_table);
        }

        /// <summary>
        /// ���캯������ʼ���ʷ���������������ԡ�
        /// </summary>
        /// <param name="reader">�����ı��Ķ�ȡ����</param>
        public Lexer(TextReader reader)
        {
            // Ĭ������ע��
            allow_comments = true;
            // Ĭ�����������ַ���
            allow_single_quoted_strings = true;

            // ������뻺����
            input_buffer = 0;
            // ��ʼ���ַ�������������ʼ����Ϊ 128
            string_buffer = new StringBuilder(128);
            // ��ʼ״̬Ϊ 1
            state = 1;
            // ��ʼδ��������ĩβ
            end_of_input = false;
            // ���������ı��Ķ�ȡ��
            this.reader = reader;

            // ��ʼ������״̬����������
            fsm_context = new FsmContext();
            fsm_context.L = this;
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// ��ȡʮ�������ַ���Ӧ��ʮ����ֵ��
        /// </summary>
        /// <param name="digit">ʮ�������ַ���</param>
        /// <returns>��Ӧ��ʮ����ֵ��</returns>
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
        /// �������״̬���Ĵ�������ͷ��ر�
        /// </summary>
        /// <param name="fsm_handler_table">��������</param>
        /// <param name="fsm_return_table">���ر�</param>
        private static void PopulateFsmTables(out StateHandler[] fsm_handler_table, out int[] fsm_return_table)
        {
            // �ο��ֲ� A.1 ���˽�����״̬������ϸ��Ϣ
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
        /// ����ת���ַ�����ת���ַ�ת��Ϊ��Ӧ��ʵ���ַ���
        /// </summary>
        /// <param name="esc_char">ת���ַ���</param>
        /// <returns>ת�����ʵ���ַ���</returns>
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
                    // �����ϲ��ᵽ������
                    return '?';
            }
        }

        /// <summary>
        /// ״̬ 1 �Ĵ������������������ʼ״̬��
        /// </summary>
        /// <param name="ctx">����״̬���������ġ�</param>
        /// <returns>�Ƿ���ɹ���</returns>
        private static bool State1(FsmContext ctx)
        {
            // ѭ����ȡ�ַ�
            while (ctx.L.GetChar())
            {
                // ���Կհ��ַ�
                if (ctx.L.input_char == ' ' ||
                    ctx.L.input_char >= '\t' && ctx.L.input_char <= '\r')
                    continue;

                // �������ֿ�ͷ�����
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
        /// ״̬ 2 �Ĵ��������������������
        /// </summary>
        /// <param name="ctx">����״̬���������ġ�</param>
        /// <returns>�Ƿ���ɹ���</returns>
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
        /// ״̬ 3 �Ĵ������������������ֵ����֡�
        /// </summary>
        /// <param name="ctx">����״̬���������ġ�</param>
        /// <returns>�Ƿ���ɹ���</returns>
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
        /// ״̬ 4 �Ĵ������������� 0 ��ͷ�����֡�
        /// </summary>
        /// <param name="ctx">����״̬���������ġ�</param>
        /// <returns>�Ƿ���ɹ���</returns>
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
        /// ״̬ 5 �Ĵ�����������С���������֡�
        /// </summary>
        /// <param name="ctx">����״̬���������ġ�</param>
        /// <returns>�Ƿ���ɹ���</returns>
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
        /// ״̬ 6 �Ĵ���������������С���������֡�
        /// </summary>
        /// <param name="ctx">����״̬���������ġ�</param>
        /// <returns>�Ƿ���ɹ���</returns>
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
        /// ״̬ 7 �Ĵ������������ѧ��������ָ�����֡�
        /// </summary>
        /// <param name="ctx">����״̬���������ġ�</param>
        /// <returns>�Ƿ���ɹ���</returns>
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
        /// ״̬ 8 �Ĵ����������������ѧ��������ָ���������֡�
        /// </summary>
        /// <param name="ctx">����״̬���������ġ�</param>
        /// <returns>�Ƿ���ɹ���</returns>
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
        /// ״̬ 9 �Ĵ���������ʼ���� "true" ����ֵ��
        /// </summary>
        /// <param name="ctx">����״̬���������ġ�</param>
        /// <returns>�Ƿ���ɹ���</returns>
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
        /// ״̬ 10 �Ĵ��������������� "true" ����ֵ��
        /// </summary>
        /// <param name="ctx">����״̬���������ġ�</param>
        /// <returns>�Ƿ���ɹ���</returns>
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
        /// ״̬ 11 �Ĵ���������ɴ��� "true" ����ֵ��
        /// </summary>
        /// <param name="ctx">����״̬���������ġ�</param>
        /// <returns>�Ƿ���ɹ���</returns>
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
        /// ״̬ 12 �Ĵ���������ʼ���� "false" ����ֵ��
        /// </summary>
        /// <param name="ctx">����״̬���������ġ�</param>
        /// <returns>�Ƿ���ɹ���</returns>
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
        /// ״̬ 13 �Ĵ��������������� "false" ����ֵ��
        /// </summary>
        /// <param name="ctx">����״̬���������ġ�</param>
        /// <returns>�Ƿ���ɹ���</returns>
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
        /// ״̬ 14 �Ĵ��������������� "false" ����ֵ��
        /// </summary>
        /// <param name="ctx">����״̬���������ġ�</param>
        /// <returns>�Ƿ���ɹ���</returns>
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
        /// ״̬ 15 �Ĵ���������ɴ��� "false" ����ֵ��
        /// </summary>
        /// <param name="ctx">����״̬���������ġ�</param>
        /// <returns>�Ƿ���ɹ���</returns>
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
        /// ״̬ 16 �Ĵ���������ʼ���� "null" ֵ��
        /// </summary>
        /// <param name="ctx">����״̬���������ġ�</param>
        /// <returns>�Ƿ���ɹ���</returns>
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
        /// ״̬ 17 �Ĵ��������������� "null" ֵ��
        /// </summary>
        /// <param name="ctx">����״̬���������ġ�</param>
        /// <returns>�Ƿ���ɹ���</returns>
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
        /// ״̬ 18 �Ĵ���������ɴ��� "null" ֵ��
        /// </summary>
        /// <param name="ctx">����״̬���������ġ�</param>
        /// <returns>�Ƿ���ɹ���</returns>
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
        /// ״̬ 19 �Ĵ���������ʼ����˫�����ַ�����
        /// </summary>
        /// <param name="ctx">����״̬���������ġ�</param>
        /// <returns>�Ƿ���ɹ���</returns>
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
        /// ״̬ 20 �Ĵ���������������˫�����ַ�����
        /// </summary>
        /// <param name="ctx">����״̬���������ġ�</param>
        /// <returns>�Ƿ���ɹ���</returns>
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
        /// ״̬ 21 �Ĵ������������ַ����е�ת���ַ���
        /// </summary>
        /// <param name="ctx">����״̬���������ġ�</param>
        /// <returns>�Ƿ���ɹ���</returns>
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
        /// ״̬ 22 �Ĵ������������ַ����е� Unicode ת���ַ���
        /// </summary>
        /// <param name="ctx">����״̬���������ġ�</param>
        /// <returns>�Ƿ���ɹ���</returns>
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
        /// ״̬ 23 �Ĵ���������ʼ���������ַ��������������
        /// </summary>
        /// <param name="ctx">����״̬���������ġ�</param>
        /// <returns>�Ƿ���ɹ���</returns>
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
        /// ״̬ 24 �Ĵ��������������������ַ��������������
        /// </summary>
        /// <param name="ctx">����״̬���������ġ�</param>
        /// <returns>�Ƿ���ɹ���</returns>
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
        /// ״̬ 25 �Ĵ���������ʼ����ע�͡�
        /// </summary>
        /// <param name="ctx">����״̬���������ġ�</param>
        /// <returns>�Ƿ���ɹ���</returns>
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
        /// ״̬ 26 �Ĵ�������������ע�͡�
        /// </summary>
        /// <param name="ctx">����״̬���������ġ�</param>
        /// <returns>�Ƿ���ɹ���</returns>
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
        /// ״̬ 27 �Ĵ��������������ע�Ϳ�ʼ��Ĳ��֡�
        /// </summary>
        /// <param name="ctx">����״̬���������ġ�</param>
        /// <returns>�Ƿ���ɹ���</returns>
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
        /// ״̬ 28 �Ĵ��������������ע�ͽ������֡�
        /// </summary>
        /// <param name="ctx">����״̬���������ġ�</param>
        /// <returns>�Ƿ���ɹ���</returns>
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
        /// ��ȡ��һ���ַ��������������ĩβ���ǽ�����
        /// </summary>
        /// <returns>�Ƿ�ɹ���ȡ�ַ���</returns>
        private bool GetChar()
        {
            if ((input_char = NextChar()) != -1)
                return true;

            end_of_input = true;
            return false;
        }

        /// <summary>
        /// ��ȡ��һ���ַ������ȴ����뻺������ȡ��
        /// </summary>
        /// <returns>��ȡ���ַ����������ĩβ���� -1��</returns>
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
        /// ������һ���ʷ���Ԫ��token����
        /// </summary>
        /// <returns>�Ƿ�ɹ���������һ�� token��</returns>
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
        /// ����ǰ�ַ����˵����뻺������
        /// </summary>
        private void UngetChar()
        {
            input_buffer = input_char;
        }
    }
}