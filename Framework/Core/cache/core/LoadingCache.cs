using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Timers;


namespace Framework.Core.Cache {

    public class LoadingCache<K, V> : ILoadingCache<K, V>
    {
        private readonly CacheBuilder<K, V> cacheBuilder;
        private LRUCache<K, V> lruCache;

        private Func<K, V> load; // ��cache����û�����ݵ�ʱ�򣬵����������ȥ����

        private bool lockState = false;

        private static int IntervalMillSeconed = 60 * 1000;

        public LoadingCache(CacheBuilder<K, V> cacheBuilder, Func<K, V> load) {
            this.cacheBuilder = cacheBuilder;
            this.load = load;
            this.lruCache = new LRUCache<K, V>(cacheBuilder);
            this.AutoReSize();
        }

        public V Get(K key)
        {
            var cache = GetCacheData(key);
            if (cache.Item1 != null && cache.Item2)
            {
                return cache.Item1;
            }

            if (cache.Item1 == null)
            {
                //ѹ��û����,ȫ������
                return GetCacheFromLoad(key);
            }

            if (cache.Item2)
            {
                return cache.Item1;
            }
            //�첽ˢ��δ���ã�ȫ�����������»�ȡ
            if (this.cacheBuilder.RefreshAfterWriteMilliseconds <= 0)
            {
                return GetCacheFromLoad(key);
            }

            //�첽ˢ�»���
            if (this.cacheBuilder.AutoRefresh)
            {
                //�Զ�ˢ�µģ�ֱ�Ӻ�̨�̴߳���ֱ�ӷ��ؾ�ֵ����
                Task.Run(() =>
                {
                    if (!lockState)
                    {
                        //Console.WriteLine("��ִ̨�л���ˢ�£���ǰ����ֵΪ��" + cache.Item1.ToString()+", �߳�Id��"+Task.CurrentId);
                        GetCacheFromLoad(key);
                    }
                });

                return cache.Item1;
            }

            //����һ���߳�ˢ�£������̲߳�����
            if (!lockState)
            {
                return GetCacheFromLoad(key);
            }

            return cache.Item1;
        }

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private V GetCacheFromLoad(K key)
        {
            V value = default(V);

            //��ʱ�򵥴��������˴���Ҫ����keyֵ��ͬ��ȡ��ͬ��
            lock (this)
            {
                lockState = true;
                value = GetIfPresent(key);
                if (null == value)
                {
                    value = load(key);
                    if (value != null)
                    {
                        Set(key, value);
                    }
                }
                lockState = false;
            }
            
            return value;
        }

        public Dictionary<K, V> GetAllPresent(List<K> keys)
        {
            var dic = new Dictionary<K, V>();

            foreach (var key in keys)
            {
                var value = GetIfPresent(key);
                if (value != null)
                {
                    dic.Add(key, value);
                }
            }

            return dic;
        }

        public V GetIfPresent(K key)
        {
            var cache = this.GetCacheData(key);
            if (cache.Item1 == null || !cache.Item2)
            {
                return default(V);
            }

            return cache.Item1;
        }

        private Tuple<V, bool> GetCacheData(K key)
        {
            var value = lruCache.Get(key);
            if (value.Item1 == null)
            {
                return new Tuple<V, bool>(default(V), false);
            }

            var accessT = (DateTime.Now - value.Item2).TotalMilliseconds;
            var writeT = (DateTime.Now - value.Item3).TotalMilliseconds;

            if (cacheBuilder.RefreshAfterWriteMilliseconds > 0 && cacheBuilder.RefreshAfterWriteMilliseconds < writeT)
            {
                //����ʧЧ
                return new Tuple<V, bool>(value.Item1, false);
            }

            //�����Ƿ���Ч
            var cacheValidate = (cacheBuilder.ExpireAfterAccessMilliseconds <= 0 || cacheBuilder.ExpireAfterAccessMilliseconds > accessT)
                && (cacheBuilder.ExpireAfterWriteMilliseconds <= 0 || cacheBuilder.ExpireAfterWriteMilliseconds > writeT);

            //��Ҫ����ʱ��ˢ�»���
            if (!cacheValidate)
            {
                //����ʧЧ���������
                lruCache.Remove(key);
                return new Tuple<V, bool>(default(V), false);
            }

            return new Tuple<V, bool>(value.Item1, true);
        }

        public void Invalidate(K key)
        {
            lruCache.Remove(key);
        }

        public void Set(K key, V value)
        {
            //���û���ʱ��
            var cacheItemPolicy = new CacheItemPolicy();

            var expireTime = Math.Max(cacheBuilder.ExpireAfterAccessMilliseconds, cacheBuilder.ExpireAfterWriteMilliseconds);
            //���û������ʱ��Ҫע�⣬���������ˢ��ʱ�䣬�򻺴治�ܹ��ڣ���Ϊֻ�е��̸߳��»���Ӱ�������̻߳�ȡ����
            if (this.cacheBuilder.RefreshAfterWriteMilliseconds <= 0 && expireTime > 0)
            {
                //���û���ʱ��
                cacheItemPolicy.SlidingExpiration = TimeSpan.FromMilliseconds(expireTime);
            }
            else
            {
                ////�������Զ�ˢ�»��棬����δ���û���ʱ�䣬�������ò�ʧЧ
                cacheItemPolicy.Priority = CacheItemPriority.NotRemovable;
            }
            lruCache.Set(key, value, cacheItemPolicy);
        }

        private void AutoReSize()
        {
            Task.Run(() =>
            {
                Timer timer = new Timer();
                timer.AutoReset = true;
                timer.Enabled = true;
                timer.Interval = IntervalMillSeconed;
                timer.Elapsed += Timer_Elapsed;
                timer.Start();
            });
        }

        /// ��ʱˢ��,����ڴ�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (this.cacheBuilder.AutoResize)
            {
                ///�����ڴ��С�����������
                //var phyCacheMemoryLimit = cache.PhysicalMemoryLimit;
                //var memoryLimit = cache.CacheMemoryLimit;

                var calSize = 100000; //����ɷ���ʹ���� todo ������

                this.lruCache.Capacity = calSize;
            }
        }
    }

}

