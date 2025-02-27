using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace LitJson
{
    // �ڲ�ö�٣�����д������еĸ�������
    internal enum Condition
    {
        InArray,        // ��������
        InObject,       // �ڶ�����
        NotAProperty,   // ���Ƕ�������
        Property,       // ��������
        Value           // ֵ
    }

    // �ڲ��࣬���ڴ洢д����������Ϣ
    internal class WriterContext
    {
        public int Count;           // ��ǰ�������е�Ԫ������
        public bool InArray;         // �Ƿ���������
        public bool InObject;        // �Ƿ��ڶ�����
        public bool ExpectingValue;  // �Ƿ��ڴ�һ��ֵ
        public int Padding;         // ���ڸ�ʽ���������䳤��
    }

    // �����࣬���ڽ������� JSON ��ʽд���ı���
    public class JsonWriter
    {
        #region Fields
        // ��ֻ̬�������ָ�ʽ��Ϣ��ʹ�ò����Ļ���Ϣ
        private static readonly NumberFormatInfo number_format;

        private WriterContext context;         // ��ǰд��������
        private Stack<WriterContext> ctx_stack;       // ������ջ������Ƕ�׽ṹ
        private bool has_reached_end; // �Ƿ��Ѿ�����д�����״̬
        private char[] hex_seq;         // ���ڴ洢ʮ�������ַ�����
        private int indentation;     // ��ǰ������
        private int indent_value;    // ÿ�������Ŀո���
        private StringBuilder inst_string_builder; // �ڲ�ʹ�õ��ַ���������
        private bool pretty_print;    // �Ƿ���и�ʽ�����
        private bool validate;        // �Ƿ����������֤
        private bool lower_case_properties; // �Ƿ�������ת��ΪСд
        private TextWriter writer;          // ����д�����ݵ��ı�д����
        #endregion

        #region Properties
        // ��ȡ������ÿ�������Ŀո���
        public int IndentValue
        {
            get { return indent_value; }
            set
            {
                indentation = (indentation / indent_value) * value;
                indent_value = value;
            }
        }

        // ��ȡ�������Ƿ���и�ʽ�����
        public bool PrettyPrint
        {
            get { return pretty_print; }
            set { pretty_print = value; }
        }

        // ��ȡ��ǰʹ�õ��ı�д����
        public TextWriter TextWriter
        {
            get { return writer; }
        }

        // ��ȡ�������Ƿ����������֤
        public bool Validate
        {
            get { return validate; }
            set { validate = value; }
        }

        // ��ȡ�������Ƿ�������ת��ΪСд
        public bool LowerCaseProperties
        {
            get { return lower_case_properties; }
            set { lower_case_properties = value; }
        }
        #endregion

        #region Constructors
        // ��̬���캯������ʼ�����ָ�ʽ��Ϣ
        static JsonWriter()
        {
            number_format = NumberFormatInfo.InvariantInfo;
        }

        // �޲ι��캯����ʹ���ڲ����ַ���������
        public JsonWriter()
        {
            inst_string_builder = new StringBuilder();
            writer = new StringWriter(inst_string_builder);

            Init();
        }

        // ���� StringBuilder �����Ĺ��캯��
        public JsonWriter(StringBuilder sb) :
            this(new StringWriter(sb))
        {
        }

        // ���� TextWriter �����Ĺ��캯��
        public JsonWriter(TextWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            this.writer = writer;

            Init();
        }
        #endregion

        #region Private Methods
        // ����������֤��˽�з���
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

        // ��ʼ��д������˽�з���
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

        // ������ת��Ϊʮ�������ַ����е�˽�з���
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

        // ������������˽�з���
        private void Indent()
        {
            if (pretty_print)
                indentation += indent_value;
        }

        // ���ı�д����д���ַ�����˽�з���������ݸ�ʽ��ѡ���������
        private void Put(string str)
        {
            if (pretty_print && !context.ExpectingValue)
                for (int i = 0; i < indentation; i++)
                    writer.Write(' ');

            writer.Write(str);
        }

        // д�뻻�з���˽�з�������ѡ���Ƿ���Ӷ���
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

        // д���ַ�����˽�з������ᴦ�������ַ�
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

                // Ĭ����������ַ�ת��Ϊ \uXXXX ����
                IntToHex((int)str[i], hex_seq);
                writer.Write("\\u");
                writer.Write(hex_seq);
            }

            writer.Write('"');
        }

        // ������������˽�з���
        private void Unindent()
        {
            if (pretty_print)
                indentation -= indent_value;
        }
        #endregion

        // ��д ToString �����������ڲ��ַ���������������
        public override string ToString()
        {
            if (inst_string_builder == null)
                return String.Empty;

            return inst_string_builder.ToString();
        }

        // ����д����״̬�Ĺ�������
        public void Reset()
        {
            has_reached_end = false;

            ctx_stack.Clear();
            context = new WriterContext();
            ctx_stack.Push(context);

            if (inst_string_builder != null)
                inst_string_builder.Remove(0, inst_string_builder.Length);
        }

        // д�벼��ֵ�Ĺ�������
        public void Write(bool boolean)
        {
            DoValidation(Condition.Value);
            PutNewline();

            Put(boolean ? "true" : "false");

            context.ExpectingValue = false;
        }

        // д��ʮ�������Ĺ�������
        public void Write(decimal number)
        {
            DoValidation(Condition.Value);
            PutNewline();

            Put(Convert.ToString(number, number_format));

            context.ExpectingValue = false;
        }

        // д��˫���ȸ������Ĺ�������
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

        // д�뵥���ȸ������Ĺ�������
        public void Write(float number)
        {
            DoValidation(Condition.Value);
            PutNewline();

            string str = Convert.ToString(number, number_format);
            Put(str);

            context.ExpectingValue = false;
        }

        // д�������Ĺ�������
        public void Write(int number)
        {
            DoValidation(Condition.Value);
            PutNewline();

            Put(Convert.ToString(number, number_format));

            context.ExpectingValue = false;
        }

        // д�볤�����Ĺ�������
        public void Write(long number)
        {
            DoValidation(Condition.Value);
            PutNewline();

            Put(Convert.ToString(number, number_format));

            context.ExpectingValue = false;
        }

        // д���ַ����Ĺ�������
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

        // д���޷��ų������Ĺ�������
        [CLSCompliant(false)]
        public void Write(ulong number)
        {
            DoValidation(Condition.Value);
            PutNewline();

            Put(Convert.ToString(number, number_format));

            context.ExpectingValue = false;
        }

        // д������������ŵĹ�������
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

        // д�����鿪ʼ���ŵĹ�������
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

        // д�����������ŵĹ�������
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

        // д�����ʼ���ŵĹ�������
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

        // д������������Ĺ�������
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