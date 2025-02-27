using System;
using System.Reflection;

namespace Framework.Core.Net
{

    public class CmdExecutor
    {
        /** logic controller  */
        // �洢�߼����������󣬸ö�����������ҵ���߼�������
        public object handler;

        /** logic handler method */
        // �洢�߼�����������Ϣ��ͨ��������Ե��ø÷���
        public MethodInfo method;

        /** arguments passed to method */
        // �洢���ݸ��������Ĳ����������飬����ȷ����������ʱ�Ĳ���
        public Type[] @params;

        // ��̬���������ڴ��� CmdExecutor ʵ��
        public static CmdExecutor Create(MethodInfo method, Type[] @params, object handler)
        {
            // ����һ���µ� CmdExecutor ʵ��
            CmdExecutor executor = new CmdExecutor();
            // ������Ĵ�������Ϣ��ֵ��ʵ���� method ����
            executor.method = method;
            // ������Ĳ����������鸳ֵ��ʵ���� @params ����
            executor.@params = @params;
            // ��������߼�����������ֵ��ʵ���� handler ����
            executor.handler = handler;

            // ���ش����õ� CmdExecutor ʵ��
            return executor;
        }
    }
}