using System.Collections.Generic;

namespace Framework.Core.Cache {
    public interface ILoadingCache<K, V> {
        /// <summary>
        /// ��ȡ��������
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        V GetIfPresent(K key);

        /// <summary>
        /// ��ȡ���ݣ�����û�����ί�в�ѯ
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        V Get(K key);

        /// <summary>
        /// ��ӻ���
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Set(K key, V value);

        /// <summary>
        /// ��ȡ������Ч����
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        Dictionary<K, V> GetAllPresent(List<K> keys);

        /// <summary>
        /// �ǻ���ʧЧ
        /// </summary>
        /// <param name="key"></param>
        void Invalidate(K key);
    }

}