using Framework.Core.Net;
using Framework.Core.Utils;
using Game.Datas.DBEntities;
using Game.Datas.Messages;
using Game.Core.GM_Bonues;
using Game.Core.Caches;
using Game.Core.EntityMgr;
using Game.Datas.GMEntities;
using Game.Core.GM_Task;
using System.Collections.Generic;
using Game.Core.GM_MailMessage;
using Game.Core.GM_Rank;
using Game.Core.GM_Backpack;
using Game.Core.GM_Trading;

namespace Game.Entries.Modules
{
    /// <summary>
    /// 玩家模块类，负责处理玩家相关的各种业务逻辑，采用单例模式。
    /// </summary>
    public class PlayerModule
    {
        /// <summary>
        /// 单例实例，全局唯一。
        /// </summary>
        public static PlayerModule Instance = new PlayerModule();

        /// <summary>
        /// NLog 日志记录器，用于记录模块运行过程中的信息。
        /// </summary>
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 初始化模块，主要初始化相关的缓存。
        /// </summary>
        public void Init() {
            PlayerIDCache.Instance.Init();
            PlayerAcountIDCache.Instance.Init();
            PlayerIdLoginBonuesCache.Instance.Init();
        }

        /// <summary>
        /// 将数据库中的玩家信息复制到响应数据对象中。
        /// </summary>
        /// <param name="p">数据库中的玩家对象。</param>
        /// <param name="pInfo">要填充的玩家信息响应对象。</param>
        private void CopyDbPlayerToResponesData(Game.Datas.DBEntities.Player p, PlayerInfo pInfo) {
            pInfo.exp = p.exp;
            pInfo.hp = p.HP;
            pInfo.mp = p.MP;
            pInfo.umoney = p.umoney;
            pInfo.unick = p.name;
            pInfo.ucion = p.ucoin;
            pInfo.usex = p.usex;
        }

        /// <summary>
        /// 处理领取登录奖励的请求。
        /// 验证玩家登录状态，检查奖励是否可领取，更新玩家金币和奖励状态。
        /// </summary>
        /// <param name="s">会话对象，包含玩家登录信息。</param>
        /// <param name="req">领取登录奖励的请求对象。</param>
        /// <returns>领取登录奖励的响应对象。</returns>
        public ResRecvLoginBonues HandlerRecvLoginBonues(IdSession s, ReqRecvLoginBonues req) {
            ResRecvLoginBonues res = new ResRecvLoginBonues();

            // 验证玩家账号是否已登录
            if (s.accountId <= 0 || s.playerId <= 0) {
                res.status = (int)Respones.AccountIsNotLogin;
                return res;
            }

            // 从缓存中获取登录奖励数据
            Loginbonues data = PlayerIdLoginBonuesCache.Instance.Get(s.playerId);
            if (data == null) {
                res.status = (int)Respones.SystemErr;
                return res;
            }

            // 检查奖励是否可领取
            if (data.days == 0 || data.status != 0) {
                res.status = (int)Respones.InvalidOpt;
                return res;
            }

            res.status = (int)Respones.OK;
            res.num = data.bonues;

            // 更新玩家金币数量
            Game.Datas.DBEntities.Player p = PlayerAcountIDCache.Instance.Get(s.accountIdAndJob);
            if (p != null) {
                p.ucoin += data.bonues;
            }

            p = PlayerIDCache.Instance.Get((s.playerId));
            if (p != null) {
                p.ucoin += data.bonues;
            }

            if (p != null) {
                PlayerIDCache.Instance.UpdateDataToDb(p);
            }

            // 更新奖励状态为已领取
            data.bonues = 0;
            data.status = 1;
            PlayerIdLoginBonuesCache.Instance.UpdateDataToDb(data);

            return res;
        }

