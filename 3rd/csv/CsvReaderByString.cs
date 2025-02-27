using System;
using System.Collections;
using System.IO;

// 定义一个用于读取和解析CSV格式字符串的类
public class CsvReaderByString
{
    // 私有成员变量，使用ArrayList来存储CSV文件的每一行数据
    // 每一行数据本身也是一个ArrayList，用于存储该行的各个列数据
    private ArrayList rowAL;

    // 构造函数，接收一个CSV格式的字符串作为输入，用于初始化解析过程
    public CsvReaderByString(string parseString)
    {
        // 初始化存储行数据的ArrayList
        this.rowAL = new ArrayList();
        // 创建一个StringReader对象，用于逐行读取输入的CSV字符串
        StringReader sr = new StringReader(parseString);
        // 用于临时存储合并后的CSV数据行
        string csvDataLine = "";

        // 进入循环，逐行读取CSV字符串
        while (true)
        {
            // 用于存储每次从输入字符串中读取的一行数据
            string fileDataLine;
            // 从StringReader中读取一行数据
            fileDataLine = sr.ReadLine();
            // 如果读取到的行为空，说明已经读取完整个字符串，退出循环
            if (fileDataLine == null)
            {
                break;
            }
            // 如果临时存储的行数据为空，直接将当前读取的行赋值给它
            if (csvDataLine == "")
            {
                csvDataLine = fileDataLine;
            }
            else
            {
                // 否则，将当前读取的行添加到临时存储的行数据后面，并添加换行符
                csvDataLine += "\r\n" + fileDataLine;
            }
            // 检查当前临时存储的行数据中引号的数量是否为偶数
            // 如果为偶数，说明这是一个完整的数据行
            if (!IfOddQuota(csvDataLine))
            {
                // 将完整的数据行添加到rowAL中
                AddNewDataLine(csvDataLine);
                // 清空临时存储的行数据，准备处理下一行
                csvDataLine = "";
            }
        }
        // 关闭StringReader，释放资源
        sr.Close();
        // 如果循环结束后，临时存储的行数据还有内容，说明CSV文件格式有错误
        if (csvDataLine.Length > 0)
        {
            throw new Exception("CSV文件的格式有错误");
        }
    }

    /// <summary>
    /// 获取CSV数据的行数
    /// </summary>
    public int RowCount
    {
        get
        {
            // 返回存储行数据的ArrayList的元素数量，即行数
            return this.rowAL.Count;
        }
    }

    /// <summary>
    /// 获取CSV数据的最大列数
    /// </summary>
    public int ColCount
    {
        get
        {
            // 用于记录最大列数
            int maxCol;
            // 初始化最大列数为0
            maxCol = 0;
            // 遍历存储行数据的ArrayList
            for (int i = 0; i < this.rowAL.Count; i++)
            {
                // 获取当前行的列数据ArrayList
                ArrayList colAL = (ArrayList)this.rowAL[i];
                // 更新最大列数，如果当前行的列数大于之前记录的最大列数，则更新
                maxCol = (maxCol > colAL.Count) ? maxCol : colAL.Count;
            }
            // 返回最大列数
            return maxCol;
        }
    }

    /// <summary>
    /// 通过行和列的索引获取CSV数据中对应单元格的值
    /// row: 行索引，从1开始计数
    /// col: 列索引，从1开始计数
    /// </summary>
    public string this[int row, int col]
    {
        get
        {
            // 检查行索引是否有效
            CheckRowValid(row);
            // 检查列索引是否有效
            CheckColValid(col);
            // 获取指定行的列数据ArrayList
            ArrayList colAL = (ArrayList)this.rowAL[row - 1];
            // 如果请求的列索引大于当前行的列数，返回空字符串
            if (colAL.Count < col)
            {
                return "";
            }
            // 返回指定单元格的值，并转换为字符串类型
            return colAL[col - 1].ToString();
        }
    }

    /// <summary>
    /// 检查行索引是否有效
    /// </summary>
    /// <param name="row">行索引</param>
    private void CheckRowValid(int row)
    {
        // 行索引不能小于等于0
        if (row <= 0)
        {
            throw new Exception("行数不能小于0");
        }
        // 行索引不能超过实际的行数
        if (row > RowCount)
        {
            throw new Exception("没有当前行的数据");
        }
    }

