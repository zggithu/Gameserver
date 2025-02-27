using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

// 定义 LitJson 命名空间，用于组织处理 JSON 数据的相关类和接口
namespace LitJson
{
    // JsonData 类实现了 IJsonWrapper 接口和 IEquatable<JsonData> 接口
    // IJsonWrapper 接口提供了处理各种 JSON 数据类型的方法
    // IEquatable<JsonData> 接口用于比较两个 JsonData 实例是否相等
    public class JsonData : IJsonWrapper, IEquatable<JsonData>
    {
        #region Fields
        // 存储 JSON 数组的数据
        private IList<JsonData> inst_array;
        // 存储 JSON 布尔类型的数据
        private bool inst_boolean;
        // 存储 JSON 双精度浮点数类型的数据
        private double inst_double;
        // 存储 JSON 整数类型的数据
        private int inst_int;
        // 存储 JSON 长整数类型的数据
        private long inst_long;
        // 存储 JSON 对象的数据，使用字典存储键值对
        private IDictionary<string, JsonData> inst_object;
        // 存储 JSON 字符串类型的数据
        private string inst_string;
        // 存储序列化后的 JSON 字符串
        private string json;
        // 表示当前 JsonData 实例的 JSON 数据类型
        private JsonType type;

        // 用于实现 IOrderedDictionary 接口，存储对象的键值对列表，保证顺序
        private IList<KeyValuePair<string, JsonData>> object_list;
        #endregion


        #region Properties
        // 获取当前 JsonData 实例作为集合时的元素数量
        public int Count
        {
            get { return EnsureCollection().Count; }
        }

        // 判断当前 JsonData 实例是否为 JSON 数组类型
        public bool IsArray
        {
            get { return type == JsonType.Array; }
        }

        // 判断当前 JsonData 实例是否为 JSON 布尔类型
        public bool IsBoolean
        {
            get { return type == JsonType.Boolean; }
        }

        // 判断当前 JsonData 实例是否为 JSON 双精度浮点数类型
        public bool IsDouble
        {
            get { return type == JsonType.Double; }
        }

        // 判断当前 JsonData 实例是否为 JSON 整数类型
        public bool IsInt
        {
            get { return type == JsonType.Int; }
        }

        // 判断当前 JsonData 实例是否为 JSON 长整数类型
        public bool IsLong
        {
            get { return type == JsonType.Long; }
        }

        // 判断当前 JsonData 实例是否为 JSON 对象类型
        public bool IsObject
        {
            get { return type == JsonType.Object; }
        }

        // 判断当前 JsonData 实例是否为 JSON 字符串类型
        public bool IsString
        {
            get { return type == JsonType.String; }
        }

        // 获取当前 JsonData 实例作为对象时的所有键的集合
        public ICollection<string> Keys
        {
            get { EnsureDictionary(); return inst_object.Keys; }
        }

        /// <summary>
        /// Determines whether the json contains an element that has the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the json.</param>
        /// <returns>true if the json contains an element that has the specified key; otherwise, false.</returns>
        // 判断当前 JsonData 实例作为对象时是否包含指定的键
        public Boolean ContainsKey(String key)
        {
            EnsureDictionary();
            return this.inst_object.Keys.Contains(key);
        }
        #endregion


        #region ICollection Properties
        // 实现 ICollection 接口的 Count 属性，获取元素数量
        int ICollection.Count
        {
            get
            {
                return Count;
            }
        }

        // 实现 ICollection 接口的 IsSynchronized 属性，判断集合是否线程安全
        bool ICollection.IsSynchronized
        {
            get
            {
                return EnsureCollection().IsSynchronized;
            }
        }

        // 实现 ICollection 接口的 SyncRoot 属性，获取用于同步集合访问的对象
        object ICollection.SyncRoot
        {
            get
            {
                return EnsureCollection().SyncRoot;
            }
        }
        #endregion


        #region IDictionary Properties
        // 实现 IDictionary 接口的 IsFixedSize 属性，判断字典是否具有固定大小
        bool IDictionary.IsFixedSize
        {
            get
            {
                return EnsureDictionary().IsFixedSize;
            }
        }

        // 实现 IDictionary 接口的 IsReadOnly 属性，判断字典是否为只读
        bool IDictionary.IsReadOnly
        {
            get
            {
                return EnsureDictionary().IsReadOnly;
            }
        }

