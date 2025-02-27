using System;
using System.Collections;
using System.Collections.Specialized;

namespace LitJson
{
    /// <summary>
    /// JsonMockWrapper 类是一个模拟对象，实现了 IJsonWrapper 接口。
    /// 其主要用途是为了更高效地执行诸如跳过 JSON 数据等操作，
    /// 当不需要处理具体的 JSON 数据时，可以使用该模拟对象来占位。
    /// </summary>
    public class JsonMockWrapper : IJsonWrapper
    {
        /// <summary>
        /// 指示该模拟对象是否表示一个 JSON 数组，始终返回 false。
        /// </summary>
        public bool IsArray { get { return false; } }

        /// <summary>
        /// 指示该模拟对象是否表示一个布尔类型的 JSON 值，始终返回 false。
        /// </summary>
        public bool IsBoolean { get { return false; } }

        /// <summary>
        /// 指示该模拟对象是否表示一个双精度浮点数类型的 JSON 值，始终返回 false。
        /// </summary>
        public bool IsDouble { get { return false; } }

        /// <summary>
        /// 指示该模拟对象是否表示一个整数类型的 JSON 值，始终返回 false。
        /// </summary>
        public bool IsInt { get { return false; } }

        /// <summary>
        /// 指示该模拟对象是否表示一个长整数类型的 JSON 值，始终返回 false。
        /// </summary>
        public bool IsLong { get { return false; } }

        /// <summary>
        /// 指示该模拟对象是否表示一个 JSON 对象，始终返回 false。
        /// </summary>
        public bool IsObject { get { return false; } }

        /// <summary>
        /// 指示该模拟对象是否表示一个字符串类型的 JSON 值，始终返回 false。
        /// </summary>
        public bool IsString { get { return false; } }

        /// <summary>
        /// 获取该模拟对象表示的布尔值，始终返回 false。
        /// </summary>
        /// <returns>布尔值 false。</returns>
        public bool GetBoolean() { return false; }

        /// <summary>
        /// 获取该模拟对象表示的双精度浮点数，始终返回 0.0。
        /// </summary>
        /// <returns>双精度浮点数 0.0。</returns>
        public double GetDouble() { return 0.0; }

        /// <summary>
        /// 获取该模拟对象表示的整数，始终返回 0。
        /// </summary>
        /// <returns>整数 0。</returns>
        public int GetInt() { return 0; }

        /// <summary>
        /// 获取该模拟对象的 JSON 类型，始终返回 JsonType.None。
        /// </summary>
        /// <returns>JsonType.None。</returns>
        public JsonType GetJsonType() { return JsonType.None; }

        /// <summary>
        /// 获取该模拟对象表示的长整数，始终返回 0L。
        /// </summary>
        /// <returns>长整数 0L。</returns>
        public long GetLong() { return 0L; }

        /// <summary>
        /// 获取该模拟对象表示的字符串，始终返回空字符串。
        /// </summary>
        /// <returns>空字符串。</returns>
        public string GetString() { return ""; }

        /// <summary>
        /// 设置该模拟对象的布尔值，此方法为空实现，不执行任何操作。
        /// </summary>
        /// <param name="val">要设置的布尔值。</param>
        public void SetBoolean(bool val) { }

        /// <summary>
        /// 设置该模拟对象的双精度浮点数，此方法为空实现，不执行任何操作。
        /// </summary>
        /// <param name="val">要设置的双精度浮点数。</param>
        public void SetDouble(double val) { }

        /// <summary>
        /// 设置该模拟对象的整数，此方法为空实现，不执行任何操作。
        /// </summary>
        /// <param name="val">要设置的整数。</param>
        public void SetInt(int val) { }

        /// <summary>
        /// 设置该模拟对象的 JSON 类型，此方法为空实现，不执行任何操作。
        /// </summary>
        /// <param name="type">要设置的 JSON 类型。</param>
        public void SetJsonType(JsonType type) { }

        /// <summary>
        /// 设置该模拟对象的长整数，此方法为空实现，不执行任何操作。
        /// </summary>
        /// <param name="val">要设置的长整数。</param>
        public void SetLong(long val) { }

