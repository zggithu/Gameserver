using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

// ���� LitJson �����ռ䣬������֯���� JSON ���ݵ������ͽӿ�
namespace LitJson
{
    // JsonData ��ʵ���� IJsonWrapper �ӿں� IEquatable<JsonData> �ӿ�
    // IJsonWrapper �ӿ��ṩ�˴������ JSON �������͵ķ���
    // IEquatable<JsonData> �ӿ����ڱȽ����� JsonData ʵ���Ƿ����
    public class JsonData : IJsonWrapper, IEquatable<JsonData>
    {
        #region Fields
        // �洢 JSON ���������
        private IList<JsonData> inst_array;
        // �洢 JSON �������͵�����
        private bool inst_boolean;
        // �洢 JSON ˫���ȸ��������͵�����
        private double inst_double;
        // �洢 JSON �������͵�����
        private int inst_int;
        // �洢 JSON ���������͵�����
        private long inst_long;
        // �洢 JSON ��������ݣ�ʹ���ֵ�洢��ֵ��
        private IDictionary<string, JsonData> inst_object;
        // �洢 JSON �ַ������͵�����
        private string inst_string;
        // �洢���л���� JSON �ַ���
        private string json;
        // ��ʾ��ǰ JsonData ʵ���� JSON ��������
        private JsonType type;

        // ����ʵ�� IOrderedDictionary �ӿڣ��洢����ļ�ֵ���б���֤˳��
        private IList<KeyValuePair<string, JsonData>> object_list;
        #endregion


        #region Properties
        // ��ȡ��ǰ JsonData ʵ����Ϊ����ʱ��Ԫ������
        public int Count
        {
            get { return EnsureCollection().Count; }
        }

        // �жϵ�ǰ JsonData ʵ���Ƿ�Ϊ JSON ��������
        public bool IsArray
        {
            get { return type == JsonType.Array; }
        }

        // �жϵ�ǰ JsonData ʵ���Ƿ�Ϊ JSON ��������
        public bool IsBoolean
        {
            get { return type == JsonType.Boolean; }
        }

        // �жϵ�ǰ JsonData ʵ���Ƿ�Ϊ JSON ˫���ȸ���������
        public bool IsDouble
        {
            get { return type == JsonType.Double; }
        }

        // �жϵ�ǰ JsonData ʵ���Ƿ�Ϊ JSON ��������
        public bool IsInt
        {
            get { return type == JsonType.Int; }
        }

        // �жϵ�ǰ JsonData ʵ���Ƿ�Ϊ JSON ����������
        public bool IsLong
        {
            get { return type == JsonType.Long; }
        }

        // �жϵ�ǰ JsonData ʵ���Ƿ�Ϊ JSON ��������
        public bool IsObject
        {
            get { return type == JsonType.Object; }
        }

        // �жϵ�ǰ JsonData ʵ���Ƿ�Ϊ JSON �ַ�������
        public bool IsString
        {
            get { return type == JsonType.String; }
        }

        // ��ȡ��ǰ JsonData ʵ����Ϊ����ʱ�����м��ļ���
        public ICollection<string> Keys
        {
            get { EnsureDictionary(); return inst_object.Keys; }
        }

        /// <summary>
        /// Determines whether the json contains an element that has the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the json.</param>
        /// <returns>true if the json contains an element that has the specified key; otherwise, false.</returns>
        // �жϵ�ǰ JsonData ʵ����Ϊ����ʱ�Ƿ����ָ���ļ�
        public Boolean ContainsKey(String key)
        {
            EnsureDictionary();
            return this.inst_object.Keys.Contains(key);
        }
        #endregion


        #region ICollection Properties
        // ʵ�� ICollection �ӿڵ� Count ���ԣ���ȡԪ������
        int ICollection.Count
        {
            get
            {
                return Count;
            }
        }

        // ʵ�� ICollection �ӿڵ� IsSynchronized ���ԣ��жϼ����Ƿ��̰߳�ȫ
        bool ICollection.IsSynchronized
        {
            get
            {
                return EnsureCollection().IsSynchronized;
            }
        }

        // ʵ�� ICollection �ӿڵ� SyncRoot ���ԣ���ȡ����ͬ�����Ϸ��ʵĶ���
        object ICollection.SyncRoot
        {
            get
            {
                return EnsureCollection().SyncRoot;
            }
        }
        #endregion


