using Framework.Core.Utils;
using Game.Datas.Excels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core.Configs
{
    /// <summary>
    /// 配置管理类，负责游戏配置的初始化和管理。
    /// 采用单例模式，确保整个应用程序中只有一个实例。
    /// </summary>
    public class ConfigMgr
    {
        // 单例实例，通过静态属性提供全局访问
        public static ConfigMgr Instance = new ConfigMgr();

        // 标识是否使用 Redis 的布尔变量，默认为 false
        public bool useRedis = false;

        /// <summary>
        /// 初始化配置管理器的方法。
        /// 读取配置文件中的 useRedis 配置项，并提前加载多个 Excel 配置表的数据。
        /// </summary>
        public void Init()
        {
            // 从配置文件的 AppSettings 中读取 useRedis 配置项，并将其转换为布尔值
            // 若配置文件中未找到该配置项或转换失败，可能会抛出异常
            this.useRedis = bool.Parse(ConfigurationManager.AppSettings["useRedis"]);

            // 提前加载读出来所有的 Excel 表
            // 调用 ExcelUtils 类的 ReadConfigData 方法，分别加载不同类型的 Excel 配置数据
            // 这些方法会将 Excel 数据读取并存储到相应的缓存或数据结构中，方便后续使用
            ExcelUtils.ReadConfigData<BonuesRuleA>();
            ExcelUtils.ReadConfigData<CollectTask>();
            ExcelUtils.ReadConfigData<ExchangeTrading>();
            ExcelUtils.ReadConfigData<OrderTrading>();
        }
    }
}