        /// <summary>
        /// 处理选择玩家的请求。
        /// 验证玩家登录状态，检查请求参数，获取或创建玩家数据，检查玩家是否被冻结，
        /// 复制玩家信息到响应对象，关联玩家 ID 和会话，检查登录奖励，创建玩家实体。
        /// </summary>
        /// <param name="s">会话对象，包含玩家登录信息。</param>
        /// <param name="req">选择玩家的请求对象。</param>
        /// <returns>选择玩家的响应对象。</returns>
        public ResSelectPlayer HandlerReqSelectPlayer(IdSession s, ReqSelectPlayer req) {
            ResSelectPlayer res = new ResSelectPlayer();

            // 验证玩家账号是否已登录
            if (s.accountId <= 0) {
                res.status = (int)Respones.AccountIsNotLogin;
                return res;
            }

            // 检查请求参数的有效性
            if (req.job <= 0) {
                res.status = (int)Respones.InvalidParams;
                return res;
            }

            // 获取或创建玩家数据
            Game.Datas.DBEntities.Player p = PlayerAcountIDCache.Instance.GetOrCreate(s.accountId, req);
            if (p == null) {
                res.status = (int)Respones.SystemErr;
                return res;
            }

            // 检查玩家是否被冻结
            if (p.status != 0) {
                res.status = (int)Respones.PlayerIsFreeze;
                return res;
            }

            res.status = (int)Respones.OK;
            res.pInfo = new PlayerInfo();
            this.CopyDbPlayerToResponesData(p, res.pInfo);

            s.playerId = p.id;
            s.accountIdAndJob = PlayerAcountIDCache.Instance.Key(s.accountId, req.job);

            // 检查登录奖励
            this.CheckLoginBonues(s.playerId, res.pInfo);

            // 创建玩家实体
            GM_EntityMgr.Instance.AddPlayer(s.playerId, s.accountId);

            return res;
        }

        /// <summary>
        /// 处理拉取玩家数据的请求。
        /// 验证玩家登录状态，检查请求参数，查找玩家数据，检查玩家是否被冻结，
        /// 复制玩家信息到响应对象，关联玩家 ID 和会话，检查登录奖励，创建玩家实体。
        /// </summary>
        /// <param name="s">会话对象，包含玩家登录信息。</param>
        /// <param name="req">拉取玩家数据的请求对象。</param>
        /// <returns>拉取玩家数据的响应对象。</returns>
        public ResPullingPlayerData HandlerReqPullingPlayerData(IdSession s, ReqPullingPlayerData req) {
            ResPullingPlayerData res = new ResPullingPlayerData();

            // 验证玩家账号是否已登录
            if (s.accountId <= 0) {
                res.status = (int)Respones.AccountIsNotLogin;
                return res;
            }

            int job = req.job;

            // 查找玩家数据
            Game.Datas.DBEntities.Player p = PlayerAcountIDCache.Instance.TryGetPlayer(s.accountId, req.job);
            if (p == null) {
                res.status = (int)Respones.PlayerIsNotExist;
                return res;
            }

            // 检查玩家是否被冻结
            if (p.status != 0) {
                res.status = (int)Respones.PlayerIsFreeze;
                return res;
            }

            res.status = (int)Respones.OK;
            res.pInfo = new PlayerInfo();
            this.CopyDbPlayerToResponesData(p, res.pInfo);

            s.playerId = p.id;
            s.accountIdAndJob = PlayerAcountIDCache.Instance.Key(s.accountId, req.job);

            // 检查登录奖励
            this.CheckLoginBonues(s.playerId, res.pInfo);

            // 创建玩家实体
            GM_EntityMgr.Instance.AddPlayer(s.playerId, s.accountId);

            return res;
        }

