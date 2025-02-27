using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Framework.Core.Serializer;
using Framework.Core.task;
using Framework.Core.Utils;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Framework.Core.Net
{
    /// <summary>
    /// MessageDispatcher �ฺ����Ϣ�ķַ�����������ʼ����Ϣ����ӳ�䡢
    /// ����ͻ��˵Ľ��롢�˳��¼��Լ��ͻ�����Ϣ��
    /// </summary>
    class MessageDispatcher
    {
        // ����ģʽ���ṩȫ��Ψһ�� MessageDispatcher ʵ��
        public static MessageDispatcher Instance = new MessageDispatcher();

        // ʹ�� NLog ��¼��־����ȡ��ǰ�����־��¼��
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        // �洢ģ�������� CmdExecutor ��ӳ���ϵ����Ϊģ���������ϵ��ַ�����ֵΪ CmdExecutor
        /** [module_cmd, CmdExecutor] */
        private static Dictionary<string, CmdExecutor> MODULE_CMD_HANDLERS = new();

        /// <summary>
        /// ��ȡ��������Ϣ������Ԫ���ݣ���ģ���������Ϣ��
        /// </summary>
        /// <param name="method">Ҫ���ķ�����Ϣ��</param>
        /// <returns>����ģ�������Ķ��������飬���δ�ҵ��򷵻� null��</returns>
        public short[] GetMessageMeta(MethodInfo method)
        {
            // �������������в���
            foreach (ParameterInfo parameter in method.GetParameters())
            {
                // �����������Ƿ�ɸ�ֵΪ Message ����
                if (parameter.ParameterType.IsAssignableTo(typeof(Message)))
                {
                    // ��ȡ���������ϵ� MessageMeta ����
                    MessageMeta msgMeta = parameter.ParameterType.GetCustomAttribute<MessageMeta>();
                    if (msgMeta != null)
                    {
                        // ��ȡģ���������Ϣ������
                        short[] meta = { msgMeta.module, msgMeta.cmd };
                        return meta;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// ����ģ��������Ψһ�ļ������ڴ洢�Ͳ��Ҵ������
        /// </summary>
        /// <param name="module">ģ���š�</param>
        /// <param name="cmd">�����š�</param>
        /// <returns>ģ���������ϵ��ַ�������</returns>
        private string BuildKey(short module, short cmd)
        {
            return module + "_" + cmd;
        }

        /// <summary>
        /// ��ʼ����Ϣ����ӳ�䣬ɨ����� Controller ���Ե���ʹ��� RequestMapping ���Եķ�����
        /// ������ע�ᵽ MODULE_CMD_HANDLERS �ֵ��С�
        /// </summary>
        public void Init()
        {  // ��ʼ��
            // ��ȡ���д��� Controller ���Ե���
            IEnumerable<Type> controllers = TypeScanner.ListTypesWithAttribute(typeof(Controller));
            foreach (Type controller in controllers)
            {
                try
                {
                    // �������������ʵ��
                    object handler = Activator.CreateInstance(controller);
                    // ��ȡ������������з���
                    MethodInfo[] methods = controller.GetMethods();

                    foreach (MethodInfo method in methods)
                    {
                        // ��ȡ�����ϵ� RequestMapping ����
                        RequestMapping mapperAttribute = method.GetCustomAttribute<RequestMapping>();
                        if (mapperAttribute == null)
                        {
                            // �������û�и����ԣ�������
                            continue;
                        }

                        // ��ȡ��������Ϣ������Ԫ����
                        short[] meta = this.GetMessageMeta(method);
                        short module = meta[0];
                        short cmd = meta[1];

                        // ����ģ���������ϵļ�
                        string key = BuildKey(module, cmd);
                        // ���ü��Ƿ��Ѿ�������ӳ����
                        MODULE_CMD_HANDLERS.TryGetValue(key, out CmdExecutor cmdExecutor);

                        if (cmdExecutor != null)
                        {
                            // ����Ѿ����ڣ����¼������־������
                            logger.Warn($"module[{module}] cmd[{cmd}] �ظ�ע�ᴦ����");
                            return;
                        }

                        // ���� CmdExecutor ʵ������װ���������������ͺʹ������ʵ��
                        cmdExecutor = CmdExecutor.Create(method, method.GetParameters().Select(parameterInfo => parameterInfo.ParameterType).ToArray(), handler);

                        // �����Ͷ�Ӧ�� CmdExecutor ����ӳ����
                        MODULE_CMD_HANDLERS.Add(key, cmdExecutor);
                    }
                }
                catch (Exception e)
                {
                    // �����쳣����¼������־
                    logger.Error(e.Message);
                }
            }
        }

        /// <summary>
        /// �������¿ͻ��˽�����¼�����¼�ͻ��˵�Զ�̵�ַ��
        /// </summary>
        /// <param name="s">�ͻ��˻Ự��Ϣ��</param>
        public void OnClientEnter(IdSession s)
        {
            this.logger.Debug($"On Client Enter: {s.GetRemoteAddress()}");
        }
         
        /// <summary>
        /// ����ͻ����뿪���¼�����¼�ͻ��˵�Զ�̵�ַ��
        /// </summary>
        /// <param name="s">�ͻ��˻Ự��Ϣ��</param>
        public void OnClientExit(IdSession s)
        {
            this.logger.Debug($"On Client Exit: {s.GetRemoteAddress()}");
        }

        /// <summary>
        /// ���ͻ��˻Ự����Ϣ����Ϣת��Ϊ����������Ĳ������顣
        /// </summary>
        /// <param name="session">�ͻ��˻Ự��Ϣ��</param>
        /// <param name="methodParams">�������Ĳ����������顣</param>
        /// <param name="message">���յ�����Ϣ��</param>
        /// <returns>����������Ĳ������顣</returns>
        private object[] ConvertToMethodParams(IdSession session, Type[] methodParams, Message message)
        {
            // ����һ���뷽������������ͬ�Ķ�������
            object[] result = new object[methodParams == null ? 0 : methodParams.Length];

            // ������������
            for (int i = 0; i < result.Length; i++)
            {
                Type param = methodParams[i];
                if (param.IsAssignableTo(typeof(IdSession)))
                {
                    // ������������� IdSession���򽫿ͻ��˻Ự��Ϣ��ֵ���ò���
                    result[i] = session;
                }
                else if (param.IsAssignableTo(typeof(long)))
                {
                    // ������������� long���򽫿ͻ����˺� ID ��ֵ���ò���
                    result[i] = session.accountId;
                }
                else if (param.IsAssignableTo(typeof(Message)))
                {
                    // ������������� Message���򽫽��յ�����Ϣ��ֵ���ò���
                    result[i] = message;
                }
            }

            return result;
        }

        /// <summary>
        /// ������յ��Ŀͻ�����Ϣ������ģ���������Ҷ�Ӧ�Ĵ������
        /// ������Ϣ����������ӵ�������С�
        /// </summary>
        /// <param name="s">�ͻ��˻Ự��Ϣ��</param>
        /// <param name="data">���յ����ֽ����ݡ�</param>
        /// <param name="offset">���ݵ���ʼƫ������</param>
        /// <param name="count">���ݵĳ��ȡ�</param>
        public void OnClientMsg(IdSession s, byte[] data, int offset, int count)
        {
            // ��¼�ͻ��˽��յ��������־��ע�Ͳ��֣��ɸ�����Ҫ���ã�
            // this.logger.Debug($"On Client Recv Cmd: {s.GetRemoteAddress()}");

            // ���ֽ������ж�ȡģ���ţ�ʹ��С���ֽ���
            short module = UtilsHelper.ReadShortLE(data, offset + 0);
            // ���ֽ������ж�ȡ�����ţ�ʹ��С���ֽ���
            short cmd = UtilsHelper.ReadShortLE(data, offset + 2);

            // ���ֽ������ж�ȡ�û���ǩ��ʹ��С���ֽ��򣩣����ؿ��ܻ��õ�����ʱ����
            uint utag = UtilsHelper.ReadUintLE(data, offset + 4);

            // ʹ�����л����߶���Ϣ���н���
            Message msg = SerializerHelper.PbDecode(module, cmd, data, offset + 8, count - 8);

            // ����ģ���������ϵļ�
            string key = BuildKey(module, cmd);
            // ��ӳ���в��Ҷ�Ӧ�� CmdExecutor
            MODULE_CMD_HANDLERS.TryGetValue(key, out CmdExecutor cmdExecutor);
            if (cmdExecutor != null)
            {
                // ת��Ϊ����������Ĳ�������
                object[] @params = ConvertToMethodParams(s, cmdExecutor.@params, msg);
                // ����Ϣ����������ӵ���������
                TaskWorkerPool.Instance.AddTask(MessageTask.Create(s.distributeKey, cmdExecutor.handler, cmdExecutor.method, @params, s));
                return;
            }
            else
            {  // �鿴һ���Ƿ�ΪLogic�����¼�,Ȼ��ֱ��Ͷ�ݵ��߼����̴߳���
                // ���δ�ҵ������������Ϣ���͵��߼��������̴߳���
                LogicWorkerPool.Instance.PushMsgToLogicServer(s, module, cmd, msg);
            }

        }
    }
}