        #region IDictionary Properties
        // ʵ�� IDictionary �ӿڵ� IsFixedSize ���ԣ��ж��ֵ��Ƿ���й̶���С
        bool IDictionary.IsFixedSize
        {
            get
            {
                return EnsureDictionary().IsFixedSize;
            }
        }

        // ʵ�� IDictionary �ӿڵ� IsReadOnly ���ԣ��ж��ֵ��Ƿ�Ϊֻ��
        bool IDictionary.IsReadOnly
        {
            get
            {
                return EnsureDictionary().IsReadOnly;
            }
        }

        // ʵ�� IDictionary �ӿڵ� Keys ���ԣ���ȡ�ֵ�����м��ļ���
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

        // ʵ�� IDictionary �ӿڵ� Values ���ԣ���ȡ�ֵ������ֵ�ļ���
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
        // ʵ�� IJsonWrapper �ӿڵ� IsArray ���ԣ��ж��Ƿ�Ϊ��������
        bool IJsonWrapper.IsArray
        {
            get { return IsArray; }
        }

        // ʵ�� IJsonWrapper �ӿڵ� IsBoolean ���ԣ��ж��Ƿ�Ϊ��������
        bool IJsonWrapper.IsBoolean
        {
            get { return IsBoolean; }
        }

        // ʵ�� IJsonWrapper �ӿڵ� IsDouble ���ԣ��ж��Ƿ�Ϊ˫���ȸ���������
        bool IJsonWrapper.IsDouble
        {
            get { return IsDouble; }
        }

        // ʵ�� IJsonWrapper �ӿڵ� IsInt ���ԣ��ж��Ƿ�Ϊ��������
        bool IJsonWrapper.IsInt
        {
            get { return IsInt; }
        }

        // ʵ�� IJsonWrapper �ӿڵ� IsLong ���ԣ��ж��Ƿ�Ϊ����������
        bool IJsonWrapper.IsLong
        {
            get { return IsLong; }
        }

        // ʵ�� IJsonWrapper �ӿڵ� IsObject ���ԣ��ж��Ƿ�Ϊ��������
        bool IJsonWrapper.IsObject
        {
            get { return IsObject; }
        }

        // ʵ�� IJsonWrapper �ӿڵ� IsString ���ԣ��ж��Ƿ�Ϊ�ַ�������
        bool IJsonWrapper.IsString
        {
            get { return IsString; }
        }
        #endregion


        #region IList Properties
        // ʵ�� IList �ӿڵ� IsFixedSize ���ԣ��ж��б��Ƿ���й̶���С
        bool IList.IsFixedSize
        {
            get
            {
                return EnsureList().IsFixedSize;
            }
        }

        // ʵ�� IList �ӿڵ� IsReadOnly ���ԣ��ж��б��Ƿ�Ϊֻ��
        bool IList.IsReadOnly
        {
            get
            {
                return EnsureList().IsReadOnly;
            }
        }
        #endregion


        #region IDictionary Indexer
        // ʵ�� IDictionary �ӿڵ���������ͨ������ȡ�������ֵ��е�ֵ
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
        // ʵ�� IOrderedDictionary �ӿڵ���������ͨ��������ȡ�����������ֵ��е�ֵ
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
        // ʵ�� IList �ӿڵ���������ͨ��������ȡ�������б��е�ֵ
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
        // ������������ͨ����������ȡ������ JsonData ʵ����Ϊ����ʱ������ֵ
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

        // ������������ͨ��������ȡ������ JsonData ʵ����Ϊ��������ʱ��ֵ
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
        // Ĭ�Ϲ��캯��
        public JsonData()
        {
        }

        // ���캯����ʹ�ò���ֵ��ʼ�� JsonData ʵ��
        public JsonData(bool boolean)
        {
            type = JsonType.Boolean;
            inst_boolean = boolean;
        }

        // ���캯����ʹ��˫���ȸ�������ʼ�� JsonData ʵ��
        public JsonData(double number)
        {
            type = JsonType.Double;
            inst_double = number;
        }

        // ���캯����ʹ��������ʼ�� JsonData ʵ��
        public JsonData(int number)
        {
            type = JsonType.Int;
            inst_int = number;
        }

        // ���캯����ʹ�ó�������ʼ�� JsonData ʵ��
        public JsonData(long number)
        {
            type = JsonType.Long;
            inst_long = number;
        }

        // ���캯����ʹ�ö����ʼ�� JsonData ʵ�������ݶ����������� JsonData ����
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

