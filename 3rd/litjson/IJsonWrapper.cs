// ���� System.Collections �����ռ䣬�������ռ��ṩ��һЩ�����ļ��Ͻӿں��࣬
// ���� IList �ӿڣ����� IJsonWrapper ��̳иýӿ��Ծ߱��б���ز�������
using System.Collections;
// ���� System.Collections.Specialized �����ռ䣬��������һЩ������;�ļ�����ͽӿڣ�
// ���� IOrderedDictionary �ӿڣ����� IJsonWrapper ��̳иýӿ���ʵ�������ֵ�Ĺ���
using System.Collections.Specialized;

// ����һ����Ϊ LitJson �������ռ䣬���ڽ���ص� JSON �������ͺ͹�����֯��һ��
// ���ⲻͬ����ģ��֮���������ͻ
namespace LitJson
{
    // ����һ������ö������ JsonType�����ڱ�ʾ���ֿ��ܵ� JSON ��������
    public enum JsonType
    {
        // ��ʾû���ض��� JSON �������ͣ������ڳ�ʼ�����ʾ��״̬
        None,

        // ��ʾ JSON ����JSON �������ɼ�ֵ����ɵ����ݽṹ��ͨ���û����� {} ����
        Object,
        // ��ʾ JSON ���飬JSON ������һ�������ֵ��ͨ���÷����� [] ����
        Array,
        // ��ʾ JSON �ַ�����ͨ����˫���� "" ����
        String,
        // ��ʾ JSON �е���������
        Int,
        // ��ʾ JSON �еĳ���������
        Long,
        // ��ʾ JSON �е�˫���ȸ���������
        Double,
        // ��ʾ JSON �еĲ������ͣ�ֵΪ true �� false
        Boolean
    }

    // ����һ�������ӿ� IJsonWrapper�����̳��� IList �� IOrderedDictionary �ӿڣ�
    // ��ζ��ʵ�ָýӿڵ��ཫͬʱ�߱��б�������ֵ�Ĳ������������ڴ������ JSON ����
    public interface IJsonWrapper : IList, IOrderedDictionary
    {
        // ֻ�����ԣ������жϵ�ǰ IJsonWrapper ʵ������ʾ�� JSON �����Ƿ�Ϊ��������
        bool IsArray { get; }
        // ֻ�����ԣ������жϵ�ǰ IJsonWrapper ʵ������ʾ�� JSON �����Ƿ�Ϊ��������
        bool IsBoolean { get; }
        // ֻ�����ԣ������жϵ�ǰ IJsonWrapper ʵ������ʾ�� JSON �����Ƿ�Ϊ˫���ȸ���������
        bool IsDouble { get; }
        // ֻ�����ԣ������жϵ�ǰ IJsonWrapper ʵ������ʾ�� JSON �����Ƿ�Ϊ��������
        bool IsInt { get; }
        // ֻ�����ԣ������жϵ�ǰ IJsonWrapper ʵ������ʾ�� JSON �����Ƿ�Ϊ����������
        bool IsLong { get; }
        // ֻ�����ԣ������жϵ�ǰ IJsonWrapper ʵ������ʾ�� JSON �����Ƿ�Ϊ��������
        bool IsObject { get; }
        // ֻ�����ԣ������жϵ�ǰ IJsonWrapper ʵ������ʾ�� JSON �����Ƿ�Ϊ�ַ�������
        bool IsString { get; }

        // ���������ڻ�ȡ��ǰ IJsonWrapper ʵ������ʾ�� JSON ���ݵĲ���ֵ
        bool GetBoolean();
        // ���������ڻ�ȡ��ǰ IJsonWrapper ʵ������ʾ�� JSON ���ݵ�˫���ȸ�����ֵ
        double GetDouble();
        // ���������ڻ�ȡ��ǰ IJsonWrapper ʵ������ʾ�� JSON ���ݵ�����ֵ
        int GetInt();
        // ���������ڻ�ȡ��ǰ IJsonWrapper ʵ������ʾ�� JSON ���ݵ����ͣ�����ֵΪ JsonType ö������
        JsonType GetJsonType();
        // ���������ڻ�ȡ��ǰ IJsonWrapper ʵ������ʾ�� JSON ���ݵĳ�����ֵ
        long GetLong();
        // ���������ڻ�ȡ��ǰ IJsonWrapper ʵ������ʾ�� JSON ���ݵ��ַ���ֵ
        string GetString();

        // ���������ڽ���ǰ IJsonWrapper ʵ������ʾ�� JSON ��������Ϊָ���Ĳ���ֵ
        void SetBoolean(bool val);
        // ���������ڽ���ǰ IJsonWrapper ʵ������ʾ�� JSON ��������Ϊָ����˫���ȸ�����ֵ
        void SetDouble(double val);
        // ���������ڽ���ǰ IJsonWrapper ʵ������ʾ�� JSON ��������Ϊָ��������ֵ
        void SetInt(int val);
        // ���������ڽ���ǰ IJsonWrapper ʵ������ʾ�� JSON ���ݵ���������Ϊָ���� JsonType ö��ֵ
        void SetJsonType(JsonType type);
        // ���������ڽ���ǰ IJsonWrapper ʵ������ʾ�� JSON ��������Ϊָ���ĳ�����ֵ
        void SetLong(long val);
        // ���������ڽ���ǰ IJsonWrapper ʵ������ʾ�� JSON ��������Ϊָ�����ַ���ֵ
        void SetString(string val);

        // ����������ǰ IJsonWrapper ʵ������ʾ�� JSON �������л�Ϊ JSON �ַ���������
        string ToJson();
        // ����������ǰ IJsonWrapper ʵ������ʾ�� JSON ����ͨ��ָ���� JsonWriter ����������л�
        void ToJson(JsonWriter writer);
    }
}