        /// <summary>
        /// 检查玩家的每日登录奖励情况。
        /// 根据奖励时间和状态更新奖励信息，并同步到缓存和数据库。
        /// </summary>
        /// <param name="playerId">玩家的 ID。</param>
        /// <param name="res">玩家信息响应对象，用于更新奖励信息。</param>
        private void CheckLoginBonues(long playerId, PlayerInfo res) {
            res.hasBonues = 0;
            Loginbonues data = PlayerIdLoginBonuesCache.Instance.GetOrCreate(playerId);
            if (data == null) {
                logger.Error($"每日登录奖励获取为null: {playerId}");
                return;
            }

            if (data.status == 0) {
                res.hasBonues = 1;
                res.days = data.days;
                res.loginBonues = data.bonues;
            }

            bool hasLoginBonues = (data.bonues_time < UtilsHelper.TimestampToday());
            if (!hasLoginBonues) {
                return;
            }

            bool isSustain = (data.bonues_time < UtilsHelper.TimestampToday()) &&
                (data.bonues_time >= UtilsHelper.TimestampYesterday());

            data.days = (isSustain) ? (data.days + 1) : 1;
            data.bonues = 100;
            data.bonues_time = (int)(UtilsHelper.Timestamp());
            data.status = 0;

            res.hasBonues = 1;
            res.days = data.days;
            res.loginBonues = data.bonues;

            PlayerIdLoginBonuesCache.Instance.UpdateDataToDb(data);
        }

        /// <summary>
        /// 处理拉取奖励列表的请求。
        /// 验证玩家登录状态，根据请求类型拉取奖励数据并转换为响应对象。
        /// </summary>
        /// <param name="s">会话对象，包含玩家登录信息。</param>
        /// <param name="req">拉取奖励列表的请求对象。</param>
        /// <returns>拉取奖励列表的响应对象。</returns>
        public ResPullingBonuesList HandlerReqPullingBonuesList(IdSession s, ReqPullingBonuesList req) {
            ResPullingBonuesList res = new ResPullingBonuesList();

            // 验证玩家账号是否已登录
            if (s.accountId <= 0 || s.playerId <= 0) {
                res.status = (int)Respones.AccountIsNotLogin;
                return res;
            }

            int typeId = req.typeId;

            // 拉取奖励数据
            Bonues[] datas = GM_BonuesMgr.Instance.PullingBonuesData(s.playerId, typeId);
            if (datas != null && datas.Length > 0) {
                res.bonues = new BonuesItem[datas.Length];
                for (int i = 0; i < datas.Length; i++) {
                    res.bonues[i] = new BonuesItem();
                    res.bonues[i].bonuesId = datas[i].id;
                    res.bonues[i].bonuesDesic = datas[i].bonuesDesic;
                    res.bonues[i].status = datas[i].status;
                    res.bonues[i].typeId = datas[i].tid;
                }
            }

            res.status = (int)Respones.OK;

            return res;
        }

        /// <summary>
        /// 处理领取奖励的请求。
        /// 验证玩家登录状态，检查奖励是否可领取，领取奖励并返回响应信息。
        /// </summary>
        /// <param name="s">会话对象，包含玩家登录信息。</param>
        /// <param name="req">领取奖励的请求对象。</param>
        /// <returns>领取奖励的响应对象。</returns>
        public ResRecvBonues HandlerReqRecvBonues(IdSession s, ReqRecvBonues req) {
            ResRecvBonues res = new ResRecvBonues();

            // 验证玩家账号是否已登录
            if (s.accountId <= 0 || s.playerId <= 0) {
                res.status = (int)Respones.AccountIsNotLogin;
                return res;
            }

            // 检查奖励是否存在且可领取
            Bonues data = GM_BonuesMgr.Instance.GetBonuesBy(req.bonuesId);
            if (data == null || data.uid != s.playerId || data.status != 0) {
                res.status = (int)Respones.InvalidOpt;
                return res;
            }

            // 领取奖励
            GM_BonuesMgr.Instance.RecvBonues(req.bonuesId, data);
            res.status = (int)Respones.OK;
            res.typeId = data.tid;
            res.b1 = data.bonues1;
            res.b2 = data.bonues2;
            res.b3 = data.bonues3;
            res.b4 = data.bonues4;
            res.b5 = data.bonues5;

            return res;
        }