        // 实现 IDictionary 接口的 Keys 属性，获取字典的所有键的集合
        ICollection IDictionary.Keys
        {
            get
            {
                EnsureDictionary();
                IList<string> keys = new List<string>();

                foreach (KeyValuePair<string, JsonData> entry in
                         object_list)
                {
                    keys.Add(entry.Key);
                }

                return (ICollection)keys;
            }
        }

        // 实现 IDictionary 接口的 Values 属性，获取字典的所有值的集合
        ICollection IDictionary.Values
        {
            get
            {
                EnsureDictionary();
                IList<JsonData> values = new List<JsonData>();

                foreach (KeyValuePair<string, JsonData> entry in
                         object_list)
                {
                    values.Add(entry.Value);
                }

                return (ICollection)values;
            }
        }
        #endregion


        #region IJsonWrapper Properties
        // 实现 IJsonWrapper 接口的 IsArray 属性，判断是否为数组类型
        bool IJsonWrapper.IsArray
        {
            get { return IsArray; }
        }

        // 实现 IJsonWrapper 接口的 IsBoolean 属性，判断是否为布尔类型
        bool IJsonWrapper.IsBoolean
        {
            get { return IsBoolean; }
        }

        // 实现 IJsonWrapper 接口的 IsDouble 属性，判断是否为双精度浮点数类型
        bool IJsonWrapper.IsDouble
        {
            get { return IsDouble; }
        }

        // 实现 IJsonWrapper 接口的 IsInt 属性，判断是否为整数类型
        bool IJsonWrapper.IsInt
        {
            get { return IsInt; }
        }

        // 实现 IJsonWrapper 接口的 IsLong 属性，判断是否为长整数类型
        bool IJsonWrapper.IsLong
        {
            get { return IsLong; }
        }

        // 实现 IJsonWrapper 接口的 IsObject 属性，判断是否为对象类型
        bool IJsonWrapper.IsObject
        {
            get { return IsObject; }
        }

        // 实现 IJsonWrapper 接口的 IsString 属性，判断是否为字符串类型
        bool IJsonWrapper.IsString
        {
            get { return IsString; }
        }
        #endregion


        #region IList Properties
        // 实现 IList 接口的 IsFixedSize 属性，判断列表是否具有固定大小
        bool IList.IsFixedSize
        {
            get
            {
                return EnsureList().IsFixedSize;
            }
        }

        // 实现 IList 接口的 IsReadOnly 属性，判断列表是否为只读
        bool IList.IsReadOnly
        {
            get
            {
                return EnsureList().IsReadOnly;
            }
        }
        #endregion


        #region IDictionary Indexer
        // 实现 IDictionary 接口的索引器，通过键获取或设置字典中的值
        object IDictionary.this[object key]
        {
            get
            {
                return EnsureDictionary()[key];
            }

            set
            {
                if (!(key is String))
                    throw new ArgumentException(
                        "The key has to be a string");

                JsonData data = ToJsonData(value);

                this[(string)key] = data;
            }
        }
        #endregion


        #region IOrderedDictionary Indexer
        // 实现 IOrderedDictionary 接口的索引器，通过索引获取或设置有序字典中的值
        object IOrderedDictionary.this[int idx]
        {
            get
            {
                EnsureDictionary();
                return object_list[idx].Value;
            }

            set
            {
                EnsureDictionary();
                JsonData data = ToJsonData(value);

                KeyValuePair<string, JsonData> old_entry = object_list[idx];

                inst_object[old_entry.Key] = data;

                KeyValuePair<string, JsonData> entry =
                    new KeyValuePair<string, JsonData>(old_entry.Key, data);

                object_list[idx] = entry;
            }
        }
        #endregion


        #region IList Indexer
        // 实现 IList 接口的索引器，通过索引获取或设置列表中的值
        object IList.this[int index]
        {
            get
            {
                return EnsureList()[index];
            }

            set
            {
                EnsureList();
                JsonData data = ToJsonData(value);

                this[index] = data;
            }
        }
        #endregion


