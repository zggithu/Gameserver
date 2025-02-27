using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Framework.Core.Utils
{
    /// <summary>
    /// ����ɨ�����࣬�ṩ�ڵ�ǰִ�г�����ɨ���ض����͵Ĺ��ܡ�
    /// </summary>
    public class TypeScanner
    {
        /// <summary>
        /// �г���ǰִ�г��������м̳���ָ�������͵������͡�
        /// </summary>
        /// <param name="parent">Ҫ���������͵ĸ����͡�</param>
        /// <returns>һ���������������͵Ŀ�ö�ټ��ϡ�</returns>
        public static IEnumerable<Type> ListAllSubTypes(Type parent)
        {
            // ��ȡ��ǰִ�еĳ���
            // ���� GetTypes ������ȡ�����е���������
            // ʹ�� Where ����ɸѡ��������ָ�����������������
            return Assembly.GetExecutingAssembly().GetTypes().Where(type => type.IsSubclassOf(parent));
        }

        /// <summary>
        /// �г���ǰִ�г���������ʵ����ָ���ӿڵ����ͣ����������ӿڱ���
        /// </summary>
        /// <param name="_interface">Ҫ����ʵ�����͵Ľӿڡ�</param>
        /// <returns>һ����������ʵ�ָýӿ����͵Ŀ�ö�ټ��ϡ�</returns>
        public static IEnumerable<Type> ListAllAchieveInterface(Type _interface)
        {
            // ��ȡ��ǰִ�еĳ���
            // ���� GetTypes ������ȡ�����е���������
            // ʹ�� Where ����ɸѡ�����п��Ը�ֵ��ָ���ӿ��Ҳ����ڸýӿڱ��������
            return Assembly.GetExecutingAssembly()
              .GetTypes()
              .Where(type => type.IsAssignableTo(_interface) && type != _interface);
        }

        /// <summary>
        /// �г���ǰִ�г���������Ӧ����ָ�����Ե����͡�
        /// </summary>
        /// <param name="attribute">Ҫ����Ӧ�����͵����ԡ�</param>
        /// <returns>һ����������Ӧ���˸��������͵Ŀ�ö�ټ��ϡ�</returns>
        public static IEnumerable<Type> ListTypesWithAttribute(Type attribute)
        {
            // ��ȡ��ǰִ�еĳ���
            // ���� GetTypes ������ȡ�����е���������
            // ʹ�� Where ����ɸѡ������Ӧ����ָ�����Ե�����
            return Assembly.GetExecutingAssembly()
              .GetTypes()
              .Where(type => type.IsDefined(attribute));
        }
    }
}