    /// <summary>
    /// 检查最大行索引是否有效
    /// </summary>
    /// <param name="maxRow">最大行索引</param>
    private void CheckMaxRowValid(int maxRow)
    {
        // 最大行索引不能小于等于0且不能不等于 -1
        if (maxRow <= 0 && maxRow != -1)
        {
            throw new Exception("行数不能等于0或小于-1");
        }
        // 最大行索引不能超过实际的行数
        if (maxRow > RowCount)
        {
            throw new Exception("没有当前行的数据");
        }
    }

    /// <summary>
    /// 检查列索引是否有效
    /// </summary>
    /// <param name="col">列索引</param>
    private void CheckColValid(int col)
    {
        // 列索引不能小于等于0
        if (col <= 0)
        {
            throw new Exception("列数不能小于0");
        }
        // 列索引不能超过最大列数
        if (col > ColCount)
        {
            throw new Exception("没有当前列的数据");
        }
    }

    /// <summary>
    /// 检查最大列索引是否有效
    /// </summary>
    /// <param name="maxCol">最大列索引</param>
    private void CheckMaxColValid(int maxCol)
    {
        // 最大列索引不能小于等于0且不能不等于 -1
        if (maxCol <= 0 && maxCol != -1)
        {
            throw new Exception("列数不能等于0或小于-1");
        }
        // 最大列索引不能超过最大列数
        if (maxCol > ColCount)
        {
            throw new Exception("没有当前列的数据");
        }
    }

    /// <summary>
    /// 判断字符串中引号的数量是否为奇数
    /// </summary>
    /// <param name="dataLine">要检查的字符串</param>
    /// <returns>如果引号数量为奇数，返回true；否则返回false</returns>
    private bool IfOddQuota(string dataLine)
    {
        // 用于记录引号的数量
        int quotaCount;
        // 用于标记引号数量是否为奇数
        bool oddQuota;
        // 初始化引号数量为0
        quotaCount = 0;
        // 遍历字符串中的每个字符
        for (int i = 0; i < dataLine.Length; i++)
        {
            // 如果当前字符是引号，引号数量加1
            if (dataLine[i] == '\"')
            {
                quotaCount++;
            }
        }
        // 初始化标记为false
        oddQuota = false;
        // 如果引号数量除以2的余数为1，说明引号数量为奇数，将标记设为true
        if (quotaCount % 2 == 1)
        {
            oddQuota = true;
        }
        // 返回标记结果
        return oddQuota;
    }

    /// <summary>
    /// 将新的数据行添加到rowAL中，并处理包含引号和逗号的复杂情况
    /// </summary>
    /// <param name="newDataLine">新的数据行</param>
    private void AddNewDataLine(string newDataLine)
    {
        // 用于存储当前行的列数据
        ArrayList colAL = new ArrayList();
        // 使用逗号将数据行分割成多个单元格数据
        string[] dataArray = newDataLine.Split(',');
        // 标记是否以奇数个引号开始
        bool oddStartQuota;
        // 用于临时存储单元格数据
        string cellData;
        // 初始化标记为false
        oddStartQuota = false;
        // 初始化临时存储的单元格数据为空
        cellData = "";
        // 遍历分割后的单元格数据数组
        for (int i = 0; i < dataArray.Length; i++)
        {
            // 如果以奇数个引号开始
            if (oddStartQuota)
            {
                // 因为前面用逗号分割，所以要加上逗号
                cellData += "," + dataArray[i];
                // 检查当前单元格数据是否以奇数个引号结尾
                if (IfOddEndQuota(dataArray[i]))
                {
                    // 处理单元格数据并添加到列数据ArrayList中
                    colAL.Add(GetHandleData(cellData));
                    // 标记为不以奇数个引号开始
                    oddStartQuota = false;
                    // 继续处理下一个单元格数据
                    continue;
                }
            }
            else
            {
                // 检查当前单元格数据是否以奇数个引号开始
                if (IfOddStartQuota(dataArray[i]))
                {
                    // 检查当前单元格数据是否以奇数个引号结尾，且长度大于2，且引号数量不为奇数
                    if (IfOddEndQuota(dataArray[i]) && dataArray[i].Length > 2 && !IfOddQuota(dataArray[i]))
                    {
                        // 处理单元格数据并添加到列数据ArrayList中
                        colAL.Add(GetHandleData(dataArray[i]));
                        // 标记为不以奇数个引号开始
                        oddStartQuota = false;
                        // 继续处理下一个单元格数据
                        continue;
                    }
                    else
                    {
                        // 标记为以奇数个引号开始
                        oddStartQuota = true;
                        // 将当前单元格数据赋值给临时存储的单元格数据
                        cellData = dataArray[i];
                        // 继续处理下一个单元格数据
                        continue;
                    }
                }
                else
                {
                    // 处理单元格数据并添加到列数据ArrayList中
                    colAL.Add(GetHandleData(dataArray[i]));
                }
            }
        }
        // 如果循环结束后，仍以奇数个引号开始，说明数据格式有问题，抛出异常
        if (oddStartQuota)
        {
            throw new Exception("数据格式有问题");
        }
        // 将当前行的列数据ArrayList添加到rowAL中
        this.rowAL.Add(colAL);
    }

