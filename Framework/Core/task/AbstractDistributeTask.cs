using System;

namespace Framework.Core.task
{
    /// <summary>
    /// ������ AbstractDistributeTask ��Ϊ�ֲ�ʽ����Ļ��࣬
    /// Ϊ������������ṩ�˻����ṹ�͹���������
    /// ���о���ķֲ�ʽ�����඼Ӧ�̳��Դ��ಢʵ�� DoAction ������
    /// </summary>
    public abstract class AbstractDistributeTask
    {
        /// <summary>
        /// ����ķַ������� Session �е�Ψһ����Ӧ��
        /// ���ڽ�����ַ����ض��Ĵ����̻߳���С�
        /// </summary>
        public long distributeKey;

        /// <summary>
        /// ÿ���������ʼʱ�������¼����ʼִ�е�ʱ�䡣
        /// </summary>
        private long startMillis;

        /// <summary>
        /// ÿ�����������ʱ�������¼����ִ����ɵ�ʱ�䡣
        /// </summary>
        private long endMillis;

        /// <summary>
        /// ���󷽷�������������߼�Ӧ������ʵ�֡�
        /// �÷�������������ִ��ʱҪ��ɵľ��������
        /// </summary>
        public abstract void DoAction();

        /// <summary>
        /// ��ȡ����������ơ�
        /// </summary>
        /// <returns>����������ơ�</returns>
        public string getName() => GetType().Name;

        /// <summary>
        /// ��ȡ�������ʼʱ�����
        /// </summary>
        /// <returns>�������ʼʱ��������룩��</returns>
        public long getStartMillis() => startMillis;

        /// <summary>
        /// ��ȡ����Ľ���ʱ�����
        /// </summary>
        /// <returns>����Ľ���ʱ��������룩��</returns>
        public long getEndMillis() => endMillis;

        /// <summary>
        /// ����������ʼʱ�����
        /// ʹ�� UTC ʱ������ 1970 �� 1 �� 1 �տ�ʼ����ǰʱ��ĺ�������
        /// </summary>
        public void markStartMillis() => startMillis = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

        /// <summary>
        /// �������Ľ���ʱ�����
        /// ʹ�� UTC ʱ������ 1970 �� 1 �� 1 �տ�ʼ����ǰʱ��ĺ�������
        /// </summary>
        public void markEndMillis() => endMillis = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
    }
}