        /// <summary>
        /// 处理拉取任务列表的请求。
        /// 验证玩家登录状态，获取玩家实体，根据请求类型拉取任务列表并转换为响应对象。
        /// </summary>
        /// <param name="s">会话对象，包含玩家登录信息。</param>
        /// <param name="req">拉取任务列表的请求对象。</param>
        /// <returns>拉取任务列表的响应对象。</returns>
        public ResPullingTaskList HandlerReqPullingTaskList(IdSession s, ReqPullingTaskList req) {
            ResPullingTaskList res = new ResPullingTaskList();

            // 验证玩家账号是否已登录
            if (s.accountId <= 0 || s.playerId <= 0) {
                res.status = (int)Respones.AccountIsNotLogin;
                return res;
            }

            int typeId = req.typeId;

            // 获取玩家实体
            GM_PlayerEntity player = GM_EntityMgr.Instance.Get(s.playerId);
            if (player == null) {
                res.status = (int)Respones.InvalidOpt;
                return res;
            }

            // 拉取任务列表
            List<GM_Task> tasks = GM_TaskMgr.Instance.PullingTaskList(player, typeId);

            if (tasks != null && tasks.Count > 0) {
                res.tasks = new TaskItem[tasks.Count];
                for (int i = 0; i < tasks.Count; i++) {
                    res.tasks[i] = new TaskItem();
                    res.tasks[i].taskDesic = tasks[i].taskDesic;
                    res.tasks[i].status = tasks[i].dbTaskInst.status;
                    res.tasks[i].typeId = tasks[i].dbTaskInst.tid;
                }
            }

            res.status = (int)Respones.OK;
            return res;
        }

        /// <summary>
        /// 处理更新邮件消息状态的请求。
        /// 验证玩家登录状态，获取玩家实体，更新邮件消息状态。
        /// </summary>
        /// <param name="s">会话对象，包含玩家登录信息。</param>
        /// <param name="req">更新邮件消息状态的请求对象。</param>
        /// <returns>更新邮件消息状态的响应对象。</returns>
        public ResUpdateMailMsg HandlerReqUpdateMailMsg(IdSession s, ReqUpdateMailMsg req) {
            ResUpdateMailMsg res = new ResUpdateMailMsg();

            // 验证玩家账号是否已登录
            if (s.accountId <= 0 || s.playerId <= 0) {
                res.status = (int)Respones.AccountIsNotLogin;
                return res;
            }

            // 获取玩家实体
            GM_PlayerEntity player = GM_EntityMgr.Instance.Get(s.playerId);
            if (player == null) {
                res.status = (int)Respones.InvalidOpt;
                return res;
            }

            // 更新邮件消息状态
            GM_MailMsgMgr.Instance.UpdateMailMsgStatus(req.mailMsgId, req.status);
            res.status = (int)Respones.OK;
            return res;
        }

        /// <summary>
        /// 处理拉取邮件消息的请求。
        /// 此方法会验证玩家的登录状态，获取玩家实体，从数据库拉取邮件消息数据，
        /// 并将其转换为响应对象所需的格式，最终返回拉取邮件消息的响应结果。
        /// </summary>
        /// <param name="s">会话对象，包含玩家的登录信息，如账户 ID 和玩家 ID。</param>
        /// <param name="req">拉取邮件消息的请求对象，可能包含筛选条件等信息。</param>
        /// <returns>拉取邮件消息的响应对象，包含邮件消息列表和处理状态。</returns>
        public ResPullingMailMsg HandlerReqPullingMailMsg(IdSession s, ReqPullingMailMsg req) {
            // 创建拉取邮件消息的响应对象
            ResPullingMailMsg res = new ResPullingMailMsg();

            // 验证玩家账号是否已登录，若未登录则返回对应错误状态
            if (s.accountId <= 0 || s.playerId <= 0) {
                res.status = (int)Respones.AccountIsNotLogin;
                return res;
            }

            // 通过玩家 ID 从实体管理器中获取玩家实体
            GM_PlayerEntity player = GM_EntityMgr.Instance.Get(s.playerId);
            if (player == null) {
                res.status = (int)Respones.InvalidOpt;
                return res;
            }

            // 设置响应状态为成功
            res.status = (int)Respones.OK;

            // 从邮件消息管理器中拉取指定玩家的邮件消息数据，这里的 0 可能是某种默认的筛选条件
            Game.Datas.DBEntities.Mailmessage[] mailMessages = GM_MailMsgMgr.Instance.PullingMailMsg(s.playerId, 0);
            if (mailMessages != null && mailMessages.Length > 0) {
                // 初始化响应对象中的邮件消息列表
                res.mailMessages = new MailMsgItem[mailMessages.Length];
                for (int i = 0; i < mailMessages.Length; i++) {
                    // 创建单个邮件消息项
                    res.mailMessages[i] = new MailMsgItem();
                    // 复制邮件消息的内容
                    res.mailMessages[i].msgBody = mailMessages[i].msgBody;
                    // 复制邮件消息的状态
                    res.mailMessages[i].status = mailMessages[i].status;
                    // 复制邮件消息的发送时间
                    res.mailMessages[i].sendTime = mailMessages[i].sendTime;
                    // 复制邮件消息的 ID
                    res.mailMessages[i].msgId = mailMessages[i].id;
                }
            }

            return res;
        }