        #region Public Indexers
        // 公共索引器，通过属性名获取或设置 JsonData 实例作为对象时的属性值
        public JsonData this[string prop_name]
        {
            get
            {
                EnsureDictionary();
                return inst_object[prop_name];
            }

            set
            {
                EnsureDictionary();

                KeyValuePair<string, JsonData> entry =
                    new KeyValuePair<string, JsonData>(prop_name, value);

                if (inst_object.ContainsKey(prop_name))
                {
                    for (int i = 0; i < object_list.Count; i++)
                    {
                        if (object_list[i].Key == prop_name)
                        {
                            object_list[i] = entry;
                            break;
                        }
                    }
                }
                else
                    object_list.Add(entry);

                inst_object[prop_name] = value;

                json = null;
            }
        }

        // 公共索引器，通过索引获取或设置 JsonData 实例作为数组或对象时的值
        public JsonData this[int index]
        {
            get
            {
                EnsureCollection();

                if (type == JsonType.Array)
                    return inst_array[index];

                return object_list[index].Value;
            }

            set
            {
                EnsureCollection();

                if (type == JsonType.Array)
                    inst_array[index] = value;
                else
                {
                    KeyValuePair<string, JsonData> entry = object_list[index];
                    KeyValuePair<string, JsonData> new_entry =
                        new KeyValuePair<string, JsonData>(entry.Key, value);

                    object_list[index] = new_entry;
                    inst_object[entry.Key] = value;
                }

                json = null;
            }
        }
        #endregion


        #region Constructors
        // 默认构造函数
        public JsonData()
        {
        }

        // 构造函数，使用布尔值初始化 JsonData 实例
        public JsonData(bool boolean)
        {
            type = JsonType.Boolean;
            inst_boolean = boolean;
        }

        // 构造函数，使用双精度浮点数初始化 JsonData 实例
        public JsonData(double number)
        {
            type = JsonType.Double;
            inst_double = number;
        }

        // 构造函数，使用整数初始化 JsonData 实例
        public JsonData(int number)
        {
            type = JsonType.Int;
            inst_int = number;
        }

        // 构造函数，使用长整数初始化 JsonData 实例
        public JsonData(long number)
        {
            type = JsonType.Long;
            inst_long = number;
        }

        // 构造函数，使用对象初始化 JsonData 实例，根据对象类型设置 JsonData 类型
        public JsonData(object obj)
        {
            if (obj is Boolean)
            {
                type = JsonType.Boolean;
                inst_boolean = (bool)obj;
                return;
            }

            if (obj is Double)
            {
                type = JsonType.Double;
                inst_double = (double)obj;
                return;
            }

            if (obj is Int32)
            {
                type = JsonType.Int;
                inst_int = (int)obj;
                return;
            }

            if (obj is Int64)
            {
                type = JsonType.Long;
                inst_long = (long)obj;
                return;
            }

            if (obj is String)
            {
                type = JsonType.String;
                inst_string = (string)obj;
                return;
            }

            throw new ArgumentException(
                "Unable to wrap the given object with JsonData");
        }

        // 构造函数，使用字符串初始化 JsonData 实例
        public JsonData(string str)
        {
            type = JsonType.String;
            inst_string = str;
        }
        #endregion


        #region Implicit Conversions
        // 隐式转换，将布尔值转换为 JsonData 实例
        public static implicit operator JsonData(Boolean data)
        {
            return new JsonData(data);
        }

        // 隐式转换，将双精度浮点数转换为 JsonData 实例
        public static implicit operator JsonData(Double data)
        {
            return new JsonData(data);
        }

        // 隐式转换，将整数转换为 JsonData 实例
        public static implicit operator JsonData(Int32 data)
        {
            return new JsonData(data);
        }

        // 隐式转换，将长整数转换为 JsonData 实例
        public static implicit operator JsonData(Int64 data)
        {
            return new JsonData(data);
        }

        // 隐式转换，将字符串转换为 JsonData 实例
        public static implicit operator JsonData(String data)
        {
            return new JsonData(data);
        }
        #endregion


        #region Explicit Conversions
        // 显式转换，将 JsonData 实例转换为布尔值
        public static explicit operator Boolean(JsonData data)
        {
            if (data.type != JsonType.Boolean)
                throw new InvalidCastException(
                    "Instance of JsonData doesn't hold a double");

            return data.inst_boolean;
        }

        // 显式转换，将 JsonData 实例转换为双精度浮点数
        public static explicit operator Double(JsonData data)
        {
            if (data.type != JsonType.Double)
                throw new InvalidCastException(
                    "Instance of JsonData doesn't hold a double");

            return data.inst_double;
        }

