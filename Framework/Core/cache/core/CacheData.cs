using System;

namespace Framework.Core.Cache {
    public class CacheData<T>
    {
        /// <summary>
        /// ����ʱ��
        /// </summary>
        public DateTime AccessTime { get; set; }

        /// <summary>
        /// д��ʱ��
        /// </summary>
        public DateTime WriteTime { get; set; }

        ///// <summary>
        ///// ��������
        ///// </summary>
        //public T Data { get; set; }

        /// <summary>
        /// �����
        /// </summary>
        public string Key { get; set; }

        public T Original { get; set; }
    }
}