        /// <summary>
        /// 处理拉取排行榜数据的请求。
        /// 此方法会验证玩家的登录状态，获取玩家实体，从排行榜管理器中获取指定类型的排行榜数据，
        /// 并将其转换为响应对象所需的格式，同时标记玩家在排行榜中的位置，最终返回拉取排行榜数据的响应结果。
        /// </summary>
        /// <param name="s">会话对象，包含玩家的登录信息，如账户 ID 和玩家 ID。</param>
        /// <param name="req">拉取排行榜数据的请求对象，可能包含排行榜类型等信息。</param>
        /// <returns>拉取排行榜数据的响应对象，包含排行榜数据列表、玩家自身排名和处理状态。</returns>
        public ResPullingRank HandlerReqPullingRank(IdSession s, ReqPullingRank req) {
            // 创建拉取排行榜数据的响应对象
            ResPullingRank res = new ResPullingRank();

            // 验证玩家账号是否已登录，若未登录则返回对应错误状态
            if (s.accountId <= 0 || s.playerId <= 0) {
                res.status = (int)Respones.AccountIsNotLogin;
                return res;
            }

            // 通过玩家 ID 从实体管理器中获取玩家实体
            GM_PlayerEntity player = GM_EntityMgr.Instance.Get(s.playerId);
            if (player == null) {
                res.status = (int)Respones.InvalidOpt;
                return res;
            }

            // 设置响应状态为成功
            res.status = (int)Respones.OK;
            // 初始化玩家自身在排行榜中的排名为 -1，表示未找到
            res.selfIndex = -1;

            // 这里暂时固定排行榜类型为世界金币排行榜，可根据需求修改
            int rankType = (int)RankType.WorldCoin;
            // 从排行榜管理器中获取指定类型的排行榜数据，最多获取前 30 条
            RankData[] rankData = GM_RankMgr.Instance.GetRankData(rankType, 30);
            if (rankData == null || rankData.Length <= 0) {
                return res;
            }

            // 初始化响应对象中的排行榜数据列表
            res.ranks = new RankItem[rankData.Length];
            for (int i = 0; i < rankData.Length; i++) {
                // 创建单个排行榜项
                RankItem item = new RankItem();

                // 通过排行榜数据中的玩家 ID 从玩家 ID 缓存中获取玩家信息
                Player p = PlayerIDCache.Instance.Get(rankData[i].uid);
                if (p == null) {
                    // 若未找到玩家信息，记录错误日志并跳过该条数据
                    this.logger.Error($"rankdata Error uid {rankData[i].uid}");
                    continue;
                }
                // 通过玩家信息中的账户 ID 从账户 ID 缓存中获取账户信息
                Account a = AccountIDCache.Instance.Get(p.accountId);
                if (a == null) {
                    // 若未找到账户信息，记录错误日志并跳过该条数据
                    this.logger.Error($"rankdata Error uid {rankData[i].uid}");
                    continue;
                }
                // 复制账户的头像信息到排行榜项
                item.uface = a.uface;
                // 复制账户的昵称信息到排行榜项
                item.unick = a.unick;
                // 复制排行榜数据中的值到排行榜项
                item.value = rankData[i].value;

                // 检查当前排行榜项的玩家 ID 是否为当前请求玩家的 ID，若是则记录玩家自身排名
                if (rankData[i].uid == s.playerId) {
                    res.selfIndex = i;
                }

                // 将排行榜项添加到响应对象的排行榜数据列表中
                res.ranks[i] = item;
            }

            return res;
        }