        // 显式转换，将 JsonData 实例转换为整数
        public static explicit operator Int32(JsonData data)
        {
            if (data.type != JsonType.Int && data.type != JsonType.Long)
            {
                throw new InvalidCastException(
                    "Instance of JsonData doesn't hold an int");
            }

            // cast may truncate data... but that's up to the user to consider
            return data.type == JsonType.Int ? data.inst_int : (int)data.inst_long;
        }

        // 显式转换，将 JsonData 实例转换为长整数
        public static explicit operator Int64(JsonData data)
        {
            if (data.type != JsonType.Long && data.type != JsonType.Int)
            {
                throw new InvalidCastException(
                    "Instance of JsonData doesn't hold a long");
            }

            return data.type == JsonType.Long ? data.inst_long : data.inst_int;
        }

        // 显式转换，将 JsonData 实例转换为字符串
        public static explicit operator String(JsonData data)
        {
            if (data.type != JsonType.String)
                throw new InvalidCastException(
                    "Instance of JsonData doesn't hold a string");

            return data.inst_string;
        }
        #endregion


        #region ICollection Methods
        // 实现 ICollection 接口的 CopyTo 方法，将集合元素复制到数组中
        void ICollection.CopyTo(Array array, int index)
        {
            EnsureCollection().CopyTo(array, index);
        }
        #endregion


        #region IDictionary Methods
        // 实现 IDictionary 接口的 Add 方法，向字典中添加键值对
        void IDictionary.Add(object key, object value)
        {
            JsonData data = ToJsonData(value);

            EnsureDictionary().Add(key, data);

            KeyValuePair<string, JsonData> entry =
                new KeyValuePair<string, JsonData>((string)key, data);
            object_list.Add(entry);

            json = null;
        }

        // 实现 IDictionary 接口的 Clear 方法，清空字典
        void IDictionary.Clear()
        {
            EnsureDictionary().Clear();
            object_list.Clear();
            json = null;
        }

        // 实现 IDictionary 接口的 Contains 方法，判断字典是否包含指定的键
        bool IDictionary.Contains(object key)
        {
            return EnsureDictionary().Contains(key);
        }

        // 实现 IDictionary 接口的 GetEnumerator 方法，获取字典的枚举器
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return ((IOrderedDictionary)this).GetEnumerator();
        }

        // 实现 IDictionary 接口的 Remove 方法，从字典中移除指定键的键值对
        void IDictionary.Remove(object key)
        {
            EnsureDictionary().Remove(key);

            for (int i = 0; i < object_list.Count; i++)
            {
                if (object_list[i].Key == (string)key)
                {
                    object_list.RemoveAt(i);
                    break;
                }
            }

            json = null;
        }
        #endregion


        #region IEnumerable Methods
        // 实现 IEnumerable 接口的 GetEnumerator 方法，获取集合的枚举器
        IEnumerator IEnumerable.GetEnumerator()
        {
            return EnsureCollection().GetEnumerator();
        }
        #endregion


        #region IJsonWrapper Methods
        // 实现 IJsonWrapper 接口的 GetBoolean 方法，获取布尔值
        bool IJsonWrapper.GetBoolean()
        {
            if (type != JsonType.Boolean)
                throw new InvalidOperationException(
                    "JsonData instance doesn't hold a boolean");

            return inst_boolean;
        }

        // 实现 IJsonWrapper 接口的 GetDouble 方法，获取双精度浮点数值
        double IJsonWrapper.GetDouble()
        {
            if (type != JsonType.Double)
                throw new InvalidOperationException(
                    "JsonData instance doesn't hold a double");

            return inst_double;
        }

        // 实现 IJsonWrapper 接口的 GetInt 方法，获取整数值
        int IJsonWrapper.GetInt()
        {
            if (type != JsonType.Int)
                throw new InvalidOperationException(
                    "JsonData instance doesn't hold an int");

            return inst_int;
        }

        // 实现 IJsonWrapper 接口的 GetLong 方法，获取长整数值
        long IJsonWrapper.GetLong()
        {
            if (type != JsonType.Long)
                throw new InvalidOperationException(
                    "JsonData instance doesn't hold a long");

            return inst_long;
        }

        // 实现 IJsonWrapper 接口的 GetString 方法，获取字符串值
        string IJsonWrapper.GetString()
        {
            if (type != JsonType.String)
                throw new InvalidOperationException(
                    "JsonData instance doesn't hold a string");

            return inst_string;
        }

