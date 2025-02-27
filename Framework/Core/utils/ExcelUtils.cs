using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.IO;

/*
*   主键: key, ID, 如果没有用索引，从0开始
*/

namespace Framework.Core.Utils
{
    /// <summary>
    /// 该类提供了读取 CSV 格式配置文件并将其数据映射到指定类型对象的功能。
    /// 支持根据键获取单条配置数据，获取指定文件的所有配置数据列表，以及读取配置文件并存储数据。
    /// </summary>
    public class ExcelUtils
    {
        // 使用 NLog 记录日志，获取当前类的日志记录器
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        // 静态字典，用于存储从 CSV 文件中读取的数据
        // 外层字典的键为文件名，值为内层字典
        // 内层字典的键为数据的主键或索引，值为对应的对象
        static Dictionary<string, Dictionary<string, object>> dataDic = new Dictionary<string, Dictionary<string, object>>();

        /// <summary>
        /// 根据指定的键和文件名获取配置数据对象。
        /// </summary>
        /// <typeparam name="T">配置数据对象的类型。</typeparam>
        /// <param name="key">要获取的数据的键。</param>
        /// <param name="fileName">CSV 文件名，若为 null 则使用类型 T 的名称。</param>
        /// <returns>配置数据对象，若未找到则返回 null。</returns>
        public static object GetConfigData<T>(string key, string fileName = null)
        {
            // 获取类型 T 的 Type 对象
            Type setT = typeof(T);
            // 若文件名未指定，则使用类型 T 的名称作为文件名
            if (fileName == null)
            {
                fileName = setT.Name;
            }

            // 若 dataDic 中不包含该文件名的数据，则读取数据
            if (!dataDic.ContainsKey(fileName))
            {
                ReadConfigData<T>(fileName);
            }

            // 获取该文件名对应的对象字典
            Dictionary<string, object> objDic = dataDic[fileName];
            // 若对象字典中不包含指定的键，则返回 null
            if (!objDic.ContainsKey(key))
            {
                return null;
            }
            // 返回指定键对应的对象
            return objDic[key];
        }

        /// <summary>
        /// 获取指定文件名的所有配置数据对象列表。
        /// </summary>
        /// <typeparam name="T">配置数据对象的类型。</typeparam>
        /// <param name="fileName">CSV 文件名，若为 null 则使用类型 T 的名称。</param>
        /// <param name="isPriKey">是否使用主键，默认为 true。</param>
        /// <returns>配置数据对象列表。</returns>
        public static List<T> GetConfigDatas<T>(string fileName, bool isPriKey = true)
        {
            // 用于存储返回的配置数据对象列表
            List<T> returnList = new List<T>();
            // 获取类型 T 的 Type 对象
            Type setT = typeof(T);
            // 若文件名未指定，则使用类型 T 的名称作为文件名
            if (fileName == null)
            {
                fileName = setT.Name;
            }

            // 若 dataDic 中不包含该文件名的数据，则读取数据
            if (!dataDic.ContainsKey(fileName))
            {
                ReadConfigData<T>(fileName, isPriKey);
            }

            // 获取该文件名对应的对象字典
            Dictionary<string, object> objDic = dataDic[fileName];
            // 遍历对象字典，将对象添加到返回列表中
            foreach (KeyValuePair<string, object> kvp in objDic)
            {
                returnList.Add((T)kvp.Value);
            }
            return returnList;
        }

        /// <summary>
        /// 读取指定文件名的 CSV 文件，并将数据映射到指定类型的对象中。
        /// </summary>
        /// <typeparam name="T">配置数据对象的类型。</typeparam>
        /// <param name="fileName">CSV 文件名，若为 null 则使用类型 T 的名称。</param>
        /// <param name="hasPriKey">是否有主键，默认为 true。</param>
        public static void ReadConfigData<T>(string fileName = null, bool hasPriKey = true)
        {
            // 若文件名未指定，则使用类型 T 的名称作为文件名
            if (fileName == null)
            {
                fileName = typeof(T).Name;
            }

            // 构建 CSV 文件的路径
            string path = "Configs/Csvs/" + fileName + ".csv";
            // 读取 CSV 文件的所有文本内容
            string getString = File.ReadAllText(path);
            // 创建 CsvReaderByString 对象，用于解析 CSV 内容
            CsvReaderByString csr = new CsvReaderByString(getString);

            // 用于存储对象的字典
            Dictionary<string, object> objDic = new Dictionary<string, object>();

            // 用于存储类型 T 的字段信息数组
            FieldInfo[] fis = new FieldInfo[csr.ColCount];
            // 遍历 CSV 文件的列，获取类型 T 中对应的字段信息
            for (int colNum = 1; colNum < csr.ColCount + 1; colNum++)
            {
                fis[colNum - 1] = typeof(T).GetField(csr[3, colNum]);
            }

            // 用于记录无主键时的索引
            int index = 0;
            // 从第 4 行开始遍历 CSV 文件的行
            for (int rowNum = 4; rowNum < csr.RowCount + 1; rowNum++)
            {
                // 创建类型 T 的实例
                T configObj = Activator.CreateInstance<T>();
                // 遍历字段信息数组
                for (int i = 0; i < fis.Length; i++)
                {
                    // 获取当前单元格的值
                    string fieldValue = csr[rowNum, i + 1];
                    // 用于存储解析后的值
                    object setValue = new object();

                    // 根据字段类型进行值的解析
                    switch (fis[i].FieldType.ToString())
                    {
                        case "System.Int32":
                            setValue = int.Parse(fieldValue);
                            break;
                        case "System.Int64":
                            setValue = long.Parse(fieldValue);
                            break;
                        case "System.String":
                            setValue = fieldValue;
                            break;
                        case "System.Single":
                            try
                            {
                                setValue = float.Parse(fieldValue);
                            }
                            catch (System.Exception)
                            {
                                setValue = 0.0f;
                            }
                            break;
                        default:
                            // 记录不支持的数据类型错误日志
                            logger.Error("error data type: " + fis[i].FieldType.ToString());
                            break;
                    }
                    // 将解析后的值设置到对象的字段中
                    fis[i].SetValue(configObj, setValue);
                    // 若有主键且字段名为 key 或 ID，则将对象添加到 objDic 中
                    if (hasPriKey && (fis[i].Name == "key" || fis[i].Name == "ID"))
                    {
                        objDic.Add(setValue.ToString(), configObj);
                    }
                }

                // 若无主键，则使用索引将对象添加到 objDic 中
                if (!hasPriKey)
                {
                    objDic.Add(index.ToString(), configObj);
                }
                index++;
            }
            // 将对象字典添加到 dataDic 中
            dataDic.Add(fileName, objDic);
        }
    }
}