using System;
using System.Collections;
using System.Collections.Specialized;

namespace LitJson
{
    /// <summary>
    /// JsonMockWrapper ����һ��ģ�����ʵ���� IJsonWrapper �ӿڡ�
    /// ����Ҫ��;��Ϊ�˸���Ч��ִ���������� JSON ���ݵȲ�����
    /// ������Ҫ�������� JSON ����ʱ������ʹ�ø�ģ�������ռλ��
    /// </summary>
    public class JsonMockWrapper : IJsonWrapper
    {
        /// <summary>
        /// ָʾ��ģ������Ƿ��ʾһ�� JSON ���飬ʼ�շ��� false��
        /// </summary>
        public bool IsArray { get { return false; } }

        /// <summary>
        /// ָʾ��ģ������Ƿ��ʾһ���������͵� JSON ֵ��ʼ�շ��� false��
        /// </summary>
        public bool IsBoolean { get { return false; } }

        /// <summary>
        /// ָʾ��ģ������Ƿ��ʾһ��˫���ȸ��������͵� JSON ֵ��ʼ�շ��� false��
        /// </summary>
        public bool IsDouble { get { return false; } }

        /// <summary>
        /// ָʾ��ģ������Ƿ��ʾһ���������͵� JSON ֵ��ʼ�շ��� false��
        /// </summary>
        public bool IsInt { get { return false; } }

        /// <summary>
        /// ָʾ��ģ������Ƿ��ʾһ�����������͵� JSON ֵ��ʼ�շ��� false��
        /// </summary>
        public bool IsLong { get { return false; } }

        /// <summary>
        /// ָʾ��ģ������Ƿ��ʾһ�� JSON ����ʼ�շ��� false��
        /// </summary>
        public bool IsObject { get { return false; } }

        /// <summary>
        /// ָʾ��ģ������Ƿ��ʾһ���ַ������͵� JSON ֵ��ʼ�շ��� false��
        /// </summary>
        public bool IsString { get { return false; } }

        /// <summary>
        /// ��ȡ��ģ������ʾ�Ĳ���ֵ��ʼ�շ��� false��
        /// </summary>
        /// <returns>����ֵ false��</returns>
        public bool GetBoolean() { return false; }

        /// <summary>
        /// ��ȡ��ģ������ʾ��˫���ȸ�������ʼ�շ��� 0.0��
        /// </summary>
        /// <returns>˫���ȸ����� 0.0��</returns>
        public double GetDouble() { return 0.0; }

        /// <summary>
        /// ��ȡ��ģ������ʾ��������ʼ�շ��� 0��
        /// </summary>
        /// <returns>���� 0��</returns>
        public int GetInt() { return 0; }

        /// <summary>
        /// ��ȡ��ģ������ JSON ���ͣ�ʼ�շ��� JsonType.None��
        /// </summary>
        /// <returns>JsonType.None��</returns>
        public JsonType GetJsonType() { return JsonType.None; }

        /// <summary>
        /// ��ȡ��ģ������ʾ�ĳ�������ʼ�շ��� 0L��
        /// </summary>
        /// <returns>������ 0L��</returns>
        public long GetLong() { return 0L; }

        /// <summary>
        /// ��ȡ��ģ������ʾ���ַ�����ʼ�շ��ؿ��ַ�����
        /// </summary>
        /// <returns>���ַ�����</returns>
        public string GetString() { return ""; }

        /// <summary>
        /// ���ø�ģ�����Ĳ���ֵ���˷���Ϊ��ʵ�֣���ִ���κβ�����
        /// </summary>
        /// <param name="val">Ҫ���õĲ���ֵ��</param>
        public void SetBoolean(bool val) { }

        /// <summary>
        /// ���ø�ģ������˫���ȸ��������˷���Ϊ��ʵ�֣���ִ���κβ�����
        /// </summary>
        /// <param name="val">Ҫ���õ�˫���ȸ�������</param>
        public void SetDouble(double val) { }

        /// <summary>
        /// ���ø�ģ�������������˷���Ϊ��ʵ�֣���ִ���κβ�����
        /// </summary>
        /// <param name="val">Ҫ���õ�������</param>
        public void SetInt(int val) { }

        /// <summary>
        /// ���ø�ģ������ JSON ���ͣ��˷���Ϊ��ʵ�֣���ִ���κβ�����
        /// </summary>
        /// <param name="type">Ҫ���õ� JSON ���͡�</param>
        public void SetJsonType(JsonType type) { }

        /// <summary>
        /// ���ø�ģ�����ĳ��������˷���Ϊ��ʵ�֣���ִ���κβ�����
        /// </summary>
        /// <param name="val">Ҫ���õĳ�������</param>
        public void SetLong(long val) { }

        /// <summary>
        /// ���ø�ģ�������ַ������˷���Ϊ��ʵ�֣���ִ���κβ�����
        /// </summary>
        /// <param name="val">Ҫ���õ��ַ�����</param>
        public void SetString(string val) { }