        /// <summary>
        /// 处理兑换商品的请求。
        /// 此方法会验证玩家的登录状态，获取玩家实体，检查玩家是否可以兑换指定商品，
        /// 若可以则执行兑换操作，最终返回兑换商品的响应结果。
        /// </summary>
        /// <param name="s">会话对象，包含玩家的登录信息，如账户 ID 和玩家 ID。</param>
        /// <param name="req">兑换商品的请求对象，包含要兑换的商品 ID 等信息。</param>
        /// <returns>兑换商品的响应对象，包含处理状态。</returns>
        public ResExchangeProduct HandlerReqExchangeProduct(IdSession s, ReqExchangeProduct req) {
            // 创建兑换商品的响应对象
            ResExchangeProduct res = new ResExchangeProduct();

            // 验证玩家账号是否已登录，若未登录则返回对应错误状态
            if (s.accountId <= 0 || s.playerId <= 0) {
                res.status = (int)Respones.AccountIsNotLogin;
                return res;
            }

            // 通过玩家 ID 从实体管理器中获取玩家实体
            GM_PlayerEntity player = GM_EntityMgr.Instance.Get(s.playerId);
            if (player == null) {
                res.status = (int)Respones.InvalidOpt;
                return res;
            }

            // 检查玩家是否可以兑换指定商品，返回检查状态
            int status = GM_TradingMgr.Instance.CanExchangeProduct(player, req.productId);
            if (status != (int)Respones.OK) {
                // 若检查不通过，将检查状态设置为响应状态并返回
                res.status = status;
                return res;
            }

            // 若检查通过，执行兑换商品的操作
            GM_TradingMgr.Instance.DoExchangeProduct(player, req.productId);

            // 设置响应状态为成功
            res.status = (int)Respones.OK;
            return res;
        }

        /// <summary>
        /// 处理拉取背包数据的请求。
        /// 此方法会验证玩家的登录状态，获取玩家实体，从背包管理器中获取玩家的背包数据，
        /// 并将其转换为响应对象所需的格式，最终返回拉取背包数据的响应结果。
        /// </summary>
        /// <param name="s">会话对象，包含玩家的登录信息，如账户 ID 和玩家 ID。</param>
        /// <param name="req">拉取背包数据的请求对象，可能包含筛选条件等信息。</param>
        /// <returns>拉取背包数据的响应对象，包含背包物品列表和处理状态。</returns>
        public ResPullingPackData HandlerReqPullingPackData(IdSession s, ReqPullingPackData req) {
            // 创建拉取背包数据的响应对象
            ResPullingPackData res = new ResPullingPackData();

            // 验证玩家账号是否已登录，若未登录则返回对应错误状态
            if (s.accountId <= 0 || s.playerId <= 0) {
                res.status = (int)Respones.AccountIsNotLogin;
                return res;
            }

            // 通过玩家 ID 从实体管理器中获取玩家实体
            GM_PlayerEntity player = GM_EntityMgr.Instance.Get(s.playerId);
            if (player == null) {
                res.status = (int)Respones.InvalidOpt;
                return res;
            }

            // 从背包管理器中获取玩家的背包数据
            Dictionary<int, List<GoodsItem>> ret = GM_BackpackMgr.Instance.GetBackpackData(ref player.uBackpack);
            // 初始化响应对象中的背包物品列表
            res.packGoods = new List<DicGoodsItem>();
            foreach (var key in ret.Keys) {
                // 创建单个背包物品项
                DicGoodsItem dic = new DicGoodsItem();
                // 复制物品的主类型
                dic.mainType = key;
                // 复制物品列表
                dic.Value = ret[key];
                // 将背包物品项添加到响应对象的背包物品列表中
                res.packGoods.Add(dic);
            }

            // 设置响应状态为成功
            res.status = (int)Respones.OK;
            return res;
        }