        // 实现 IJsonWrapper 接口的 SetBoolean 方法，设置布尔值
        void IJsonWrapper.SetBoolean(bool val)
        {
            type = JsonType.Boolean;
            inst_boolean = val;
            json = null;
        }

        // 实现 IJsonWrapper 接口的 SetDouble 方法，设置双精度浮点数值
        void IJsonWrapper.SetDouble(double val)
        {
            type = JsonType.Double;
            inst_double = val;
            json = null;
        }

        // 实现 IJsonWrapper 接口的 SetInt 方法，设置整数值
        void IJsonWrapper.SetInt(int val)
        {
            type = JsonType.Int;
            inst_int = val;
            json = null;
        }

        // 实现 IJsonWrapper 接口的 SetLong 方法，设置长整数值
        void IJsonWrapper.SetLong(long val)
        {
            type = JsonType.Long;
            inst_long = val;
            json = null;
        }

        // 实现 IJsonWrapper 接口的 SetString 方法，设置字符串值
        void IJsonWrapper.SetString(string val)
        {
            type = JsonType.String;
            inst_string = val;
            json = null;
        }

        // 实现 IJsonWrapper 接口的 ToJson 方法，将 JsonData 实例序列化为 JSON 字符串
        string IJsonWrapper.ToJson()
        {
            return ToJson();
        }

        // 实现 IJsonWrapper 接口的 ToJson 方法，使用 JsonWriter 将 JsonData 实例序列化为 JSON
        void IJsonWrapper.ToJson(JsonWriter writer)
        {
            ToJson(writer);
        }
        #endregion


        #region IList Methods
        // 实现 IList 接口的 Add 方法，向列表中添加元素
        int IList.Add(object value)
        {
            return Add(value);
        }

        // 实现 IList 接口的 Clear 方法，清空列表
        void IList.Clear()
        {
            EnsureList().Clear();
            json = null;
        }

        // 实现 IList 接口的 Contains 方法，判断列表是否包含指定元素
        bool IList.Contains(object value)
        {
            return EnsureList().Contains(value);
        }

        // 实现 IList 接口的 IndexOf 方法，获取指定元素在列表中的索引
        int IList.IndexOf(object value)
        {
            return EnsureList().IndexOf(value);
        }

        // 实现 IList 接口的 Insert 方法，在指定索引处插入元素
        void IList.Insert(int index, object value)
        {
            EnsureList().Insert(index, value);
            json = null;
        }

        // 实现 IList 接口的 Remove 方法，从列表中移除指定元素
        void IList.Remove(object value)
        {
            EnsureList().Remove(value);
            json = null;
        }

        // 实现 IList 接口的 RemoveAt 方法，移除列表中指定索引处的元素
        void IList.RemoveAt(int index)
        {
            EnsureList().RemoveAt(index);
            json = null;
        }
        #endregion


        #region IOrderedDictionary Methods
        // 实现 IOrderedDictionary 接口的 GetEnumerator 方法，获取有序字典的枚举器
        IDictionaryEnumerator IOrderedDictionary.GetEnumerator()
        {
            EnsureDictionary();

            return new OrderedDictionaryEnumerator(
                object_list.GetEnumerator());
        }

        // 实现 IOrderedDictionary 接口的 Insert 方法，在指定索引处插入键值对
        void IOrderedDictionary.Insert(int idx, object key, object value)
        {
            string property = (string)key;
            JsonData data = ToJsonData(value);

            this[property] = data;

            KeyValuePair<string, JsonData> entry =
                new KeyValuePair<string, JsonData>(property, data);

            object_list.Insert(idx, entry);
        }

        // 实现 IOrderedDictionary 接口的 RemoveAt 方法，移除指定索引处的键值对
        void IOrderedDictionary.RemoveAt(int idx)
        {
            EnsureDictionary();

            inst_object.Remove(object_list[idx].Key);
            object_list.RemoveAt(idx);
        }
        #endregion


        #region Private Methods
        // 确保当前 JsonData 实例是一个集合类型（数组或对象），并返回对应的集合
        private ICollection EnsureCollection()
        {
            if (type == JsonType.Array)
                return (ICollection)inst_array;

            if (type == JsonType.Object)
                return (ICollection)inst_object;

            throw new InvalidOperationException(
                "The JsonData instance has to be initialized first");
        }