        /// <summary>
        /// 设置该模拟对象的字符串，此方法为空实现，不执行任何操作。
        /// </summary>
        /// <param name="val">要设置的字符串。</param>
        public void SetString(string val) { }

        /// <summary>
        /// 将该模拟对象转换为 JSON 字符串，始终返回空字符串。
        /// </summary>
        /// <returns>空字符串。</returns>
        public string ToJson() { return ""; }

        /// <summary>
        /// 使用指定的 JsonWriter 将该模拟对象转换为 JSON 数据，此方法为空实现，不执行任何操作。
        /// </summary>
        /// <param name="writer">用于写入 JSON 数据的 JsonWriter。</param>
        public void ToJson(JsonWriter writer) { }


        #region IList 接口实现

        /// <summary>
        /// 指示该模拟列表的大小是否固定，始终返回 true。
        /// </summary>
        bool IList.IsFixedSize { get { return true; } }

        /// <summary>
        /// 指示该模拟列表是否为只读，始终返回 true。
        /// </summary>
        bool IList.IsReadOnly { get { return true; } }

        /// <summary>
        /// 获取或设置模拟列表中指定索引处的元素。
        /// get 方法始终返回 null，set 方法为空实现，不执行任何操作。
        /// </summary>
        /// <param name="index">要获取或设置元素的索引。</param>
        /// <returns>始终返回 null。</returns>
        object IList.this[int index]
        {
            get { return null; }
            set { }
        }

        /// <summary>
        /// 将一个对象添加到模拟列表中，始终返回 0，表示不添加元素。
        /// </summary>
        /// <param name="value">要添加的对象。</param>
        /// <returns>始终返回 0。</returns>
        int IList.Add(object value) { return 0; }

        /// <summary>
        /// 清空模拟列表，此方法为空实现，不执行任何操作。
        /// </summary>
        void IList.Clear() { }

        /// <summary>
        /// 检查模拟列表中是否包含指定的对象，始终返回 false。
        /// </summary>
        /// <param name="value">要检查的对象。</param>
        /// <returns>始终返回 false。</returns>
        bool IList.Contains(object value) { return false; }

        /// <summary>
        /// 获取指定对象在模拟列表中的索引，始终返回 -1，表示元素不存在。
        /// </summary>
        /// <param name="value">要查找的对象。</param>
        /// <returns>始终返回 -1。</returns>
        int IList.IndexOf(object value) { return -1; }

        /// <summary>
        /// 在模拟列表的指定索引处插入一个对象，此方法为空实现，不执行任何操作。
        /// </summary>
        /// <param name="i">插入位置的索引。</param>
        /// <param name="v">要插入的对象。</param>
        void IList.Insert(int i, object v) { }

        /// <summary>
        /// 从模拟列表中移除指定的对象，此方法为空实现，不执行任何操作。
        /// </summary>
        /// <param name="value">要移除的对象。</param>
        void IList.Remove(object value) { }

        /// <summary>
        /// 移除模拟列表中指定索引处的元素，此方法为空实现，不执行任何操作。
        /// </summary>
        /// <param name="index">要移除元素的索引。</param>
        void IList.RemoveAt(int index) { }

        #endregion

        #region ICollection 接口实现

        /// <summary>
        /// 获取模拟集合中元素的数量，始终返回 0。
        /// </summary>
        int ICollection.Count { get { return 0; } }

        /// <summary>
        /// 指示模拟集合是否是线程安全的，始终返回 false。
        /// </summary>
        bool ICollection.IsSynchronized { get { return false; } }

        /// <summary>
        /// 获取可用于同步对模拟集合的访问的对象，始终返回 null。
        /// </summary>
        object ICollection.SyncRoot { get { return null; } }

        /// <summary>
        /// 将模拟集合的元素复制到指定数组的指定索引位置，此方法为空实现，不执行任何操作。
        /// </summary>
        /// <param name="array">要复制到的数组。</param>
        /// <param name="index">开始复制的索引位置。</param>
        void ICollection.CopyTo(Array array, int index) { }

