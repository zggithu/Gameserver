using Game.Core.Caches;

namespace Game.Datas.GMEntities
{
    /// <summary>
    /// �ýṹ������������ҵ������Ϣ�������˻���Ϣ�����������ϸ��Ϣ
    /// </summary>
    public struct PlayerComponent
    {
        /// <summary>
        /// ��ҵ��˻���Ϣ����Դ�����ݿ��е� Account ��
        /// </summary>
        public Game.Datas.DBEntities.Account accountInfo;
        /// <summary>
        /// ����������ϸ��Ϣ����Դ�����ݿ��е� Player ��
        /// </summary>
        public Game.Datas.DBEntities.Player playerInfo;
    }
}