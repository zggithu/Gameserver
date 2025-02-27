using System;
using System.Reflection;

namespace Framework.Core.Net
{

    public class CmdExecutor
    {
        /** logic controller  */
        // 存储逻辑控制器对象，该对象包含具体的业务逻辑处理方法
        public object handler;

        /** logic handler method */
        // 存储逻辑处理方法的信息，通过反射可以调用该方法
        public MethodInfo method;

        /** arguments passed to method */
        // 存储传递给处理方法的参数类型数组，用于确定方法调用时的参数
        public Type[] @params;

        // 静态方法，用于创建 CmdExecutor 实例
        public static CmdExecutor Create(MethodInfo method, Type[] @params, object handler)
        {
            // 创建一个新的 CmdExecutor 实例
            CmdExecutor executor = new CmdExecutor();
            // 将传入的处理方法信息赋值给实例的 method 属性
            executor.method = method;
            // 将传入的参数类型数组赋值给实例的 @params 属性
            executor.@params = @params;
            // 将传入的逻辑控制器对象赋值给实例的 handler 属性
            executor.handler = handler;

            // 返回创建好的 CmdExecutor 实例
            return executor;
        }
    }
}