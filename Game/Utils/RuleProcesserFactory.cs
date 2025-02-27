using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Game.Utils
{
    /// <summary>
    /// RuleModule 特性，用于标记包含规则处理方法的类。
    /// 该特性只能应用于类，且每个类只能有一个该特性，可被继承。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class RuleModule : Attribute
    {
        /// <summary>
        /// 模块编号，用于标识不同的规则模块。
        /// </summary>
        public int module;
        /// <summary>
        /// 基础编号，用于计算子类型的键值。
        /// </summary>
        public int baseId;

        /// <summary>
        /// 构造函数，初始化模块编号和基础编号。
        /// </summary>
        /// <param name="module">模块编号。</param>
        /// <param name="baseId">基础编号。</param>
        public RuleModule(int module, int baseId) {
            this.module = module;
            this.baseId = baseId;
        }
    }

    /// <summary>
    /// RuleProcesser 特性，用于标记具体的规则处理方法。
    /// 该特性只能应用于方法，且每个方法只能有一个该特性，可被继承。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RuleProcesser : Attribute
    {
        /// <summary>
        /// 子类型编号，用于区分同一模块下不同的规则处理方法。
        /// </summary>
        public int subType;
        /// <summary>
        /// 方法名称，用于在查找规则处理方法时使用。
        /// </summary>
        public string funcName;

        /// <summary>
        /// 构造函数，初始化方法名称和子类型编号。
        /// </summary>
        /// <param name="name">方法名称。</param>
        /// <param name="subType">子类型编号。</param>
        public RuleProcesser(string name, int subType) {
            this.funcName = name;
            this.subType = subType;
        }
    }

    /// <summary>
    /// RuleProcesserFactory 类，用于扫描程序集中带有 RuleModule 和 RuleProcesser 特性的类和方法，
    /// 并提供方法根据模块、方法名和类型 ID 查找对应的规则处理方法。
    /// </summary>
    public class RuleProcesserFactory
    {
        /* 字典结构
         * Dic<int, Dic<int, Dic<string, Method>>>
         * [任务]====>[200001] ===> "Func1", Func1
         *                     ===> "Func2", Func2
         *                     ===> "Func3", Func3
         *             [200000 -1] ===> "Func1", Func1
         *      
         * [奖励]=====>[200001] ===> "Func1", Func1
         *                     ===> "Func2", Func2
         *                     ===> "Func3", Func3
         *             [200000 -1] ===> "Func1", Func1
         * ....
         */
        // 静态字典，用于存储扫描到的规则处理方法
        static Dictionary<int, Dictionary<int, Dictionary<string, MethodInfo>>> ruleSet;

        /// <summary>
        /// 初始化方法，调用 ScanRuleProcesser 方法扫描程序集中的规则处理方法，并将结果存储在 ruleSet 中。
        /// </summary>
        public static void Init() {
            ruleSet = ScanRuleProcesser();
        }

        /// <summary>
        /// 扫描单个带有 RuleModule 特性的类，将其中带有 RuleProcesser 特性的方法添加到规则集合中。
        /// </summary>
        /// <param name="ruleSet">规则集合，用于存储扫描到的规则处理方法。</param>
        /// <param name="t">要扫描的类的类型。</param>
        /// <param name="mainRule">类上的 RuleModule 特性实例。</param>
        private static void ScaneOneRule(Dictionary<int, Dictionary<int, Dictionary<string, MethodInfo>>> ruleSet, Type t, RuleModule mainRule) {
            // 存储当前模块的规则处理方法集合
            Dictionary<int, Dictionary<string, MethodInfo>> oneRuleSet = null;
            if (!ruleSet.ContainsKey(mainRule.module)) {
                // 如果规则集合中不包含当前模块，则创建一个新的集合
                oneRuleSet = new Dictionary<int, Dictionary<string, MethodInfo>>();
                ruleSet.Add(mainRule.module, oneRuleSet);
            } else {
                // 如果规则集合中已包含当前模块，则获取对应的集合
                oneRuleSet = ruleSet[mainRule.module];
            }

            // 获取类中所有的静态、公共和非公共方法
            MethodInfo[] funcs = t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            for (int j = 0; j < funcs.Length; j++) {
                // 获取方法上的 RuleProcesser 特性实例
                RuleProcesser p = funcs[j].GetCustomAttribute<RuleProcesser>();
                if (p == null) {
                    // 如果方法上没有 RuleProcesser 特性，则跳过该方法
                    continue;
                }

                // 计算子类型的键值
                int subType = (p.subType == -1) ? -1 : (p.subType % mainRule.baseId);
                int key = mainRule.baseId + subType;

                // 存储当前子类型的规则处理方法集合
                Dictionary<string, MethodInfo> processFuncs = null;
                if (!oneRuleSet.ContainsKey(key)) {
                    // 如果当前模块的规则处理方法集合中不包含当前子类型，则创建一个新的集合
                    processFuncs = new Dictionary<string, MethodInfo>();
                    oneRuleSet.Add(key, processFuncs);
                } else {
                    // 如果当前模块的规则处理方法集合中已包含当前子类型，则获取对应的集合
                    processFuncs = oneRuleSet[key];
                }

                // 将方法添加到当前子类型的规则处理方法集合中
                processFuncs.Add(p.funcName, funcs[j]);
            }
        }

        /// <summary>
        /// 扫描程序集中所有带有 RuleModule 特性的类，并调用 ScaneOneRule 方法处理每个类。
        /// </summary>
        /// <returns>包含所有规则处理方法的规则集合。</returns>
        public static Dictionary<int, Dictionary<int, Dictionary<string, MethodInfo>>> ScanRuleProcesser() {
            // 初始化规则集合
            Dictionary<int, Dictionary<int, Dictionary<string, MethodInfo>>> ruleSet = new Dictionary<int, Dictionary<int, Dictionary<string, MethodInfo>>>();
            // 获取当前应用程序域中的所有程序集
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies) {
                // 获取程序集中的所有类型
                Type[] allTypes = assembly.GetTypes();
                for (int i = 0; i < allTypes.Length; i++) {
                    Type t = allTypes[i];
                    // 获取类型上的 RuleModule 特性实例
                    RuleModule mainRule = t.GetCustomAttribute<RuleModule>();
                    if (mainRule != null) {
                        // 如果类型上有 RuleModule 特性，则调用 ScaneOneRule 方法处理该类型
                        ScaneOneRule(ruleSet, t, mainRule);
                    }
                }
            }
            return ruleSet;
        }

        /// <summary>
        /// 根据方法名从指定的规则处理方法集合和默认规则处理方法集合中查找对应的方法。
        /// </summary>
        /// <param name="funcName">要查找的方法名。</param>
        /// <param name="funMap">规则处理方法集合。</param>
        /// <param name="defaultFunMap">默认规则处理方法集合。</param>
        /// <returns>找到的方法信息，如果未找到则返回 null。</returns>
        private static MethodInfo GetProcesserFunc(string funcName, Dictionary<string, MethodInfo> funMap, Dictionary<string, MethodInfo> defaultFunMap) {
            if (funMap.ContainsKey(funcName)) {
                // 如果规则处理方法集合中包含该方法名，则返回对应的方法信息
                return funMap[funcName];
            }
            if (defaultFunMap.ContainsKey(funcName)) {
                // 如果默认规则处理方法集合中包含该方法名，则返回对应的方法信息
                return defaultFunMap[funcName];
            }

            // 如果都未找到，则返回 null
            return null;
        }

        /// <summary>
        /// 根据模块、方法名、类型 ID 和基础编号查找对应的规则处理方法。
        /// </summary>
        /// <param name="module">模块编号。</param>
        /// <param name="name">方法名。</param>
        /// <param name="tid">类型 ID。</param>
        /// <param name="baseId">基础编号。</param>
        /// <returns>找到的方法信息，如果未找到则返回 null。</returns>
        public static MethodInfo GetProcesser(int module, string name, int tid, int baseId) {
            // 计算默认键值
            int key = baseId;
            key = key - 1;

            // 存储当前模块的规则处理方法集合
            Dictionary<int, Dictionary<string, MethodInfo>> oneRuleSet = null;
            if (!ruleSet.ContainsKey(module)) {
                // 如果规则集合中不包含当前模块，则返回 null
                return null;
            }

            // 获取当前模块的规则处理方法集合
            oneRuleSet = ruleSet[module];
            // 存储当前类型 ID 的规则处理方法集合
            Dictionary<string, MethodInfo> funMap = null;
            // 存储默认规则处理方法集合
            Dictionary<string, MethodInfo> defaultFunMap = null;

            if (oneRuleSet.ContainsKey(key)) {
                // 如果当前模块的规则处理方法集合中包含默认键值，则获取对应的默认规则处理方法集合
                defaultFunMap = oneRuleSet[key];
            }

            if (oneRuleSet.ContainsKey(tid)) {
                // 如果当前模块的规则处理方法集合中包含当前类型 ID，则获取对应的规则处理方法集合
                funMap = oneRuleSet[tid];
            } else {
                // 如果不包含当前类型 ID，则使用默认规则处理方法集合
                funMap = defaultFunMap;
            }
            if (funMap == null) {
                // 如果规则处理方法集合为空，则返回 null
                return null;
            }

            // 调用 GetProcesserFunc 方法查找对应的方法信息
            MethodInfo actionFunc = GetProcesserFunc(name, funMap, defaultFunMap);
            return actionFunc;
        }
    }
}