        // 确保当前 JsonData 实例是一个字典类型（对象），并返回对应的字典
        private IDictionary EnsureDictionary()
        {
            if (type == JsonType.Object)
                return (IDictionary)inst_object;

            if (type != JsonType.None)
                throw new InvalidOperationException(
                    "Instance of JsonData is not a dictionary");

            type = JsonType.Object;
            inst_object = new Dictionary<string, JsonData>();
            object_list = new List<KeyValuePair<string, JsonData>>();

            return (IDictionary)inst_object;
        }

        // 确保当前 JsonData 实例是一个列表类型（数组），并返回对应的列表
        private IList EnsureList()
        {
            if (type == JsonType.Array)
                return (IList)inst_array;

            if (type != JsonType.None)
                throw new InvalidOperationException(
                    "Instance of JsonData is not a list");

            type = JsonType.Array;
            inst_array = new List<JsonData>();

            return (IList)inst_array;
        }

        // 将对象转换为 JsonData 实例
        private JsonData ToJsonData(object obj)
        {
            if (obj == null)
                return null;

            if (obj is JsonData)
                return (JsonData)obj;

            return new JsonData(obj);
        }

        // 递归地将 IJsonWrapper 实例序列化为 JSON 并写入 JsonWriter
        private static void WriteJson(IJsonWrapper obj, JsonWriter writer)
        {
            if (obj == null)
            {
                writer.Write(null);
                return;
            }

            if (obj.IsString)
            {
                writer.Write(obj.GetString());
                return;
            }

            if (obj.IsBoolean)
            {
                writer.Write(obj.GetBoolean());
                return;
            }

            if (obj.IsDouble)
            {
                writer.Write(obj.GetDouble());
                return;
            }

            if (obj.IsInt)
            {
                writer.Write(obj.GetInt());
                return;
            }

            if (obj.IsLong)
            {
                writer.Write(obj.GetLong());
                return;
            }

            if (obj.IsArray)
            {
                writer.WriteArrayStart();
                foreach (object elem in (IList)obj)
                    WriteJson((JsonData)elem, writer);
                writer.WriteArrayEnd();

                return;
            }

            if (obj.IsObject)
            {
                writer.WriteObjectStart();

                foreach (DictionaryEntry entry in ((IDictionary)obj))
                {
                    writer.WritePropertyName((string)entry.Key);
                    WriteJson((JsonData)entry.Value, writer);
                }
                writer.WriteObjectEnd();

                return;
            }
        }
        #endregion


        // 向列表或对象中添加元素
        public int Add(object value)
        {
            JsonData data = ToJsonData(value);

            json = null;

            return EnsureList().Add(data);
        }

        // 从列表或对象中移除元素
        public bool Remove(object obj)
        {
            json = null;
            if (IsObject)
            {
                JsonData value = null;
                if (inst_object.TryGetValue((string)obj, out value))
                    return inst_object.Remove((string)obj) && object_list.Remove(new KeyValuePair<string, JsonData>((string)obj, value));
                else
                    throw new KeyNotFoundException("The specified key was not found in the JsonData object.");
            }
            if (IsArray)
            {
                return inst_array.Remove(ToJsonData(obj));
            }
            throw new InvalidOperationException(
                    "Instance of JsonData is not an object or a list.");
        }

        // 清空列表或对象
        public void Clear()
        {
            if (IsObject)
            {
                ((IDictionary)this).Clear();
                return;
            }

            if (IsArray)
            {
                ((IList)this).Clear();
                return;
            }
        }

