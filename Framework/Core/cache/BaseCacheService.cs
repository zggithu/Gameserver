namespace Framework.Core.Cache
{

    // ע�⣬��ͬ��Cache����ͬһ�������ڲ���copyһ������ʵ������;
    // ����һ�������� BaseCacheSerivce��ʹ�÷��� K ��Ϊ�������ͣ�V ��Ϊֵ������
    // ����ʵ���� IPersistable<K, V> �ӿ�
    public abstract class BaseCacheSerivce<K, V> : IPersistable<K, V>
    {
        // ����һ�� AbstractCacheContainer ���͵�˽���ֶΣ����ڴ洢������
        AbstractCacheContainer<K, V> container = null;

        // ���캯������ʵ����ʱ��ʼ����������
        public BaseCacheSerivce()
        {
            // ����һ�� DefaultCacheContainer ʵ���������뵱ǰ������Ϊ����
            this.container = new DefaultCacheContainer<K, V>(this);
        }

        // ���󷽷���Ҫ������ʵ�ִӳ־û��洢�������ݿ⣩�м���ָ������Ӧ��ֵ���߼�
        public abstract V Load(K k);

        // ���������ڴӻ����л�ȡָ������ֵ
        public V Get(K key)
        {
            // ���û��������� Get ������ȡֵ
            return this.container.Get(key);
        }

        // ���������ڻ�ȡ�����д��ڵ�ָ������ֵ
        public V GetIfPresent(K k)
        {
            // ���û��������� GetIfPresent ������ȡֵ
            return this.container.GetIfPresent(k);
        }

        // ���������ڽ�ָ���ļ�ֵ�Դ��뻺��
        public void Put(K key, V v)
        {
            // ���û��������� Put �����洢��ֵ��
            this.container.Put(key, v);
        }

        // ���������ڴӻ������Ƴ�ָ�����Ļ�����
        public void Remove(K key)
        {
            // ���û��������� Remove ����ʹָ�����Ļ�����ʧЧ
            this.container.Remove(key);
        }
    }
}