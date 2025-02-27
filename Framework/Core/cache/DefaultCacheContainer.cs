using System.Runtime.Caching;

namespace Framework.Core.Cache
{
    // 定义一个泛型类 DefaultCacheContainer，继承自 AbstractCacheContainer
    // 泛型参数 K 表示键的类型，V 表示值的类型
    public class DefaultCacheContainer<K, V> : AbstractCacheContainer<K, V>
    {
        // 声明一个 IPersistable 类型的私有字段，用于从持久化存储加载数据
        private IPersistable<K, V> loader = null;

        // 构造函数，接收一个 IPersistable 类型的加载器作为参数
        // 调用基类的构造函数进行初始化
        public DefaultCacheContainer(IPersistable<K, V> loader) : base()
        {
            // 将传入的加载器赋值给私有字段
            this.loader = loader;
        }

        // 重写基类的抽象方法 loadFromDb，用于从持久化存储加载数据
        protected override V loadFromDb(K key)
        {
            // 检查加载器是否为空
            if (this.loader != null)
            {
                // 如果加载器不为空，调用其 Load 方法加载指定键的数据
                return this.loader.Load(key);
            }

            // 如果加载器为空，返回值类型的默认值
            return default(V);
        }

        // 重写基类的 onRemoval 方法，当缓存项被移除时调用
        public override void onRemoval(CacheEntryRemovedArguments arguments)
        {
            // 当前该方法为空实现，可根据需求添加缓存项移除时的处理逻辑
        }
    }
}