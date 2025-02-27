using System;
using System.Runtime.Caching;

namespace Framework.Core.Cache {
    public class CacheBuilder<K, V> //where V : CacheDataTime
    {
        #region * filed
        /// <summary>
        /// Ĭ��ֵ
        /// </summary>
        public const int UNSET_INT = -1;
        private const int DEFAULT_INITIAL_CAPACITY = 16;
        //private const int DEFAULT_CONCURRENCY_LEVEL = 4;
        private const int DEFAULT_EXPIRATION_MILLISECONDS = 0;
        private const int DEFAULT_REFRESH_MILLISECONDS = 0;

        private int initCapacity;
        private long maxImunSize;
        private long expireAfterAccessMilliseconds;
        private long expireAfterWriteMilliseconds;
        private long refreshAfterWriteMilliseconds;
        private long refreshMilliseconds;
        private bool autoReSize = false;
        private bool autoRefresh = false;

        public CacheEntryRemovedCallback removalListener;

        /// <summary>
        /// ��ʼ������
        /// </summary>
        public int InitCapacity
        {
            get
            {
                return initCapacity == UNSET_INT ? DEFAULT_INITIAL_CAPACITY : initCapacity;
            }
        }

        /// <summary>
        /// �������
        /// </summary>
        public long MaxImumSize
        {
            get
            {
                return maxImunSize;
            }
        }

        /// <summary>
        /// ����������ָ����ʱ�����û�б�����д�ͻᱻ����
        /// </summary>
        public long ExpireAfterAccessMilliseconds
        {
            get
            {
                return expireAfterAccessMilliseconds == UNSET_INT ? DEFAULT_EXPIRATION_MILLISECONDS : expireAfterAccessMilliseconds;
            }
        }

        /// <summary>
        /// ����������ָ����ʱ�����û�и��¾ͻᱻ����
        /// </summary>
        public long ExpireAfterWriteMilliseconds
        {
            get
            {
                return expireAfterWriteMilliseconds == UNSET_INT ? DEFAULT_EXPIRATION_MILLISECONDS : expireAfterWriteMilliseconds;
            }
        }

        /// <summary>
        /// ����������һ�θ��²���֮��Ķ�ûᱻˢ��
        /// (ֻ��һ���߳�Э��ˢ�»��棬�����������߳�)
        /// </summary>
        /// <returns></returns>
        public long RefreshAfterWriteMilliseconds
        {
            get
            {
                return this.refreshAfterWriteMilliseconds == UNSET_INT ? DEFAULT_EXPIRATION_MILLISECONDS : refreshAfterWriteMilliseconds;
            }
        }

        ///// <summary>
        ///// ����������һ�θ��²���֮��Ķ�ûᱻˢ��
        ///// ����ʱ�����Զ�ˢ�»�����ƣ���ֵ����С�ڻ���ʱ�䣩
        ///// </summary>
        //public long RefreshMilliseconds
        //{
        //    get
        //    {
        //        return refreshMilliseconds == UNSET_INT ? DEFAULT_REFRESH_MILLISECONDS : refreshMilliseconds;
        //    }
        //}

        /// <summary>
        /// �Զ������ڴ��С����
        /// </summary>
        public bool AutoResize { get { return autoReSize; } }

        /// <summary>
        /// �Զ��������
        /// </summary>
        public bool AutoRefresh { get { return autoRefresh; } }
        #endregion

        /// <summary>
        /// ˽�л����캯��
        /// </summary>
        private CacheBuilder()
        {
            initCapacity = UNSET_INT;
            maxImunSize = UNSET_INT;
            expireAfterAccessMilliseconds = UNSET_INT;
            expireAfterWriteMilliseconds = UNSET_INT;
            refreshMilliseconds = UNSET_INT;
            refreshAfterWriteMilliseconds = UNSET_INT;
        }

        /// <summary>
        /// ��������
        /// </summary>
        /// <returns></returns>
        public static CacheBuilder<K, V> NewBuilder()
        {
            return new CacheBuilder<K, V>();
        }

        /// <summary>
        /// �����������(ע�����ǻ������)
        /// </summary>
        /// <param name="maxImumSize"></param>
        /// <returns></returns>
        public CacheBuilder<K, V> SetMaxImumSize(long maxImumSize)
        {
            this.maxImunSize = maxImumSize;
            return this;
        }

        /// <summary>
        /// ����������ָ����ʱ�����û�б�����д�ͻᱻ����
        /// (���û�����Чʱ�����ζ�Ż����Զ�ʧЧ�����ݼ���ʱ�������߳�)
        /// </summary>
        /// <param name="expireAfterAccess"></param>
        /// <returns></returns>
        public CacheBuilder<K, V> SetExpireAfterAccessMilliseconds(long expireAfterAccess)
        {
            this.expireAfterAccessMilliseconds = expireAfterAccess;
            return this;
        }

        /// <summary>
        /// ����������ָ����ʱ�����û�и��¾ͻᱻ����
        /// </summary>
        /// <param name="expireAfterWriteNanos"></param>
        /// <returns></returns>
        public CacheBuilder<K, V> SetExpireAfterWriteMilliseconds(long expireAfterWriteNanos)
        {
            this.expireAfterWriteMilliseconds = expireAfterWriteNanos;
            return this;
        }

        /// <summary>
        /// ����������һ�θ��²���֮��Ķ�ûᱻˢ��
        /// (ֻ��һ���߳�Э��ˢ�»��棬�����������̣߳�ֵӦ�ñ�ExpireAfterAccessС)
        /// </summary>
        /// <param name="refreshAfterWriteMilliseconds"></param>
        /// <returns></returns>
        public CacheBuilder<K, V> SetRefreshAfterWriteMilliseconds(long refreshAfterWriteMilliseconds)
        {
            this.refreshAfterWriteMilliseconds = refreshAfterWriteMilliseconds;
            return this;
        }

        ///// <summary>
        ///// ����ˢ��ʱ��
        ///// (ֻ��һ���߳�Э��ˢ�»��棬�����������߳�)
        ///// </summary>
        ///// <param name="refreshNanos"></param>
        ///// <returns></returns>
        //public CacheBuilder<V> SetRefreshMilliseconds(long refreshNanos)
        //{
        //    this.refreshMilliseconds = refreshNanos;
        //    return this;
        //}

        /// <summary>
        /// �����ڴ��С�Զ����� 
        /// </summary>
        /// <param name="autoReSize"></param>
        /// <returns></returns>
        public CacheBuilder<K, V> SetAutoResize(bool autoReSize)
        {
            this.autoReSize = autoReSize;
            return this;
        }

        /// <summary>
        /// �Զ����»���
        /// ����̨���£�
        /// </summary>
        /// <param name="autoReSize"></param>
        /// <returns></returns>
        public CacheBuilder<K, V> SetAutoRefresh(bool autoRefresh)
        {
            this.autoRefresh = autoRefresh;
            return this;
        }

        /// <summary>
        /// �����������
        /// </summary>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public LoadingCache<K, V> Build(Func<K, V> func)
        {
            return new LoadingCache<K, V>(this, func);
        }

        ///// <summary>
        ///// �����������
        ///// </summary>
        ///// <param name="key"></param>
        ///// <param name="func"></param>
        ///// <returns></returns>
        //public LoadingCache<V> Build(CacheLoader<V> cacheLoader)
        //{
        //    return new LoadingCache<V>(this, cacheLoader);
        //}

        public CacheBuilder<K, V> RemovalListener(CacheEntryRemovedCallback action)
        {
            removalListener = action;
            return this;
        }
    }
}

