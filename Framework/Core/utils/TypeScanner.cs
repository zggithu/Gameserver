using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Framework.Core.Utils
{
    /// <summary>
    /// 类型扫描器类，提供在当前执行程序集中扫描特定类型的功能。
    /// </summary>
    public class TypeScanner
    {
        /// <summary>
        /// 列出当前执行程序集中所有继承自指定父类型的子类型。
        /// </summary>
        /// <param name="parent">要查找子类型的父类型。</param>
        /// <returns>一个包含所有子类型的可枚举集合。</returns>
        public static IEnumerable<Type> ListAllSubTypes(Type parent)
        {
            // 获取当前执行的程序集
            // 调用 GetTypes 方法获取程序集中的所有类型
            // 使用 Where 方法筛选出所有是指定父类型子类的类型
            return Assembly.GetExecutingAssembly().GetTypes().Where(type => type.IsSubclassOf(parent));
        }

        /// <summary>
        /// 列出当前执行程序集中所有实现了指定接口的类型，但不包括接口本身。
        /// </summary>
        /// <param name="_interface">要查找实现类型的接口。</param>
        /// <returns>一个包含所有实现该接口类型的可枚举集合。</returns>
        public static IEnumerable<Type> ListAllAchieveInterface(Type _interface)
        {
            // 获取当前执行的程序集
            // 调用 GetTypes 方法获取程序集中的所有类型
            // 使用 Where 方法筛选出所有可以赋值给指定接口且不等于该接口本身的类型
            return Assembly.GetExecutingAssembly()
              .GetTypes()
              .Where(type => type.IsAssignableTo(_interface) && type != _interface);
        }

        /// <summary>
        /// 列出当前执行程序集中所有应用了指定特性的类型。
        /// </summary>
        /// <param name="attribute">要查找应用类型的特性。</param>
        /// <returns>一个包含所有应用了该特性类型的可枚举集合。</returns>
        public static IEnumerable<Type> ListTypesWithAttribute(Type attribute)
        {
            // 获取当前执行的程序集
            // 调用 GetTypes 方法获取程序集中的所有类型
            // 使用 Where 方法筛选出所有应用了指定特性的类型
            return Assembly.GetExecutingAssembly()
              .GetTypes()
              .Where(type => type.IsDefined(attribute));
        }
    }
}