using System;
using System.Runtime.Caching;

namespace Framework.Core.Cache
{

    // 定义一个抽象类，使用泛型 K 作为键的类型，V 作为值的类型
    public abstract class AbstractCacheContainer<K, V>
    {
        // 声明一个 LoadingCache 类型的私有字段，用于存储缓存项
        private LoadingCache<K, V> cache = null;

        // 构造函数，在实例化时初始化缓存
        public AbstractCacheContainer()
        {
            // 创建一个 CacheBuilder 实例，用于构建缓存
            var builder = CacheBuilder<K, V>.NewBuilder();
            // 设置缓存项在最后一次访问后 18000000 毫秒（即 5 小时）过期
            builder.SetExpireAfterAccessMilliseconds(18000000);
            // 设置缓存项在写入后 18000000 毫秒（即 5 小时）过期
            builder.SetExpireAfterWriteMilliseconds(18000000);
            // 注册缓存项移除监听器，当缓存项被移除时调用 onRemoval 方法
            builder.RemovalListener(this.onRemoval);

            // 使用 DataLoader 方法作为数据加载器，构建缓存实例
            this.cache = builder.Build(DataLoader);
        }

        // 虚拟方法，用于从缓存中获取指定键的值
        public virtual V Get(K k)
        {
            try
            {
                // 尝试从缓存中获取值
                return cache.Get(k);
            }
            catch (Exception)
            {
                // 如果出现异常，返回值类型的默认值
                return default(V);
            }
        }

        // 方法，用于获取缓存中存在的指定键的值
        public V GetIfPresent(K k)
        {
            try
            {
                // 尝试获取缓存中存在的指定键的值
                return cache.GetIfPresent(k);
            }
            catch (Exception)
            {
                // 如果出现异常，返回值类型的默认值
                return default(V);
            }
        }

        // 方法，用于将指定的键值对存入缓存
        public void Put(K k, V v)
        {
            // 调用缓存的 Set 方法存储键值对
            cache.Set(k, v);
        }

        // 方法，用于从缓存中移除指定键的缓存项
        public void Remove(K k)
        {
            // 调用缓存的 Invalidate 方法使指定键的缓存项失效
            cache.Invalidate(k);
        }

        // 受保护的方法，作为数据加载器，当缓存中不存在指定键的值时调用
        protected V DataLoader(K key)
        {
            // 调用抽象方法 loadFromDb 从数据库加载数据
            return loadFromDb(key);
        }

        // 抽象方法，要求子类实现从数据库加载指定键对应的值的逻辑
        protected abstract V loadFromDb(K key);

        // 虚拟方法，当缓存项被移除时调用，可在子类中重写以实现自定义逻辑
        public virtual void onRemoval(CacheEntryRemovedArguments arguments)
        {

        }
    }
}