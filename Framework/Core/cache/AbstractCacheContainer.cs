using System;
using System.Runtime.Caching;

namespace Framework.Core.Cache
{

    // ����һ�������࣬ʹ�÷��� K ��Ϊ�������ͣ�V ��Ϊֵ������
    public abstract class AbstractCacheContainer<K, V>
    {
        // ����һ�� LoadingCache ���͵�˽���ֶΣ����ڴ洢������
        private LoadingCache<K, V> cache = null;

        // ���캯������ʵ����ʱ��ʼ������
        public AbstractCacheContainer()
        {
            // ����һ�� CacheBuilder ʵ�������ڹ�������
            var builder = CacheBuilder<K, V>.NewBuilder();
            // ���û����������һ�η��ʺ� 18000000 ���루�� 5 Сʱ������
            builder.SetExpireAfterAccessMilliseconds(18000000);
            // ���û�������д��� 18000000 ���루�� 5 Сʱ������
            builder.SetExpireAfterWriteMilliseconds(18000000);
            // ע�Ỻ�����Ƴ�����������������Ƴ�ʱ���� onRemoval ����
            builder.RemovalListener(this.onRemoval);

            // ʹ�� DataLoader ������Ϊ���ݼ���������������ʵ��
            this.cache = builder.Build(DataLoader);
        }

        // ���ⷽ�������ڴӻ����л�ȡָ������ֵ
        public virtual V Get(K k)
        {
            try
            {
                // ���Դӻ����л�ȡֵ
                return cache.Get(k);
            }
            catch (Exception)
            {
                // ��������쳣������ֵ���͵�Ĭ��ֵ
                return default(V);
            }
        }

        // ���������ڻ�ȡ�����д��ڵ�ָ������ֵ
        public V GetIfPresent(K k)
        {
            try
            {
                // ���Ի�ȡ�����д��ڵ�ָ������ֵ
                return cache.GetIfPresent(k);
            }
            catch (Exception)
            {
                // ��������쳣������ֵ���͵�Ĭ��ֵ
                return default(V);
            }
        }

        // ���������ڽ�ָ���ļ�ֵ�Դ��뻺��
        public void Put(K k, V v)
        {
            // ���û���� Set �����洢��ֵ��
            cache.Set(k, v);
        }

        // ���������ڴӻ������Ƴ�ָ�����Ļ�����
        public void Remove(K k)
        {
            // ���û���� Invalidate ����ʹָ�����Ļ�����ʧЧ
            cache.Invalidate(k);
        }

        // �ܱ����ķ�������Ϊ���ݼ��������������в�����ָ������ֵʱ����
        protected V DataLoader(K key)
        {
            // ���ó��󷽷� loadFromDb �����ݿ��������
            return loadFromDb(key);
        }

        // ���󷽷���Ҫ������ʵ�ִ����ݿ����ָ������Ӧ��ֵ���߼�
        protected abstract V loadFromDb(K key);

        // ���ⷽ������������Ƴ�ʱ���ã�������������д��ʵ���Զ����߼�
        public virtual void onRemoval(CacheEntryRemovedArguments arguments)
        {

        }
    }
}