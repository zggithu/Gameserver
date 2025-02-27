namespace Game.Datas.GMEntities
{
    /// <summary>
    /// 表示游戏中的玩家实体类。
    /// 该类封装了玩家的基本信息、任务信息和背包信息等组件。
    /// 通过此类可以方便地管理和操作玩家在游戏中的各种属性和状态。
    /// </summary>
    public class GM_PlayerEntity
    {

        /// <summary>
        /// 玩家的基本信息组件，包含玩家的基础属性、角色信息等。
        /// </summary>
        public PlayerComponent uPlayer;

        /// <summary>
        /// 玩家的任务信息组件，用于管理玩家当前的任务状态、任务进度等。
        /// </summary>
        public TaskComponent uTask;

        /// <summary>
        /// 玩家的背包信息组件，负责管理玩家拥有的物品、道具等。
        /// </summary>
        public BackpackComponent uBackpack;
    }
}