namespace Framework.Core.Cache
{
    // 定义一个泛型接口 IPersistable，K 表示键的类型，V 表示值的类型
    // 此接口用于规范从持久化存储加载数据的操作
    public interface IPersistable<K, V>
    {
        // 定义一个抽象方法 Load，用于从持久化存储中加载指定键 k 对应的值
        // 任何实现此接口的类都必须实现该方法，以提供具体的加载逻辑
        public V Load(K k);
    }
}