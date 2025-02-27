namespace LitJson
{
    /// <summary>
    /// 此枚举用于表示 JSON 解析过程中所涉及的各类标记（token），
    /// 这些标记可分为词法分析器（Lexer）识别的基本词法单元，
    /// 以及语法分析器（Parser）使用的规则标记，
    /// 有助于解析器准确识别和处理不同的 JSON 结构。
    /// </summary>
    internal enum ParserToken
    {
        // 词法分析器标记（参考手册 A.1.1 节）
        /// <summary>
        /// 表示特殊的未定义或初始状态，初始值超出普通字符范围，
        /// 用于解析器初始化或遇到无法归类的情况。
        /// </summary>
        None = System.Char.MaxValue + 1,
        /// <summary>
        /// 表示 JSON 中的数字类型，如整数或浮点数。
        /// </summary>
        Number,
        /// <summary>
        /// 表示 JSON 中的布尔值 true。
        /// </summary>
        True,
        /// <summary>
        /// 表示 JSON 中的布尔值 false。
        /// </summary>
        False,
        /// <summary>
        /// 表示 JSON 中的 null 值。
        /// </summary>
        Null,
        /// <summary>
        /// 表示一个字符序列，例如 JSON 字符串中的一部分。
        /// </summary>
        CharSeq,
        /// <summary>
        /// 表示单个字符，可能用于处理 JSON 中的特殊字符或分隔符。
        /// </summary>
        Char,

        // 语法分析器规则标记（参考手册 A.2.1 节）
        /// <summary>
        /// 表示整个 JSON 文本，是语法分析的起始点。
        /// </summary>
        Text,
        /// <summary>
        /// 表示 JSON 对象，即由键值对组成，使用花括号 {} 包裹的结构。
        /// </summary>
        Object,
        /// <summary>
        /// 可能用于处理 JSON 对象的递归情况或后续部分，如嵌套对象。
        /// </summary>
        ObjectPrime,
        /// <summary>
        /// 表示 JSON 对象中的一个键值对，由键和值通过冒号分隔。
        /// </summary>
        Pair,
        /// <summary>
        /// 用于处理 JSON 对象中多个键值对的情况，处理后续的键值对。
        /// </summary>
        PairRest,
        /// <summary>
        /// 表示 JSON 数组，即由多个值组成，使用方括号 [] 包裹的有序列表。
        /// </summary>
        Array,
        /// <summary>
        /// 可能用于处理 JSON 数组的递归情况或后续部分，如嵌套数组。
        /// </summary>
        ArrayPrime,
        /// <summary>
        /// 表示 JSON 中的一个值，可以是数字、字符串、布尔值、对象、数组或 null。
        /// </summary>
        Value,
        /// <summary>
        /// 用于处理 JSON 数组或对象中多个值的情况。
        /// </summary>
        ValueRest,
        /// <summary>
        /// 表示 JSON 中的字符串类型。
        /// </summary>
        String,

        // 输入结束标记
        /// <summary>
        /// 表示输入的结束，指示解析器已处理完整个 JSON 数据。
        /// </summary>
        End,

        // 空规则标记
        /// <summary>
        /// 在形式语言理论中表示空规则，用于处理特殊情况，如空的对象 {} 或数组 []。
        /// </summary>
        Epsilon
    }
}