        // ���캯����ʹ���ַ�����ʼ�� JsonData ʵ��
        public JsonData(string str)
        {
            type = JsonType.String;
            inst_string = str;
        }
        #endregion


        #region Implicit Conversions
        // ��ʽת����������ֵת��Ϊ JsonData ʵ��
        public static implicit operator JsonData(Boolean data)
        {
            return new JsonData(data);
        }

        // ��ʽת������˫���ȸ�����ת��Ϊ JsonData ʵ��
        public static implicit operator JsonData(Double data)
        {
            return new JsonData(data);
        }

        // ��ʽת����������ת��Ϊ JsonData ʵ��
        public static implicit operator JsonData(Int32 data)
        {
            return new JsonData(data);
        }

        // ��ʽת������������ת��Ϊ JsonData ʵ��
        public static implicit operator JsonData(Int64 data)
        {
            return new JsonData(data);
        }

        // ��ʽת�������ַ���ת��Ϊ JsonData ʵ��
        public static implicit operator JsonData(String data)
        {
            return new JsonData(data);
        }
        #endregion


        #region Explicit Conversions
        // ��ʽת������ JsonData ʵ��ת��Ϊ����ֵ
        public static explicit operator Boolean(JsonData data)
        {
            if (data.type != JsonType.Boolean)
                throw new InvalidCastException(
                    "Instance of JsonData doesn't hold a double");

            return data.inst_boolean;
        }

        // ��ʽת������ JsonData ʵ��ת��Ϊ˫���ȸ�����
        public static explicit operator Double(JsonData data)
        {
            if (data.type != JsonType.Double)
                throw new InvalidCastException(
                    "Instance of JsonData doesn't hold a double");

            return data.inst_double;
        }

        // ��ʽת������ JsonData ʵ��ת��Ϊ����
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

        // ��ʽת������ JsonData ʵ��ת��Ϊ������
        public static explicit operator Int64(JsonData data)
        {
            if (data.type != JsonType.Long && data.type != JsonType.Int)
            {
                throw new InvalidCastException(
                    "Instance of JsonData doesn't hold a long");
            }

            return data.type == JsonType.Long ? data.inst_long : data.inst_int;
        }

        // ��ʽת������ JsonData ʵ��ת��Ϊ�ַ���
        public static explicit operator String(JsonData data)
        {
            if (data.type != JsonType.String)
                throw new InvalidCastException(
                    "Instance of JsonData doesn't hold a string");

            return data.inst_string;
        }
        #endregion


        #region ICollection Methods
        // ʵ�� ICollection �ӿڵ� CopyTo ������������Ԫ�ظ��Ƶ�������
        void ICollection.CopyTo(Array array, int index)
        {
            EnsureCollection().CopyTo(array, index);
        }
        #endregion


        #region IDictionary Methods
        // ʵ�� IDictionary �ӿڵ� Add ���������ֵ�����Ӽ�ֵ��
        void IDictionary.Add(object key, object value)
        {
            JsonData data = ToJsonData(value);

            EnsureDictionary().Add(key, data);

            KeyValuePair<string, JsonData> entry =
                new KeyValuePair<string, JsonData>((string)key, data);
            object_list.Add(entry);

            json = null;
        }

        // ʵ�� IDictionary �ӿڵ� Clear ����������ֵ�
        void IDictionary.Clear()
        {
            EnsureDictionary().Clear();
            object_list.Clear();
            json = null;
        }

        // ʵ�� IDictionary �ӿڵ� Contains �������ж��ֵ��Ƿ����ָ���ļ�
        bool IDictionary.Contains(object key)
        {
            return EnsureDictionary().Contains(key);
        }

        // ʵ�� IDictionary �ӿڵ� GetEnumerator ��������ȡ�ֵ��ö����
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return ((IOrderedDictionary)this).GetEnumerator();
        }

        // ʵ�� IDictionary �ӿڵ� Remove ���������ֵ����Ƴ�ָ�����ļ�ֵ��
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
        // ʵ�� IEnumerable �ӿڵ� GetEnumerator ��������ȡ���ϵ�ö����
        IEnumerator IEnumerable.GetEnumerator()
        {
            return EnsureCollection().GetEnumerator();
        }
        #endregion


        #region IJsonWrapper Methods
        // ʵ�� IJsonWrapper �ӿڵ� GetBoolean ��������ȡ����ֵ
        bool IJsonWrapper.GetBoolean()
        {
            if (type != JsonType.Boolean)
                throw new InvalidOperationException(
                    "JsonData instance doesn't hold a boolean");

            return inst_boolean;
        }

