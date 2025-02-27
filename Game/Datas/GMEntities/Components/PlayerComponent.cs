using Game.Core.Caches;

namespace Game.Datas.GMEntities
{
    /// <summary>
    /// 该结构体用于整合玩家的相关信息，包含账户信息和玩家自身详细信息
    /// </summary>
    public struct PlayerComponent
    {
        /// <summary>
        /// 玩家的账户信息，来源于数据库中的 Account 表
        /// </summary>
        public Game.Datas.DBEntities.Account accountInfo;
        /// <summary>
        /// 玩家自身的详细信息，来源于数据库中的 Player 表
        /// </summary>
        public Game.Datas.DBEntities.Player playerInfo;
    }
}