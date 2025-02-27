using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Core.Cache {
    public class LRUCache<K, V> {
        private const int DEFAULT_CAPACITY = 255; // Ĭ�ϻ����������;

        private readonly MemoryCache cache;
        private long _capacity;//�������� 

        private ReaderWriterLockSlim _locker;

        private LinkedList<CacheData<K>> _linkedList; // ÿ����¼�������и�cache data;

        private CacheEntryRemovedCallback removedCallback; // ������MemoryCacheɾ��һ����¼��ʱ�򣬾ͻ�ص�;



        public LRUCache(CacheBuilder<K, V> cacheBuilder) {
            this._locker = new ReaderWriterLockSlim();
            this.cache = new MemoryCache(Guid.NewGuid().ToString());

            this._capacity = cacheBuilder.InitCapacity > 0 ? cacheBuilder.InitCapacity : DEFAULT_CAPACITY;
            this._linkedList = new LinkedList<CacheData<K>>();

            this.removedCallback += AfterCacheRemove;
            if (cacheBuilder.removalListener != null) {
                removedCallback += cacheBuilder.removalListener;
            }


        }

        public void Set(K key, V value, CacheItemPolicy cacheItemPolicy) {
            _locker.EnterWriteLock();
            try {
                string hashCode = key.GetHashCode().ToString();
                if (cache.Contains(hashCode)) { // ֮ǰ�Ѿ����ˣ����Ծ�Ҫ����������ԣ������ŵ�ǰ��
                    //�첽����LRU˳���
                    Task.Run(() =>
                    {
                        var linkCache = _linkedList.FirstOrDefault(p => p.Key == hashCode);
                        if (linkCache != null)
                        {
                            LRUReSort(linkCache, true);
                        }
                    });
                }
                else
                { // ����һ���µ� CacheData,���뵽��ǰ��;
                    var cacheData = new CacheData<K>() { Original = key, Key = hashCode, AccessTime = DateTime.Now, WriteTime = DateTime.Now };
                    _linkedList.AddFirst(cacheData);
                }

                cacheItemPolicy.RemovedCallback = this.removedCallback;
                this.cache.Set(hashCode, value, cacheItemPolicy);

                // ��̭��̫�ò��õ�;
                if (_linkedList.Count > _capacity)
                {
                    cache.Remove(_linkedList.Last.Value.Key);
                }
            }
            finally { _locker.ExitWriteLock(); }
        }


        private void LRUReSort(CacheData<K> linkCache, bool write = false) {
            lock (_linkedList) {
                //LRU����
                _linkedList.Remove(linkCache);
                //�޸ķ���ʱ��
                linkCache.AccessTime = DateTime.Now;
                if (write) {
                    linkCache.WriteTime = DateTime.Now;
                }
                _linkedList.AddFirst(linkCache);
            }
        }


        private void AfterCacheRemove(CacheEntryRemovedArguments arguments) {
            var key = arguments.CacheItem.Key;
            if (!this.cache.Contains(key))
            {
                lock (_linkedList)
                {
                    var linkCache = _linkedList.FirstOrDefault(p => p.Key == key);
                    if (linkCache != null)
                    {
                        _linkedList.Remove(linkCache);
                    }
                }
            }
        }

        public Tuple<V, DateTime, DateTime> Get(K key) {
            _locker.EnterUpgradeableReadLock();
            try {
                DateTime readTime = DateTime.MinValue;
                DateTime writeTime = DateTime.MinValue;
                string hashCode = key.GetHashCode().ToString();
                var v = cache.Get(hashCode);

                var cacheData = _linkedList.FirstOrDefault(p => p.Key == hashCode);
                var pass = (v != null && cacheData != null);

                if (!pass) {
                    return new Tuple<V, DateTime, DateTime>(default(V), readTime, writeTime);
                }

                // ����Ҫ����һ�����readTime;
                cacheData.AccessTime = DateTime.Now;
                readTime = cacheData.AccessTime;
                writeTime = cacheData.WriteTime;

                
                _locker.EnterWriteLock();
                try
                {
                    LRUReSort(cacheData);
                }
                finally { _locker.ExitWriteLock(); }
                return new Tuple<V, DateTime, DateTime>((V)v, readTime, writeTime);
            }
            finally { _locker.ExitUpgradeableReadLock(); } // ����Ҫ����һ�£����Ƿ������ͷ������;
        }

        public bool Remove(K key) {

            // ע�⣬���ﲻ������ɾ�������ǣ�cacheɾ���ص���ʱ���ٰ�������������Ľڵ����ɾ��
            // AfterCacheRemove
            string hashCode = key.GetHashCode().ToString();
            return cache.Remove(hashCode) != null;


        }

        public bool ContainsKey(string key)
        {
            _locker.EnterReadLock();
            try
            {
                return cache.Contains(key);
            }
            finally { _locker.ExitReadLock(); }
        }

        public long Count
        {
            get
            {
                _locker.EnterReadLock();
                try
                {
                    return cache.GetCount();
                }
                finally { _locker.ExitReadLock(); }
            }
        }

        public long Capacity
        {
            get
            {
                _locker.EnterReadLock();
                try
                {
                    return _capacity;
                }
                finally { _locker.ExitReadLock(); }
            }
            set
            {
                _locker.EnterUpgradeableReadLock();
                try
                {
                    if (value > 0 && _capacity != value)
                    {
                        _locker.EnterWriteLock();
                        try
                        {
                            _capacity = value;
                            while (_linkedList.Count > _capacity)
                            {
                                var last = _linkedList.LastOrDefault();
                                if (last != null)
                                {
                                    Remove(last.Original);
                                }
                            }
                        }
                        finally { _locker.ExitWriteLock(); }
                    }
                }
                finally { _locker.ExitUpgradeableReadLock(); }
            }
        }

        private List<string> GetCacheKeys()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var entries = cache.GetType().GetField("_entries", flags).GetValue(cache);
            var cacheItems = entries as IDictionary;
            var keys = new List<string>();
            if (cacheItems == null)
            {
                return keys;
            }
            foreach (DictionaryEntry cacheItem in cacheItems)
            {
                keys.Add(cacheItem.Key.ToString());
            }
            return keys;
        }

        public ICollection<string> Keys
        {
            get
            {
                _locker.EnterReadLock();
                try
                {
                    return GetCacheKeys();
                }
                finally { _locker.ExitReadLock(); }
            }
        }


    }
}