        // ʵ�� IJsonWrapper �ӿڵ� GetDouble ��������ȡ˫���ȸ�����ֵ
        double IJsonWrapper.GetDouble()
        {
            if (type != JsonType.Double)
                throw new InvalidOperationException(
                    "JsonData instance doesn't hold a double");

            return inst_double;
        }

        // ʵ�� IJsonWrapper �ӿڵ� GetInt ��������ȡ����ֵ
        int IJsonWrapper.GetInt()
        {
            if (type != JsonType.Int)
                throw new InvalidOperationException(
                    "JsonData instance doesn't hold an int");

            return inst_int;
        }

        // ʵ�� IJsonWrapper �ӿڵ� GetLong ��������ȡ������ֵ
        long IJsonWrapper.GetLong()
        {
            if (type != JsonType.Long)
                throw new InvalidOperationException(
                    "JsonData instance doesn't hold a long");

            return inst_long;
        }

        // ʵ�� IJsonWrapper �ӿڵ� GetString ��������ȡ�ַ���ֵ
        string IJsonWrapper.GetString()
        {
            if (type != JsonType.String)
                throw new InvalidOperationException(
                    "JsonData instance doesn't hold a string");

            return inst_string;
        }

        // ʵ�� IJsonWrapper �ӿڵ� SetBoolean ���������ò���ֵ
        void IJsonWrapper.SetBoolean(bool val)
        {
            type = JsonType.Boolean;
            inst_boolean = val;
            json = null;
        }

        // ʵ�� IJsonWrapper �ӿڵ� SetDouble ����������˫���ȸ�����ֵ
        void IJsonWrapper.SetDouble(double val)
        {
            type = JsonType.Double;
            inst_double = val;
            json = null;
        }

        // ʵ�� IJsonWrapper �ӿڵ� SetInt ��������������ֵ
        void IJsonWrapper.SetInt(int val)
        {
            type = JsonType.Int;
            inst_int = val;
            json = null;
        }

        // ʵ�� IJsonWrapper �ӿڵ� SetLong ���������ó�����ֵ
        void IJsonWrapper.SetLong(long val)
        {
            type = JsonType.Long;
            inst_long = val;
            json = null;
        }

        // ʵ�� IJsonWrapper �ӿڵ� SetString �����������ַ���ֵ
        void IJsonWrapper.SetString(string val)
        {
            type = JsonType.String;
            inst_string = val;
            json = null;
        }

        // ʵ�� IJsonWrapper �ӿڵ� ToJson �������� JsonData ʵ�����л�Ϊ JSON �ַ���
        string IJsonWrapper.ToJson()
        {
            return ToJson();
        }

        // ʵ�� IJsonWrapper �ӿڵ� ToJson ������ʹ�� JsonWriter �� JsonData ʵ�����л�Ϊ JSON
        void IJsonWrapper.ToJson(JsonWriter writer)
        {
            ToJson(writer);
        }
        #endregion


        #region IList Methods
        // ʵ�� IList �ӿڵ� Add ���������б������Ԫ��
        int IList.Add(object value)
        {
            return Add(value);
        }

        // ʵ�� IList �ӿڵ� Clear ����������б�
        void IList.Clear()
        {
            EnsureList().Clear();
            json = null;
        }

        // ʵ�� IList �ӿڵ� Contains �������ж��б��Ƿ����ָ��Ԫ��
        bool IList.Contains(object value)
        {
            return EnsureList().Contains(value);
        }

        // ʵ�� IList �ӿڵ� IndexOf ��������ȡָ��Ԫ�����б��е�����
        int IList.IndexOf(object value)
        {
            return EnsureList().IndexOf(value);
        }

        // ʵ�� IList �ӿڵ� Insert ��������ָ������������Ԫ��
        void IList.Insert(int index, object value)
        {
            EnsureList().Insert(index, value);
            json = null;
        }

        // ʵ�� IList �ӿڵ� Remove ���������б����Ƴ�ָ��Ԫ��
        void IList.Remove(object value)
        {
            EnsureList().Remove(value);
            json = null;
        }

        // ʵ�� IList �ӿڵ� RemoveAt �������Ƴ��б���ָ����������Ԫ��
        void IList.RemoveAt(int index)
        {
            EnsureList().RemoveAt(index);
            json = null;
        }
        #endregion