        /// <summary>
        /// 处理测试更新物品的请求。
        /// 此方法会验证玩家的登录状态，获取玩家实体，调用背包管理器更新指定类型物品的数量，
        /// 最终返回测试更新物品的响应结果。
        /// </summary>
        /// <param name="s">会话对象，包含玩家的登录信息，如账户 ID 和玩家 ID。</param>
        /// <param name="req">测试更新物品的请求对象，包含物品类型 ID 和更新数量等信息。</param>
        /// <returns>测试更新物品的响应对象，包含处理状态。</returns>
        public ResTestUpdateGoods HandlerReqTestUpdatesGoods(IdSession s, ReqTestUpdateGooods req) {
            // 创建测试更新物品的响应对象
            ResTestUpdateGoods res = new ResTestUpdateGoods();

            // 验证玩家账号是否已登录，若未登录则返回对应错误状态
            if (s.accountId <= 0 || s.playerId <= 0) {
                res.status = (int)Respones.AccountIsNotLogin;
                return res;
            }

            // 通过玩家 ID 从实体管理器中获取玩家实体
            GM_PlayerEntity player = GM_EntityMgr.Instance.Get(s.playerId);
            if (player == null) {
                res.status = (int)Respones.InvalidOpt;
                return res;
            }

            // 调用背包管理器更新指定类型物品的数量
            GM_BackpackMgr.Instance.UpdateGoodsWithTid(player, req.typeId, req.num);

            // 设置响应状态为成功
            res.status = (int)Respones.OK;
            return res;
        }

        /// <summary>
        /// 处理测试获取物品的请求。
        /// 此方法会验证玩家的登录状态，获取玩家实体，根据物品类型更新玩家任务进度，
        /// 最终返回测试获取物品的响应结果。
        /// </summary>
        /// <param name="s">会话对象，包含玩家的登录信息，如账户 ID 和玩家 ID。</param>
        /// <param name="req">测试获取物品的请求对象，包含物品类型 ID 和获取数量等信息。</param>
        /// <returns>测试获取物品的响应对象，包含处理状态。</returns>
        public ResTestGetGoods HandlerReqTestGetGoods(IdSession s, ReqTestGetGoods req) {
            // 创建测试获取物品的响应对象
            ResTestGetGoods res = new ResTestGetGoods();

            // 验证玩家账号是否已登录，若未登录则返回对应错误状态
            if (s.accountId <= 0 || s.playerId <= 0) {
                res.status = (int)Respones.AccountIsNotLogin;
                return res;
            }

            // 通过玩家 ID 从实体管理器中获取玩家实体
            GM_PlayerEntity player = GM_EntityMgr.Instance.Get(s.playerId);
            if (player == null) {
                res.status = (int)Respones.InvalidOpt;
                return res;
            }

            // 测试代码：根据物品类型更新玩家任务进度
            if (req.typeId == 1) // 钻石
            {
                // 获取当前钻石收集任务数据
                GM_Task collectTask = GM_TaskMgr.Instance.GetCurrectTaskData(player, 100000);
                if (collectTask != null) {
                    // 更新任务进度
                    GM_TaskMgr.Instance.UpdateTaskProgress(player, collectTask, "damond", req.num);
                }
            } else if (req.typeId == 2) // 天书
              {
                // 获取当前天书收集任务数据
                GM_Task collectTask = GM_TaskMgr.Instance.GetCurrectTaskData(player, 100000);
                if (collectTask != null) {
                    // 更新任务进度
                    GM_TaskMgr.Instance.UpdateTaskProgress(player, collectTask, "book", req.num);
                }
            }

            // 设置响应状态为成功
            res.status = (int)Respones.OK;
            return res;
        }
    }
}