// 引入 System.Collections 命名空间，该命名空间提供了一些基本的集合接口和类，
// 例如 IList 接口，后续 IJsonWrapper 会继承该接口以具备列表相关操作能力
using System.Collections;
// 引入 System.Collections.Specialized 命名空间，它包含了一些特殊用途的集合类和接口，
// 比如 IOrderedDictionary 接口，后续 IJsonWrapper 会继承该接口以实现有序字典的功能
using System.Collections.Specialized;

// 定义一个名为 LitJson 的命名空间，用于将相关的 JSON 处理类型和功能组织在一起，
// 避免不同代码模块之间的命名冲突
namespace LitJson
{
    // 定义一个公共枚举类型 JsonType，用于表示各种可能的 JSON 数据类型
    public enum JsonType
    {
        // 表示没有特定的 JSON 数据类型，可用于初始化或表示空状态
        None,

        // 表示 JSON 对象，JSON 对象是由键值对组成的数据结构，通常用花括号 {} 包裹
        Object,
        // 表示 JSON 数组，JSON 数组是一组有序的值，通常用方括号 [] 包裹
        Array,
        // 表示 JSON 字符串，通常用双引号 "" 包裹
        String,
        // 表示 JSON 中的整数类型
        Int,
        // 表示 JSON 中的长整数类型
        Long,
        // 表示 JSON 中的双精度浮点数类型
        Double,
        // 表示 JSON 中的布尔类型，值为 true 或 false
        Boolean
    }

    // 定义一个公共接口 IJsonWrapper，它继承自 IList 和 IOrderedDictionary 接口，
    // 意味着实现该接口的类将同时具备列表和有序字典的操作能力，用于处理各种 JSON 数据
    public interface IJsonWrapper : IList, IOrderedDictionary
    {
        // 只读属性，用于判断当前 IJsonWrapper 实例所表示的 JSON 数据是否为数组类型
        bool IsArray { get; }
        // 只读属性，用于判断当前 IJsonWrapper 实例所表示的 JSON 数据是否为布尔类型
        bool IsBoolean { get; }
        // 只读属性，用于判断当前 IJsonWrapper 实例所表示的 JSON 数据是否为双精度浮点数类型
        bool IsDouble { get; }
        // 只读属性，用于判断当前 IJsonWrapper 实例所表示的 JSON 数据是否为整数类型
        bool IsInt { get; }
        // 只读属性，用于判断当前 IJsonWrapper 实例所表示的 JSON 数据是否为长整数类型
        bool IsLong { get; }
        // 只读属性，用于判断当前 IJsonWrapper 实例所表示的 JSON 数据是否为对象类型
        bool IsObject { get; }
        // 只读属性，用于判断当前 IJsonWrapper 实例所表示的 JSON 数据是否为字符串类型
        bool IsString { get; }

        // 方法，用于获取当前 IJsonWrapper 实例所表示的 JSON 数据的布尔值
        bool GetBoolean();
        // 方法，用于获取当前 IJsonWrapper 实例所表示的 JSON 数据的双精度浮点数值
        double GetDouble();
        // 方法，用于获取当前 IJsonWrapper 实例所表示的 JSON 数据的整数值
        int GetInt();
        // 方法，用于获取当前 IJsonWrapper 实例所表示的 JSON 数据的类型，返回值为 JsonType 枚举类型
        JsonType GetJsonType();
        // 方法，用于获取当前 IJsonWrapper 实例所表示的 JSON 数据的长整数值
        long GetLong();
        // 方法，用于获取当前 IJsonWrapper 实例所表示的 JSON 数据的字符串值
        string GetString();

        // 方法，用于将当前 IJsonWrapper 实例所表示的 JSON 数据设置为指定的布尔值
        void SetBoolean(bool val);
        // 方法，用于将当前 IJsonWrapper 实例所表示的 JSON 数据设置为指定的双精度浮点数值
        void SetDouble(double val);
        // 方法，用于将当前 IJsonWrapper 实例所表示的 JSON 数据设置为指定的整数值
        void SetInt(int val);
        // 方法，用于将当前 IJsonWrapper 实例所表示的 JSON 数据的类型设置为指定的 JsonType 枚举值
        void SetJsonType(JsonType type);
        // 方法，用于将当前 IJsonWrapper 实例所表示的 JSON 数据设置为指定的长整数值
        void SetLong(long val);
        // 方法，用于将当前 IJsonWrapper 实例所表示的 JSON 数据设置为指定的字符串值
        void SetString(string val);

        // 方法，将当前 IJsonWrapper 实例所表示的 JSON 数据序列化为 JSON 字符串并返回
        string ToJson();
        // 方法，将当前 IJsonWrapper 实例所表示的 JSON 数据通过指定的 JsonWriter 对象进行序列化
        void ToJson(JsonWriter writer);
    }
}