    /// <summary>
    /// 判断字符串是否以奇数个引号结尾
    /// </summary>
    /// <param name="dataCell">要检查的字符串</param>
    /// <returns>如果以奇数个引号结尾，返回true；否则返回false</returns>
    private bool IfOddEndQuota(string dataCell)
    {
        // 用于记录引号的数量
        int quotaCount;
        // 用于标记引号数量是否为奇数
        bool oddQuota;
        // 初始化引号数量为0
        quotaCount = 0;
        // 从字符串的末尾开始向前遍历
        for (int i = dataCell.Length - 1; i >= 0; i--)
        {
            // 如果当前字符是引号，引号数量加1
            if (dataCell[i] == '\"')
            {
                quotaCount++;
            }
            else
            {
                // 遇到非引号字符，停止遍历
                break;
            }
        }
        // 初始化标记为false
        oddQuota = false;
        // 如果引号数量除以2的余数为1，说明引号数量为奇数，将标记设为true
        if (quotaCount % 2 == 1)
        {
            oddQuota = true;
        }
        // 返回标记结果
        return oddQuota;
    }

    /// <summary>
    /// 去掉单元格数据的首尾引号，并将双引号替换为单引号
    /// </summary>
    /// <param name="fileCellData">要处理的单元格数据</param>
    /// <returns>处理后的单元格数据</returns>
    private string GetHandleData(string fileCellData)
    {
        // 如果单元格数据为空，直接返回空字符串
        if (fileCellData == "")
        {
            return "";
        }
        // 检查单元格数据是否以奇数个引号开始
        if (IfOddStartQuota(fileCellData))
        {
            // 检查单元格数据是否以奇数个引号结尾
            if (IfOddEndQuota(fileCellData))
            {
                // 去掉首尾引号，并将双引号替换为单引号
                return fileCellData.Substring(1, fileCellData.Length - 2).Replace("\"\"", "\"");
            }
            else
            {
                // 如果引号无法匹配，抛出异常
                throw new Exception("数据引号无法匹配" + fileCellData);
            }
        }
        else
        {
            // 考虑形如 ""    """"      """""" 的情况
            if (fileCellData.Length > 2 && fileCellData[0] == '\"')
            {
                // 去掉首尾引号，并将双引号替换为单引号
                fileCellData = fileCellData.Substring(1, fileCellData.Length - 2).Replace("\"\"", "\"");
            }
        }
        // 返回处理后的单元格数据
        return fileCellData;
    }

    /// <summary>
    /// 判断字符串是否以奇数个引号开始
    /// </summary>
    /// <param name="dataCell">要检查的字符串</param>
    /// <returns>如果以奇数个引号开始，返回true；否则返回false</returns>
    private bool IfOddStartQuota(string dataCell)
    {
        // 用于记录引号的数量
        int quotaCount;
        // 用于标记引号数量是否为奇数
        bool oddQuota;
        // 初始化引号数量为0
        quotaCount = 0;
        // 从字符串的开头开始遍历
        for (int i = 0; i < dataCell.Length; i++)
        {
            // 如果当前字符是引号，引号数量加1
            if (dataCell[i] == '\"')
            {
                quotaCount++;
            }
            else
            {
                // 遇到非引号字符，停止遍历
                break;
            }
        }
        // 初始化标记为false
        oddQuota = false;
        // 如果引号数量除以2的余数为1，说明引号数量为奇数，将标记设为true
        if (quotaCount % 2 == 1)
        {
            oddQuota = true;
        }
        // 返回标记结果
        return oddQuota;
    }
}