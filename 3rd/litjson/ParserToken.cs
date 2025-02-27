namespace LitJson
{
    /// <summary>
    /// ��ö�����ڱ�ʾ JSON �������������漰�ĸ����ǣ�token����
    /// ��Щ��ǿɷ�Ϊ�ʷ���������Lexer��ʶ��Ļ����ʷ���Ԫ��
    /// �Լ��﷨��������Parser��ʹ�õĹ����ǣ�
    /// �����ڽ�����׼ȷʶ��ʹ���ͬ�� JSON �ṹ��
    /// </summary>
    internal enum ParserToken
    {
        // �ʷ���������ǣ��ο��ֲ� A.1.1 �ڣ�
        /// <summary>
        /// ��ʾ�����δ������ʼ״̬����ʼֵ������ͨ�ַ���Χ��
        /// ���ڽ�������ʼ���������޷�����������
        /// </summary>
        None = System.Char.MaxValue + 1,
        /// <summary>
        /// ��ʾ JSON �е��������ͣ��������򸡵�����
        /// </summary>
        Number,
        /// <summary>
        /// ��ʾ JSON �еĲ���ֵ true��
        /// </summary>
        True,
        /// <summary>
        /// ��ʾ JSON �еĲ���ֵ false��
        /// </summary>
        False,
        /// <summary>
        /// ��ʾ JSON �е� null ֵ��
        /// </summary>
        Null,
        /// <summary>
        /// ��ʾһ���ַ����У����� JSON �ַ����е�һ���֡�
        /// </summary>
        CharSeq,
        /// <summary>
        /// ��ʾ�����ַ����������ڴ��� JSON �е������ַ���ָ�����
        /// </summary>
        Char,

        // �﷨�����������ǣ��ο��ֲ� A.2.1 �ڣ�
        /// <summary>
        /// ��ʾ���� JSON �ı������﷨��������ʼ�㡣
        /// </summary>
        Text,
        /// <summary>
        /// ��ʾ JSON ���󣬼��ɼ�ֵ����ɣ�ʹ�û����� {} �����Ľṹ��
        /// </summary>
        Object,
        /// <summary>
        /// �������ڴ��� JSON ����ĵݹ������������֣���Ƕ�׶���
        /// </summary>
        ObjectPrime,
        /// <summary>
        /// ��ʾ JSON �����е�һ����ֵ�ԣ��ɼ���ֵͨ��ð�ŷָ���
        /// </summary>
        Pair,
        /// <summary>
        /// ���ڴ��� JSON �����ж����ֵ�Ե��������������ļ�ֵ�ԡ�
        /// </summary>
        PairRest,
        /// <summary>
        /// ��ʾ JSON ���飬���ɶ��ֵ��ɣ�ʹ�÷����� [] �����������б�
        /// </summary>
        Array,
        /// <summary>
        /// �������ڴ��� JSON ����ĵݹ������������֣���Ƕ�����顣
        /// </summary>
        ArrayPrime,
        /// <summary>
        /// ��ʾ JSON �е�һ��ֵ�����������֡��ַ���������ֵ����������� null��
        /// </summary>
        Value,
        /// <summary>
        /// ���ڴ��� JSON ���������ж��ֵ�������
        /// </summary>
        ValueRest,
        /// <summary>
        /// ��ʾ JSON �е��ַ������͡�
        /// </summary>
        String,

        // ����������
        /// <summary>
        /// ��ʾ����Ľ�����ָʾ�������Ѵ��������� JSON ���ݡ�
        /// </summary>
        End,

        // �չ�����
        /// <summary>
        /// ����ʽ���������б�ʾ�չ������ڴ��������������յĶ��� {} ������ []��
        /// </summary>
        Epsilon
    }
}