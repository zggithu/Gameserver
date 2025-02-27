namespace Game
{
    /// <summary>
    /// 定义游戏中的不同业务模块，每个模块对应一个唯一的整数值。
    /// </summary>
    enum Module
    {
        /// <summary>
        /// 登录模块，用于处理用户登录相关业务。
        /// </summary>
        AUTH = 101,
        /// <summary>
        /// 玩家模块，负责处理玩家相关的业务逻辑。
        /// </summary>
        PLAYER = 102,
        /// <summary>
        /// 场景模块，与游戏场景相关的业务。
        /// </summary>
        SCENE = 103,
        /// <summary>
        /// 活动模块，管理游戏中的各种活动。
        /// </summary>
        ACTIVITY = 104,
        /// <summary>
        /// 技能模块，处理游戏中技能相关的业务。
        /// </summary>
        SKILL = 105,
        /// <summary>
        /// 聊天模块，实现游戏内的聊天功能。
        /// </summary>
        CHAT = 106,

        // ------------------跨服业务功能模块（501开始）---------------------
        /// <summary>
        /// 跨服天梯模块，用于跨服天梯相关的业务。
        /// </summary>
        LADDER = 501
    }

    /// <summary>
    /// 定义游戏中的各种命令，包括请求和响应，用于客户端与服务器之间的通信。
    /// </summary>
    public enum Cmd
    {
        // 游客登录
        /// <summary>
        /// 游客登录请求命令。
        /// </summary>
        eGuestLoginReq = 1,
        /// <summary>
        /// 游客登录响应命令。
        /// </summary>
        eGuestLoginRes,

        // 用户名注册
        /// <summary>
        /// 用户名注册请求命令。
        /// </summary>
        eRegisterUserReq,
        /// <summary>
        /// 用户名注册响应命令。
        /// </summary>
        eRegisterUserRes,

        // 用户登录
        /// <summary>
        /// 用户登录请求命令。
        /// </summary>
        eUserLoginReq,
        /// <summary>
        /// 用户登录响应命令。
        /// </summary>
        eUserLoginRes,

        // 游客账号升级
        /// <summary>
        /// 游客账号升级请求命令。
        /// </summary>
        eGuestUpgradeReq,
        /// <summary>
        /// 游客账号升级响应命令。
        /// </summary>
        eGuestUpgradeRes,

        // 拉取玩家游戏数据
        /// <summary>
        /// 拉取玩家游戏数据请求命令。
        /// </summary>
        ePullingPlayerDataReq,
        /// <summary>
        /// 拉取玩家游戏数据响应命令。
        /// </summary>
        ePullingPlayerDataRes,

        // 领取每日登录奖励
        /// <summary>
        /// 领取每日登录奖励请求命令。
        /// </summary>
        eRecvLoginBonuesReq,
        /// <summary>
        /// 领取每日登录奖励响应命令。
        /// </summary>
        eRecvLoginBonuesRes,

        // 拉取玩家的奖励数据
        /// <summary>
        /// 拉取玩家奖励数据请求命令。
        /// </summary>
        ePullingBonuesListReq,
        /// <summary>
        /// 拉取玩家奖励数据响应命令。
        /// </summary>
        ePullingBonuesListRes,

        // 领取玩家的奖励
        /// <summary>
        /// 领取玩家奖励请求命令。
        /// </summary>
        eRecvBonuesReq,
        /// <summary>
        /// 领取玩家奖励响应命令。
        /// </summary>
        eRecvBonuesRes,

        // 玩家选择角色
        /// <summary>
        /// 玩家选择角色请求命令。
        /// </summary>
        eSelectPlayerReq,
        /// <summary>
        /// 玩家选择角色响应命令。
        /// </summary>
        eSelectPlayerRes,

        // 拉取玩家的任务数据
        /// <summary>
        /// 拉取玩家任务数据请求命令。
        /// </summary>
        ePullingTaskListReq,
        /// <summary>
        /// 拉取玩家任务数据响应命令。
        /// </summary>
        ePullingTaskListRes,

        // 拉取玩家的邮件消息
        /// <summary>
        /// 拉取玩家邮件消息请求命令。
        /// </summary>
        ePullingMailMsgReq,
        /// <summary>
        /// 拉取玩家邮件消息响应命令。
        /// </summary>
        ePullingMailMsgRes,

        // 玩家更新邮件消息状态
        /// <summary>
        /// 玩家更新邮件消息状态请求命令。
        /// </summary>
        eUpdateMailMsgReq,
        /// <summary>
        /// 玩家更新邮件消息状态响应命令。原注释拼写错误，这里修正为 eUpdateMailMsgRes。
        /// </summary>
        eUpdategMailMsgRes,

        // 玩家拉取排行
        /// <summary>
        /// 玩家拉取排行榜数据请求命令。
        /// </summary>
        ePullingRankReq,
        /// <summary>
        /// 玩家拉取排行榜数据响应命令。
        /// </summary>
        ePullingRankRes,

        // 玩家拉取背包数据
        /// <summary>
        /// 玩家拉取背包数据请求命令。
        /// </summary>
        ePullingPackDataReq,
        /// <summary>
        /// 玩家拉取背包数据响应命令。
        /// </summary>
        ePullingPackDataRes,

        // 玩家交易兑换
        /// <summary>
        /// 玩家交易兑换请求命令。
        /// </summary>
        eExchangeProductReq,
        /// <summary>
        /// 玩家交易兑换响应命令。
        /// </summary>
        eExchangeProductRes,

        // 测试正式项目不要直接开放出来
        /// <summary>
        /// 测试获取物品请求命令，正式项目不应直接开放。
        /// </summary>
        eTestGetGoodReq,
        /// <summary>
        /// 测试获取物品响应命令，正式项目不应直接开放。
        /// </summary>
        eTestGetGoodRes,

        // 测试更新背包物品,正式项目不要直接开放出来
        /// <summary>
        /// 测试更新背包物品请求命令，正式项目不应直接开放。
        /// </summary>
        eTestUpdateGoodsReq,
        /// <summary>
        /// 测试更新背包物品响应命令，原注释拼写错误，这里修正为 eTestUpdateGoodsRes。
        /// </summary>
        eTestUpdateGoddsRes,

        // 测试游戏逻辑服通讯命令
        /// <summary>
        /// 测试游戏逻辑服通讯请求命令。
        /// </summary>
        eTestLogicCmdEchoReq,
        /// <summary>
        /// 测试游戏逻辑服通讯响应命令。
        /// </summary>
        eTestLogicCmdEchoRes,
    }

    /// <summary>
    /// 定义游戏中各种操作的响应状态，每个状态对应一个唯一的整数值。
    /// </summary>
    public enum Respones
    {
        /// <summary>
        /// 操作成功的响应状态。
        /// </summary>
        OK = 1,

        /// <summary>
        /// 系统错误的响应状态。
        /// </summary>
        SystemErr = -100,
        /// <summary>
        /// 用户被冻结的响应状态。
        /// </summary>
        UserIsFreeze = -101,
        /// <summary>
        /// 用户不是游客账号的响应状态。
        /// </summary>
        UserIsNotGuest = -102,
        /// <summary>
        /// 参数无效的响应状态。
        /// </summary>
        InvalidParams = -103,
        /// <summary>
        /// 用户名已存在的响应状态。
        /// </summary>
        UnameIsExist = -104,
        /// <summary>
        /// 用户名或密码错误的响应状态。
        /// </summary>
        UnameOrUpwdError = -105,
        /// <summary>
        /// 无效操作的响应状态。
        /// </summary>
        InvalidOpt = -106,
        /// <summary>
        /// 玩家不存在的响应状态。
        /// </summary>
        PlayerIsNotExist = -107,
        /// <summary>
        /// 账号未登录的响应状态。
        /// </summary>
        AccountIsNotLogin = -108,
        /// <summary>
        /// 玩家被冻结的响应状态。
        /// </summary>
        PlayerIsFreeze = -109,
        /// <summary>
        /// 账号不存在的响应状态。
        /// </summary>
        AccountIsNotExist = -110,
        /// <summary>
        /// 金钱不足的响应状态。
        /// </summary>
        MoneyIsNotEnough = -111,
        // ...
    }

    /// <summary>
    /// 定义游戏的不同渠道，用于区分玩家的来源。
    /// </summary>
    public enum Channal
    {
        /// <summary>
        /// 无效渠道的标识。
        /// </summary>
        InvalidChannal = -1,
        /// <summary>
        /// 自有渠道的标识。
        /// </summary>
        SelfChannal = 0,
        /// <summary>
        /// 抖音渠道的标识。
        /// </summary>
        DouYin,
        /// <summary>
        /// 苹果应用商店渠道的标识。
        /// </summary>
        IosAppStore,
        // ...
    }

    /// <summary>
    /// 定义游戏中排行榜的类型，每个类型对应一个唯一的整数值。
    /// </summary>
    public enum RankType
    {
        /// <summary>
        /// 世界金币排行榜的类型标识。
        /// </summary>
        WorldCoin = 100001,

        // ...
    }

    /// <summary>
    /// 定义游戏中的规则类型，用于对不同的规则进行分类。
    /// </summary>
    public enum RuleType
    {
        /// <summary>
        /// 任务规则类型。
        /// </summary>
        Task = 1,
        /// <summary>
        /// 交易规则类型。
        /// </summary>
        Trading,
        /// <summary>
        /// 奖励规则类型。
        /// </summary>
        Bonues,
        /// <summary>
        /// 背包规则类型。
        /// </summary>
        Backpack,
    }
}