        #region IOrderedDictionary Methods
        // ʵ�� IOrderedDictionary �ӿڵ� GetEnumerator ��������ȡ�����ֵ��ö����
        IDictionaryEnumerator IOrderedDictionary.GetEnumerator()
        {
            EnsureDictionary();

            return new OrderedDictionaryEnumerator(
                object_list.GetEnumerator());
        }

        // ʵ�� IOrderedDictionary �ӿڵ� Insert ��������ָ�������������ֵ��
        void IOrderedDictionary.Insert(int idx, object key, object value)
        {
            string property = (string)key;
            JsonData data = ToJsonData(value);

            this[property] = data;

            KeyValuePair<string, JsonData> entry =
                new KeyValuePair<string, JsonData>(property, data);

            object_list.Insert(idx, entry);
        }

        // ʵ�� IOrderedDictionary �ӿڵ� RemoveAt �������Ƴ�ָ���������ļ�ֵ��
        void IOrderedDictionary.RemoveAt(int idx)
        {
            EnsureDictionary();

            inst_object.Remove(object_list[idx].Key);
            object_list.RemoveAt(idx);
        }
        #endregion


        #region Private Methods
        // ȷ����ǰ JsonData ʵ����һ���������ͣ��������󣩣������ض�Ӧ�ļ���
        private ICollection EnsureCollection()
        {
            if (type == JsonType.Array)
                return (ICollection)inst_array;

            if (type == JsonType.Object)
                return (ICollection)inst_object;

            throw new InvalidOperationException(
                "The JsonData instance has to be initialized first");
        }

        // ȷ����ǰ JsonData ʵ����һ���ֵ����ͣ����󣩣������ض�Ӧ���ֵ�
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

        // ȷ����ǰ JsonData ʵ����һ���б����ͣ����飩�������ض�Ӧ���б�
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

        // ������ת��Ϊ JsonData ʵ��
        private JsonData ToJsonData(object obj)
        {
            if (obj == null)
                return null;

            if (obj is JsonData)
                return (JsonData)obj;

            return new JsonData(obj);
        }

        // �ݹ�ؽ� IJsonWrapper ʵ�����л�Ϊ JSON ��д�� JsonWriter
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


        // ���б����������Ԫ��
        public int Add(object value)
        {
            JsonData data = ToJsonData(value);

            json = null;

            return EnsureList().Add(data);
        }

        // ���б��������Ƴ�Ԫ��
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

        // ����б�����
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

        // �Ƚ����� JsonData ʵ���Ƿ����
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

        // ��ȡ��ǰ JsonData ʵ���� JSON ��������
        public JsonType GetJsonType()
        {
            return type;
        }

        // ���õ�ǰ JsonData ʵ���� JSON ��������
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

        // �� JsonData ʵ�����л�Ϊ JSON �ַ���
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

        // ʹ�� JsonWriter �� JsonData ʵ�����л�Ϊ JSON
        public void ToJson(JsonWriter writer)
        {
            bool old_validate = writer.Validate;

            writer.Validate = false;

            WriteJson(this, writer);

            writer.Validate = old_validate;
        }

        // ��д ToString ���������� JsonData ʵ�����ַ�����ʾ
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


    // �ڲ��࣬����ʵ�������ֵ��ö����
    internal class OrderedDictionaryEnumerator : IDictionaryEnumerator
    {
        // �б��ö����
        IEnumerator<KeyValuePair<string, JsonData>> list_enumerator;


        // ��ȡ��ǰö����
        public object Current
        {
            get { return Entry; }
        }

        // ��ȡ��ǰö������ֵ���
        public DictionaryEntry Entry
        {
            get
            {
                KeyValuePair<string, JsonData> curr = list_enumerator.Current;
                return new DictionaryEntry(curr.Key, curr.Value);
            }
        }

        // ��ȡ��ǰö����ļ�
        public object Key
        {
            get { return list_enumerator.Current.Key; }
        }

        // ��ȡ��ǰö�����ֵ
        public object Value
        {
            get { return list_enumerator.Current.Value; }
        }


        // ���캯������ʼ���б�ö����
        public OrderedDictionaryEnumerator(
            IEnumerator<KeyValuePair<string, JsonData>> enumerator)
        {
            list_enumerator = enumerator;
        }


        // ��ö�����ƶ�����һ��Ԫ��
        public bool MoveNext()
        {
            return list_enumerator.MoveNext();
        }

        // ��ö�������õ���ʼλ��
        public void Reset()
        {
            list_enumerator.Reset();
        }
    }
}