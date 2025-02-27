using Framework.Core.Utils;
using System.Reflection;

namespace Framework.Core.Serializer
{
    /// <summary>
    /// 抽象类 Message，作为消息类的基类，提供获取消息模块、命令以及生成消息唯一键的方法。
    /// 该类利用反射机制从消息类的自定义特性 MessageMeta 中提取模块和命令信息。
    /// 由于是抽象类，不能直接实例化，需由具体的消息类继承使用。
    /// </summary>
    public abstract class Message
    {
        /// <summary>
        /// 获取消息所属的模块编号。
        /// 通过反射获取当前消息类的 MessageMeta 特性，若特性存在则返回其中的模块编号，否则返回 0。
        /// </summary>
        /// <returns>消息的模块编号。</returns>
        public short GetModule()
        {
            // 获取当前消息类的 MessageMeta 特性
            MessageMeta attribute = GetType().GetCustomAttribute<MessageMeta>();
            // 若特性存在，则返回其中的模块编号
            if (attribute != null)
            {
                return attribute.module;
            }
            // 若特性不存在，返回默认值 0
            return 0;
        }


        /// <summary>
        /// 获取消息的命令编号。
        /// 通过反射获取当前消息类的 MessageMeta 特性，若特性存在则返回其中的命令编号，否则返回 0。
        /// </summary>
        /// <returns>消息的命令编号。</returns>
        public short GetCmd()
        {
            // 获取当前消息类的 MessageMeta 特性
            MessageMeta attribute = GetType().GetCustomAttribute<MessageMeta>();
            // 若特性存在，则返回其中的命令编号
            if (attribute != null)
            {
                return attribute.cmd;
            }
            // 若特性不存在，返回默认值 0
            return 0;
        }

        /// <summary>
        /// 生成消息的唯一键。
        /// 该键由消息的模块编号和命令编号组合而成，格式为 "模块编号_命令编号"。
        /// </summary>
        /// <returns>消息的唯一键。</returns>
        public string key()
        {
            // 将模块编号和命令编号组合成唯一键
            return GetModule() + "_" + GetCmd();
        }
    }
}