namespace Framework.Core.Cache
{

    // 注意，不同的Cache存入同一个对象，内部会copy一个对象实例出来;
    // 定义一个抽象类 BaseCacheSerivce，使用泛型 K 作为键的类型，V 作为值的类型
    // 该类实现了 IPersistable<K, V> 接口
    public abstract class BaseCacheSerivce<K, V> : IPersistable<K, V>
    {
        // 声明一个 AbstractCacheContainer 类型的私有字段，用于存储缓存项
        AbstractCacheContainer<K, V> container = null;

        // 构造函数，在实例化时初始化缓存容器
        public BaseCacheSerivce()
        {
            // 创建一个 DefaultCacheContainer 实例，并传入当前对象作为参数
            this.container = new DefaultCacheContainer<K, V>(this);
        }

        // 抽象方法，要求子类实现从持久化存储（如数据库）中加载指定键对应的值的逻辑
        public abstract V Load(K k);

        // 方法，用于从缓存中获取指定键的值
        public V Get(K key)
        {
            // 调用缓存容器的 Get 方法获取值
            return this.container.Get(key);
        }

        // 方法，用于获取缓存中存在的指定键的值
        public V GetIfPresent(K k)
        {
            // 调用缓存容器的 GetIfPresent 方法获取值
            return this.container.GetIfPresent(k);
        }

        // 方法，用于将指定的键值对存入缓存
        public void Put(K key, V v)
        {
            // 调用缓存容器的 Put 方法存储键值对
            this.container.Put(key, v);
        }

        // 方法，用于从缓存中移除指定键的缓存项
        public void Remove(K key)
        {
            // 调用缓存容器的 Remove 方法使指定键的缓存项失效
            this.container.Remove(key);
        }
    }
}