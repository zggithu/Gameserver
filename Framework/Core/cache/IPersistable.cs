namespace Framework.Core.Cache
{
    // ����һ�����ͽӿ� IPersistable��K ��ʾ�������ͣ�V ��ʾֵ������
    // �˽ӿ����ڹ淶�ӳ־û��洢�������ݵĲ���
    public interface IPersistable<K, V>
    {
        // ����һ�����󷽷� Load�����ڴӳ־û��洢�м���ָ���� k ��Ӧ��ֵ
        // �κ�ʵ�ִ˽ӿڵ��඼����ʵ�ָ÷��������ṩ����ļ����߼�
        public V Load(K k);
    }
}