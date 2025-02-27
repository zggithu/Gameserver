using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Game.Utils
{
    /// <summary>
    /// ��Ϸ�����࣬�ṩ��һЩͨ�õĹ��߷��������ڴ�����������ַ���������ProtoBuf ���л��ͷ����л��Լ���������ȡ�
    /// </summary>
    public class GameUtils
    {
        /// <summary>
        /// ����ָ����Χ�ڵ����������
        /// </summary>
        /// <param name="min">���������Сֵ����������</param>
        /// <param name="max">����������ֵ������������</param>
        /// <returns>���ɵ����������</returns>
        public static int Random(int min, int max) {
            // ����һ�������������ʵ��
            Random random = new Random();
            // ����ָ����Χ�ڵ��������
            int s = random.Next(max) % (max - min + 1) + min;
            return s;
        }

        /// <summary>
        /// ������ʽΪ "key1=value1|key2=value2" ���ַ���������ת��Ϊ��ֵ���ֵ䡣
        /// </summary>
        /// <param name="formatString">Ҫ�������ַ�����</param>
        /// <returns>�����������ֵ�Ե��ֵ䣬��Ϊ�ַ������ͣ�ֵΪ�������͡�</returns>
        public static Dictionary<string, int> ParseStringWithKeyIntValue(string formatString) {
            // ��ʼ��һ���յ��ֵ����ڴ洢������ļ�ֵ��
            Dictionary<string, int> props = new Dictionary<string, int>();
            // ʹ�� '|' �ָ��ַ������õ������ֵ���ַ���
            string[] conds = formatString.Split('|');
            for (int i = 0; i < conds.Length; i++) {
                // ʹ�� '=' �ָ�ÿ����ֵ���ַ������õ�����ֵ
                string[] keyAndValue = conds[i].Split('=');
                string key = keyAndValue[0];
                // ��ֵת��Ϊ��������
                int value = int.Parse(keyAndValue[1]);

                // ����ֵ����ӵ��ֵ���
                props.Add(key, value);
            }

            return props;
        }

        /// <summary>
        /// �� ProtoBuf �������л�Ϊ�ֽ����顣
        /// </summary>
        /// <typeparam name="T">Ҫ���л��Ķ������͡�</typeparam>
        /// <param name="objectData">Ҫ���л��Ķ���ʵ����</param>
        /// <returns>���л�����ֽ����顣</returns>
        public static byte[] PbObjToBytes<T>(T objectData) {
            // ����һ���ڴ������ڴ洢���л��������
            var stream = new MemoryStream();
            // ʹ�� ProtoBuf ���л������������л�Ϊ��
            Serializer.Serialize<T>(stream, (T)objectData);
            // �����е�����ת��Ϊ�ֽ�����
            byte[] data = stream.ToArray();
            return data;
        }

        /// <summary>
        /// ���ֽ����鷴���л�Ϊ ProtoBuf ����
        /// </summary>
        /// <typeparam name="T">Ҫ�����л��Ķ������͡�</typeparam>
        /// <param name="data">Ҫ�����л����ֽ����顣</param>
        /// <returns>�����л���Ķ���ʵ����</returns>
        public static T BytesToPbObj<T>(byte[] data) {
            // ����һ���ڴ��������ֽ�����д������
            var stream = new MemoryStream(data);
            // ʹ�� ProtoBuf ���л��������з����л�����
            return Serializer.Deserialize<T>(stream);
        }

        /// <summary>
        /// ʹ�÷����ȡ������ֶ�ֵ��
        /// </summary>
        /// <param name="inst">Ҫ��ȡ�ֶ�ֵ�Ķ���ʵ����</param>
        /// <param name="filedName">�ֶε����ơ�</param>
        /// <param name="defaultObj">����ֶβ����ڣ����ص�Ĭ��ֵ��</param>
        /// <returns>�ֶε�ֵ������ֶβ������򷵻�Ĭ��ֵ��</returns>
        public static object GetFiled(object inst, string filedName, object defaultObj) {
            // ��ȡ���������
            Type t = inst.GetType();
            // ��ȡָ�����Ƶ��ֶ���Ϣ
            FieldInfo f = t.GetField(filedName);
            if (f == null) {
                // ����ֶβ����ڣ�����Ĭ��ֵ
                return defaultObj;
            }

            // ��ȡ�ֶε�ֵ
            return f.GetValue(inst);
        }

        /// <summary>
        /// ʹ�÷������ö�����ֶ�ֵ��
        /// </summary>
        /// <param name="inst">Ҫ�����ֶ�ֵ�Ķ���ʵ����</param>
        /// <param name="filedName">�ֶε����ơ�</param>
        /// <param name="value">Ҫ���õ��ֶ�ֵ��</param>
        public static void SetFiled(object inst, string filedName, object value) {
            // ��ȡ���������
            Type t = inst.GetType();
            // ��ȡָ�����Ƶ��ֶ���Ϣ
            FieldInfo f = t.GetField(filedName);
            if (f == null) {
                // ����ֶβ����ڣ�ֱ�ӷ���
                return;
            }

            // �����ֶε�ֵ
            f.SetValue(inst, value);
        }

        /// <summary>
        /// ʹ�÷����ȡ���������ֵ��
        /// </summary>
        /// <param name="inst">Ҫ��ȡ����ֵ�Ķ���ʵ����</param>
        /// <param name="propName">���Ե����ơ�</param>
        /// <param name="defaultObj">������Բ����ڣ����ص�Ĭ��ֵ��</param>
        /// <returns>���Ե�ֵ��������Բ������򷵻�Ĭ��ֵ��</returns>
        public static object GetProp(object inst, string propName, object defaultObj) {
            // ��ȡ���������
            Type t = inst.GetType();
            // ��ȡָ�����Ƶ�������Ϣ
            PropertyInfo prop = t.GetProperty(propName);
            if (prop == null) {
                // ������Բ����ڣ�����Ĭ��ֵ
                return defaultObj;
            }

            // ��ȡ���Ե�ֵ
            return prop.GetValue(inst);
        }

        /// <summary>
        /// ʹ�÷������ö��������ֵ��
        /// </summary>
        /// <param name="inst">Ҫ��������ֵ�Ķ���ʵ����</param>
        /// <param name="propName">���Ե����ơ�</param>
        /// <param name="value">Ҫ���õ�����ֵ��</param>
        public static void SetProp(object inst, string propName, object value) {
            // ��ȡ���������
            Type t = inst.GetType();
            // ��ȡָ�����Ƶ�������Ϣ
            PropertyInfo prop = t.GetProperty(propName);
            if (prop == null) {
                // ������Բ����ڣ�ֱ�ӷ���
                return;
            }

            // �������Ե�ֵ
            prop.SetValue(inst, value);
        }
    }
}