        #endregion

        #region IEnumerable 接口实现

        /// <summary>
        /// 返回一个用于遍历模拟集合的枚举器，始终返回 null。
        /// </summary>
        /// <returns>始终返回 null。</returns>
        IEnumerator IEnumerable.GetEnumerator() { return null; }

        #endregion

        #region IDictionary 接口实现

        /// <summary>
        /// 指示模拟字典的大小是否固定，始终返回 true。
        /// </summary>
        bool IDictionary.IsFixedSize { get { return true; } }

        /// <summary>
        /// 指示模拟字典是否为只读，始终返回 true。
        /// </summary>
        bool IDictionary.IsReadOnly { get { return true; } }

        /// <summary>
        /// 获取模拟字典中的所有键，始终返回 null。
        /// </summary>
        ICollection IDictionary.Keys { get { return null; } }

        /// <summary>
        /// 获取模拟字典中的所有值，始终返回 null。
        /// </summary>
        ICollection IDictionary.Values { get { return null; } }

        /// <summary>
        /// 获取或设置模拟字典中与指定键关联的值。
        /// get 方法始终返回 null，set 方法为空实现，不执行任何操作。
        /// </summary>
        /// <param name="key">要获取或设置值的键。</param>
        /// <returns>始终返回 null。</returns>
        object IDictionary.this[object key]
        {
            get { return null; }
            set { }
        }

        /// <summary>
        /// 向模拟字典中添加一个具有指定键和值的元素，此方法为空实现，不执行任何操作。
        /// </summary>
        /// <param name="k">要添加的元素的键。</param>
        /// <param name="v">要添加的元素的值。</param>
        void IDictionary.Add(object k, object v) { }

        /// <summary>
        /// 清空模拟字典，此方法为空实现，不执行任何操作。
        /// </summary>
        void IDictionary.Clear() { }

        /// <summary>
        /// 检查模拟字典中是否包含具有指定键的元素，始终返回 false。
        /// </summary>
        /// <param name="key">要检查的键。</param>
        /// <returns>始终返回 false。</returns>
        bool IDictionary.Contains(object key) { return false; }

        /// <summary>
        /// 从模拟字典中移除具有指定键的元素，此方法为空实现，不执行任何操作。
        /// </summary>
        /// <param name="key">要移除元素的键。</param>
        void IDictionary.Remove(object key) { }

        /// <summary>
        /// 返回一个用于遍历模拟字典的枚举器，始终返回 null。
        /// </summary>
        /// <returns>始终返回 null。</returns>
        IDictionaryEnumerator IDictionary.GetEnumerator() { return null; }

        #endregion

        #region IOrderedDictionary 接口实现

        /// <summary>
        /// 获取或设置模拟有序字典中指定索引处的元素。
        /// get 方法始终返回 null，set 方法为空实现，不执行任何操作。
        /// </summary>
        /// <param name="idx">要获取或设置元素的索引。</param>
        /// <returns>始终返回 null。</returns>
        object IOrderedDictionary.this[int idx]
        {
            get { return null; }
            set { }
        }

        /// <summary>
        /// 返回一个用于遍历模拟有序字典的枚举器，始终返回 null。
        /// </summary>
        /// <returns>始终返回 null。</returns>
        IDictionaryEnumerator IOrderedDictionary.GetEnumerator()
        {
            return null;
        }

        /// <summary>
        /// 在模拟有序字典的指定索引处插入一个具有指定键和值的元素，此方法为空实现，不执行任何操作。
        /// </summary>
        /// <param name="i">插入位置的索引。</param>
        /// <param name="k">要插入元素的键。</param>
        /// <param name="v">要插入元素的值。</param>
        void IOrderedDictionary.Insert(int i, object k, object v) { }

        /// <summary>
        /// 移除模拟有序字典中指定索引处的元素，此方法为空实现，不执行任何操作。
        /// </summary>
        /// <param name="i">要移除元素的索引。</param>
        void IOrderedDictionary.RemoveAt(int i) { }

        #endregion
    }
}