        /// <summary>
        /// ����ģ�����ת��Ϊ JSON �ַ�����ʼ�շ��ؿ��ַ�����
        /// </summary>
        /// <returns>���ַ�����</returns>
        public string ToJson() { return ""; }

        /// <summary>
        /// ʹ��ָ���� JsonWriter ����ģ�����ת��Ϊ JSON ���ݣ��˷���Ϊ��ʵ�֣���ִ���κβ�����
        /// </summary>
        /// <param name="writer">����д�� JSON ���ݵ� JsonWriter��</param>
        public void ToJson(JsonWriter writer) { }


        #region IList �ӿ�ʵ��

        /// <summary>
        /// ָʾ��ģ���б�Ĵ�С�Ƿ�̶���ʼ�շ��� true��
        /// </summary>
        bool IList.IsFixedSize { get { return true; } }

        /// <summary>
        /// ָʾ��ģ���б��Ƿ�Ϊֻ����ʼ�շ��� true��
        /// </summary>
        bool IList.IsReadOnly { get { return true; } }

        /// <summary>
        /// ��ȡ������ģ���б���ָ����������Ԫ�ء�
        /// get ����ʼ�շ��� null��set ����Ϊ��ʵ�֣���ִ���κβ�����
        /// </summary>
        /// <param name="index">Ҫ��ȡ������Ԫ�ص�������</param>
        /// <returns>ʼ�շ��� null��</returns>
        object IList.this[int index]
        {
            get { return null; }
            set { }
        }

        /// <summary>
        /// ��һ��������ӵ�ģ���б��У�ʼ�շ��� 0����ʾ�����Ԫ�ء�
        /// </summary>
        /// <param name="value">Ҫ��ӵĶ���</param>
        /// <returns>ʼ�շ��� 0��</returns>
        int IList.Add(object value) { return 0; }

        /// <summary>
        /// ���ģ���б��˷���Ϊ��ʵ�֣���ִ���κβ�����
        /// </summary>
        void IList.Clear() { }

        /// <summary>
        /// ���ģ���б����Ƿ����ָ���Ķ���ʼ�շ��� false��
        /// </summary>
        /// <param name="value">Ҫ���Ķ���</param>
        /// <returns>ʼ�շ��� false��</returns>
        bool IList.Contains(object value) { return false; }

        /// <summary>
        /// ��ȡָ��������ģ���б��е�������ʼ�շ��� -1����ʾԪ�ز����ڡ�
        /// </summary>
        /// <param name="value">Ҫ���ҵĶ���</param>
        /// <returns>ʼ�շ��� -1��</returns>
        int IList.IndexOf(object value) { return -1; }

        /// <summary>
        /// ��ģ���б��ָ������������һ�����󣬴˷���Ϊ��ʵ�֣���ִ���κβ�����
        /// </summary>
        /// <param name="i">����λ�õ�������</param>
        /// <param name="v">Ҫ����Ķ���</param>
        void IList.Insert(int i, object v) { }

        /// <summary>
        /// ��ģ���б����Ƴ�ָ���Ķ��󣬴˷���Ϊ��ʵ�֣���ִ���κβ�����
        /// </summary>
        /// <param name="value">Ҫ�Ƴ��Ķ���</param>
        void IList.Remove(object value) { }

        /// <summary>
        /// �Ƴ�ģ���б���ָ����������Ԫ�أ��˷���Ϊ��ʵ�֣���ִ���κβ�����
        /// </summary>
        /// <param name="index">Ҫ�Ƴ�Ԫ�ص�������</param>
        void IList.RemoveAt(int index) { }

        #endregion

        #region ICollection �ӿ�ʵ��

        /// <summary>
        /// ��ȡģ�⼯����Ԫ�ص�������ʼ�շ��� 0��
        /// </summary>
        int ICollection.Count { get { return 0; } }

        /// <summary>
        /// ָʾģ�⼯���Ƿ����̰߳�ȫ�ģ�ʼ�շ��� false��
        /// </summary>
        bool ICollection.IsSynchronized { get { return false; } }

        /// <summary>
        /// ��ȡ������ͬ����ģ�⼯�ϵķ��ʵĶ���ʼ�շ��� null��
        /// </summary>
        object ICollection.SyncRoot { get { return null; } }

        /// <summary>
        /// ��ģ�⼯�ϵ�Ԫ�ظ��Ƶ�ָ�������ָ������λ�ã��˷���Ϊ��ʵ�֣���ִ���κβ�����
        /// </summary>
        /// <param name="array">Ҫ���Ƶ������顣</param>
        /// <param name="index">��ʼ���Ƶ�����λ�á�</param>
        void ICollection.CopyTo(Array array, int index) { }

        #endregion

        #region IEnumerable �ӿ�ʵ��

