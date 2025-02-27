using System.Runtime.Caching;

namespace Framework.Core.Cache
{
    // ����һ�������� DefaultCacheContainer���̳��� AbstractCacheContainer
    // ���Ͳ��� K ��ʾ�������ͣ�V ��ʾֵ������
    public class DefaultCacheContainer<K, V> : AbstractCacheContainer<K, V>
    {
        // ����һ�� IPersistable ���͵�˽���ֶΣ����ڴӳ־û��洢��������
        private IPersistable<K, V> loader = null;

        // ���캯��������һ�� IPersistable ���͵ļ�������Ϊ����
        // ���û���Ĺ��캯�����г�ʼ��
        public DefaultCacheContainer(IPersistable<K, V> loader) : base()
        {
            // ������ļ�������ֵ��˽���ֶ�
            this.loader = loader;
        }

        // ��д����ĳ��󷽷� loadFromDb�����ڴӳ־û��洢��������
        protected override V loadFromDb(K key)
        {
            // ���������Ƿ�Ϊ��
            if (this.loader != null)
            {
                // �����������Ϊ�գ������� Load ��������ָ����������
                return this.loader.Load(key);
            }

            // ���������Ϊ�գ�����ֵ���͵�Ĭ��ֵ
            return default(V);
        }

        // ��д����� onRemoval ��������������Ƴ�ʱ����
        public override void onRemoval(CacheEntryRemovedArguments arguments)
        {
            // ��ǰ�÷���Ϊ��ʵ�֣��ɸ���������ӻ������Ƴ�ʱ�Ĵ����߼�
        }
    }
}