        // 比较两个 JsonData 实例是否相等
        public bool Equals(JsonData x)
        {
            if (x == null)
                return false;

            if (x.type != this.type)
            {
                // further check to see if this is a long to int comparison
                if ((x.type != JsonType.Int && x.type != JsonType.Long)
                    || (this.type != JsonType.Int && this.type != JsonType.Long))
                {
                    return false;
                }
            }

            switch (this.type)
            {
                case JsonType.None:
                    return true;

                case JsonType.Object:
                    return this.inst_object.Equals(x.inst_object);

                case JsonType.Array:
                    return this.inst_array.Equals(x.inst_array);

                case JsonType.String:
                    return this.inst_string.Equals(x.inst_string);

                case JsonType.Int:
                    {
                        if (x.IsLong)
                        {
                            if (x.inst_long < Int32.MinValue || x.inst_long > Int32.MaxValue)
                                return false;
                            return this.inst_int.Equals((int)x.inst_long);
                        }
                        return this.inst_int.Equals(x.inst_int);
                    }

                case JsonType.Long:
                    {
                        if (x.IsInt)
                        {
                            if (this.inst_long < Int32.MinValue || this.inst_long > Int32.MaxValue)
                                return false;
                            return x.inst_int.Equals((int)this.inst_long);
                        }
                        return this.inst_long.Equals(x.inst_long);
                    }

                case JsonType.Double:
                    return this.inst_double.Equals(x.inst_double);

                case JsonType.Boolean:
                    return this.inst_boolean.Equals(x.inst_boolean);
            }

            return false;
        }

        // 获取当前 JsonData 实例的 JSON 数据类型
        public JsonType GetJsonType()
        {
            return type;
        }

        // 设置当前 JsonData 实例的 JSON 数据类型
        public void SetJsonType(JsonType type)
        {
            if (this.type == type)
                return;

            switch (type)
            {
                case JsonType.None:
                    break;

                case JsonType.Object:
                    inst_object = new Dictionary<string, JsonData>();
                    object_list = new List<KeyValuePair<string, JsonData>>();
                    break;

                case JsonType.Array:
                    inst_array = new List<JsonData>();
                    break;

                case JsonType.String:
                    inst_string = default(String);
                    break;

                case JsonType.Int:
                    inst_int = default(Int32);
                    break;

                case JsonType.Long:
                    inst_long = default(Int64);
                    break;

                case JsonType.Double:
                    inst_double = default(Double);
                    break;

                case JsonType.Boolean:
                    inst_boolean = default(Boolean);
                    break;
            }

            this.type = type;
        }

        // 将 JsonData 实例序列化为 JSON 字符串
        public string ToJson()
        {
            if (json != null)
                return json;

            StringWriter sw = new StringWriter();
            JsonWriter writer = new JsonWriter(sw);
            writer.Validate = false;

            WriteJson(this, writer);
            json = sw.ToString();

            return json;
        }

        // 使用 JsonWriter 将 JsonData 实例序列化为 JSON
        public void ToJson(JsonWriter writer)
        {
            bool old_validate = writer.Validate;

            writer.Validate = false;

            WriteJson(this, writer);

            writer.Validate = old_validate;
        }

        // 重写 ToString 方法，返回 JsonData 实例的字符串表示
        public override string ToString()
        {
            switch (type)
            {
                case JsonType.Array:
                    return "JsonData array";

                case JsonType.Boolean:
                    return inst_boolean.ToString();

                case JsonType.Double:
                    return inst_double.ToString();

                case JsonType.Int:
                    return inst_int.ToString();

                case JsonType.Long:
                    return inst_long.ToString();

                case JsonType.Object:
                    return "JsonData object";

                case JsonType.String:
                    return inst_string;
            }

            return "Uninitialized JsonData";
        }
    }


    // 内部类，用于实现有序字典的枚举器
    internal class OrderedDictionaryEnumerator : IDictionaryEnumerator
    {
        // 列表的枚举器
        IEnumerator<KeyValuePair<string, JsonData>> list_enumerator;


        // 获取当前枚举项
        public object Current
        {
            get { return Entry; }
        }

        // 获取当前枚举项的字典项
        public DictionaryEntry Entry
        {
            get
            {
                KeyValuePair<string, JsonData> curr = list_enumerator.Current;
                return new DictionaryEntry(curr.Key, curr.Value);
            }
        }

        // 获取当前枚举项的键
        public object Key
        {
            get { return list_enumerator.Current.Key; }
        }

        // 获取当前枚举项的值
        public object Value
        {
            get { return list_enumerator.Current.Value; }
        }


        // 构造函数，初始化列表枚举器
        public OrderedDictionaryEnumerator(
            IEnumerator<KeyValuePair<string, JsonData>> enumerator)
        {
            list_enumerator = enumerator;
        }


        // 将枚举器移动到下一个元素
        public bool MoveNext()
        {
            return list_enumerator.MoveNext();
        }

        // 将枚举器重置到初始位置
        public void Reset()
        {
            list_enumerator.Reset();
        }
    }
}