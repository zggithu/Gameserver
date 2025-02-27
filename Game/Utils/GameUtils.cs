using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Game.Utils
{
    /// <summary>
    /// 游戏工具类，提供了一些通用的工具方法，用于处理随机数、字符串解析、ProtoBuf 序列化和反序列化以及反射操作等。
    /// </summary>
    public class GameUtils
    {
        /// <summary>
        /// 生成指定范围内的随机整数。
        /// </summary>
        /// <param name="min">随机数的最小值（包含）。</param>
        /// <param name="max">随机数的最大值（不包含）。</param>
        /// <returns>生成的随机整数。</returns>
        public static int Random(int min, int max) {
            // 创建一个随机数生成器实例
            Random random = new Random();
            // 生成指定范围内的随机整数
            int s = random.Next(max) % (max - min + 1) + min;
            return s;
        }

        /// <summary>
        /// 解析格式为 "key1=value1|key2=value2" 的字符串，将其转换为键值对字典。
        /// </summary>
        /// <param name="formatString">要解析的字符串。</param>
        /// <returns>包含解析后键值对的字典，键为字符串类型，值为整数类型。</returns>
        public static Dictionary<string, int> ParseStringWithKeyIntValue(string formatString) {
            // 初始化一个空的字典用于存储解析后的键值对
            Dictionary<string, int> props = new Dictionary<string, int>();
            // 使用 '|' 分割字符串，得到多个键值对字符串
            string[] conds = formatString.Split('|');
            for (int i = 0; i < conds.Length; i++) {
                // 使用 '=' 分割每个键值对字符串，得到键和值
                string[] keyAndValue = conds[i].Split('=');
                string key = keyAndValue[0];
                // 将值转换为整数类型
                int value = int.Parse(keyAndValue[1]);

                // 将键值对添加到字典中
                props.Add(key, value);
            }

            return props;
        }

        /// <summary>
        /// 将 ProtoBuf 对象序列化为字节数组。
        /// </summary>
        /// <typeparam name="T">要序列化的对象类型。</typeparam>
        /// <param name="objectData">要序列化的对象实例。</param>
        /// <returns>序列化后的字节数组。</returns>
        public static byte[] PbObjToBytes<T>(T objectData) {
            // 创建一个内存流用于存储序列化后的数据
            var stream = new MemoryStream();
            // 使用 ProtoBuf 序列化器将对象序列化为流
            Serializer.Serialize<T>(stream, (T)objectData);
            // 将流中的数据转换为字节数组
            byte[] data = stream.ToArray();
            return data;
        }

        /// <summary>
        /// 将字节数组反序列化为 ProtoBuf 对象。
        /// </summary>
        /// <typeparam name="T">要反序列化的对象类型。</typeparam>
        /// <param name="data">要反序列化的字节数组。</param>
        /// <returns>反序列化后的对象实例。</returns>
        public static T BytesToPbObj<T>(byte[] data) {
            // 创建一个内存流并将字节数组写入流中
            var stream = new MemoryStream(data);
            // 使用 ProtoBuf 序列化器从流中反序列化对象
            return Serializer.Deserialize<T>(stream);
        }

        /// <summary>
        /// 使用反射获取对象的字段值。
        /// </summary>
        /// <param name="inst">要获取字段值的对象实例。</param>
        /// <param name="filedName">字段的名称。</param>
        /// <param name="defaultObj">如果字段不存在，返回的默认值。</param>
        /// <returns>字段的值，如果字段不存在则返回默认值。</returns>
        public static object GetFiled(object inst, string filedName, object defaultObj) {
            // 获取对象的类型
            Type t = inst.GetType();
            // 获取指定名称的字段信息
            FieldInfo f = t.GetField(filedName);
            if (f == null) {
                // 如果字段不存在，返回默认值
                return defaultObj;
            }

            // 获取字段的值
            return f.GetValue(inst);
        }

        /// <summary>
        /// 使用反射设置对象的字段值。
        /// </summary>
        /// <param name="inst">要设置字段值的对象实例。</param>
        /// <param name="filedName">字段的名称。</param>
        /// <param name="value">要设置的字段值。</param>
        public static void SetFiled(object inst, string filedName, object value) {
            // 获取对象的类型
            Type t = inst.GetType();
            // 获取指定名称的字段信息
            FieldInfo f = t.GetField(filedName);
            if (f == null) {
                // 如果字段不存在，直接返回
                return;
            }

            // 设置字段的值
            f.SetValue(inst, value);
        }

        /// <summary>
        /// 使用反射获取对象的属性值。
        /// </summary>
        /// <param name="inst">要获取属性值的对象实例。</param>
        /// <param name="propName">属性的名称。</param>
        /// <param name="defaultObj">如果属性不存在，返回的默认值。</param>
        /// <returns>属性的值，如果属性不存在则返回默认值。</returns>
        public static object GetProp(object inst, string propName, object defaultObj) {
            // 获取对象的类型
            Type t = inst.GetType();
            // 获取指定名称的属性信息
            PropertyInfo prop = t.GetProperty(propName);
            if (prop == null) {
                // 如果属性不存在，返回默认值
                return defaultObj;
            }

            // 获取属性的值
            return prop.GetValue(inst);
        }

        /// <summary>
        /// 使用反射设置对象的属性值。
        /// </summary>
        /// <param name="inst">要设置属性值的对象实例。</param>
        /// <param name="propName">属性的名称。</param>
        /// <param name="value">要设置的属性值。</param>
        public static void SetProp(object inst, string propName, object value) {
            // 获取对象的类型
            Type t = inst.GetType();
            // 获取指定名称的属性信息
            PropertyInfo prop = t.GetProperty(propName);
            if (prop == null) {
                // 如果属性不存在，直接返回
                return;
            }

            // 设置属性的值
            prop.SetValue(inst, value);
        }
    }
}