        /// <summary>
        /// ����һ�����ڱ���ģ�⼯�ϵ�ö������ʼ�շ��� null��
        /// </summary>
        /// <returns>ʼ�շ��� null��</returns>
        IEnumerator IEnumerable.GetEnumerator() { return null; }

        #endregion

        #region IDictionary �ӿ�ʵ��

        /// <summary>
        /// ָʾģ���ֵ�Ĵ�С�Ƿ�̶���ʼ�շ��� true��
        /// </summary>
        bool IDictionary.IsFixedSize { get { return true; } }

        /// <summary>
        /// ָʾģ���ֵ��Ƿ�Ϊֻ����ʼ�շ��� true��
        /// </summary>
        bool IDictionary.IsReadOnly { get { return true; } }

        /// <summary>
        /// ��ȡģ���ֵ��е����м���ʼ�շ��� null��
        /// </summary>
        ICollection IDictionary.Keys { get { return null; } }

        /// <summary>
        /// ��ȡģ���ֵ��е�����ֵ��ʼ�շ��� null��
        /// </summary>
        ICollection IDictionary.Values { get { return null; } }

        /// <summary>
        /// ��ȡ������ģ���ֵ�����ָ����������ֵ��
        /// get ����ʼ�շ��� null��set ����Ϊ��ʵ�֣���ִ���κβ�����
        /// </summary>
        /// <param name="key">Ҫ��ȡ������ֵ�ļ���</param>
        /// <returns>ʼ�շ��� null��</returns>
        object IDictionary.this[object key]
        {
            get { return null; }
            set { }
        }

        /// <summary>
        /// ��ģ���ֵ������һ������ָ������ֵ��Ԫ�أ��˷���Ϊ��ʵ�֣���ִ���κβ�����
        /// </summary>
        /// <param name="k">Ҫ��ӵ�Ԫ�صļ���</param>
        /// <param name="v">Ҫ��ӵ�Ԫ�ص�ֵ��</param>
        void IDictionary.Add(object k, object v) { }

        /// <summary>
        /// ���ģ���ֵ䣬�˷���Ϊ��ʵ�֣���ִ���κβ�����
        /// </summary>
        void IDictionary.Clear() { }

        /// <summary>
        /// ���ģ���ֵ����Ƿ��������ָ������Ԫ�أ�ʼ�շ��� false��
        /// </summary>
        /// <param name="key">Ҫ���ļ���</param>
        /// <returns>ʼ�շ��� false��</returns>
        bool IDictionary.Contains(object key) { return false; }

        /// <summary>
        /// ��ģ���ֵ����Ƴ�����ָ������Ԫ�أ��˷���Ϊ��ʵ�֣���ִ���κβ�����
        /// </summary>
        /// <param name="key">Ҫ�Ƴ�Ԫ�صļ���</param>
        void IDictionary.Remove(object key) { }

        /// <summary>
        /// ����һ�����ڱ���ģ���ֵ��ö������ʼ�շ��� null��
        /// </summary>
        /// <returns>ʼ�շ��� null��</returns>
        IDictionaryEnumerator IDictionary.GetEnumerator() { return null; }

        #endregion

        #region IOrderedDictionary �ӿ�ʵ��

        /// <summary>
        /// ��ȡ������ģ�������ֵ���ָ����������Ԫ�ء�
        /// get ����ʼ�շ��� null��set ����Ϊ��ʵ�֣���ִ���κβ�����
        /// </summary>
        /// <param name="idx">Ҫ��ȡ������Ԫ�ص�������</param>
        /// <returns>ʼ�շ��� null��</returns>
        object IOrderedDictionary.this[int idx]
        {
            get { return null; }
            set { }
        }

        /// <summary>
        /// ����һ�����ڱ���ģ�������ֵ��ö������ʼ�շ��� null��
        /// </summary>
        /// <returns>ʼ�շ��� null��</returns>
        IDictionaryEnumerator IOrderedDictionary.GetEnumerator()
        {
            return null;
        }

        /// <summary>
        /// ��ģ�������ֵ��ָ������������һ������ָ������ֵ��Ԫ�أ��˷���Ϊ��ʵ�֣���ִ���κβ�����
        /// </summary>
        /// <param name="i">����λ�õ�������</param>
        /// <param name="k">Ҫ����Ԫ�صļ���</param>
        /// <param name="v">Ҫ����Ԫ�ص�ֵ��</param>
        void IOrderedDictionary.Insert(int i, object k, object v) { }

        /// <summary>
        /// �Ƴ�ģ�������ֵ���ָ����������Ԫ�أ��˷���Ϊ��ʵ�֣���ִ���κβ�����
        /// </summary>
        /// <param name="i">Ҫ�Ƴ�Ԫ�ص�������</param>
        void IOrderedDictionary.RemoveAt(int i) { }

        #endregion
    }
}