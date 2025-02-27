using Framework.Core.Utils;
using Game.Core.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core.GM_MailMessage
{
    /// <summary>
    /// 邮件消息状态的枚举，定义了邮件的三种状态：未读、已读和已删除。
    /// </summary>
    enum MailMsgStatus
    {
        // 未读状态，状态值为 0
        Unreaded = 0,
        // 已读状态
        Readed,
        // 已删除状态
        Deleted,
    }

    /// <summary>
    /// GM_MailMsgMgr 类是一个单例类，用于管理游戏中的邮件消息。
    /// 提供了邮件的发送、状态更新和拉取等功能。
    /// </summary>
    public class GM_MailMsgMgr
    {
        /// <summary>
        /// GM_MailMsgMgr 类的单例实例，方便全局访问。
        /// </summary>
        public static GM_MailMsgMgr Instance = new GM_MailMsgMgr();

        /// <summary>
        /// 初始化方法，目前为空，可用于后续添加初始化逻辑。
        /// </summary>
        public void Init()
        {
        }

        /// <summary>
        /// 发送邮件消息的方法，将邮件信息插入到数据库中。
        /// </summary>
        /// <param name="fromPlayerId">发件人的玩家 ID。</param>
        /// <param name="toPlayerId">收件人的玩家 ID。</param>
        /// <param name="msgBody">邮件的正文内容。</param>
        /// <param name="udata">用户自定义数据，默认为 0。</param>
        public void SendMailMsg(long fromPlayerId, long toPlayerId, string msgBody, long udata = 0)
        {
            // 创建一个新的邮件消息对象
            Game.Datas.DBEntities.Mailmessage msg = new Game.Datas.DBEntities.Mailmessage();
            // 设置发件人的玩家 ID
            msg.fromPlayerId = fromPlayerId;
            // 设置收件人的玩家 ID
            msg.toPlayerId = toPlayerId;
            // 设置邮件的正文内容
            msg.msgBody = msgBody;
            // 设置用户自定义数据
            msg.userData = udata;
            // 设置邮件的初始状态为未读
            msg.status = (int)MailMsgStatus.Unreaded;
            // 设置邮件的发送时间为当前时间戳
            msg.sendTime = (int)UtilsHelper.Timestamp();
            // 初始时读取时间设为 -1，表示未读取
            msg.readTime = -1;

            // 将邮件消息对象插入到数据库中，异步执行
            DBService.Instance.GetGameInstance().Insertable(msg).ExecuteCommandAsync();
        }

        /// <summary>
        /// 更新邮件消息状态的方法，验证状态合法性并更新到数据库。
        /// </summary>
        /// <param name="msgId">邮件消息的 ID。</param>
        /// <param name="status">要更新的邮件状态。</param>
        /// <returns>如果状态更新成功返回 true，否则返回 false。</returns>
        public bool UpdateMailMsgStatus(long msgId, int status)
        {
            // 验证状态是否在合法范围内
            if (status < (int)MailMsgStatus.Unreaded ||
                status > (int)MailMsgStatus.Deleted)
            {
                return false;
            }

            // 从数据库中查询指定 ID 的邮件消息
            Game.Datas.DBEntities.Mailmessage mailMsg = null;
            mailMsg = DBService.Instance.GetGameInstance().Queryable<Game.Datas.DBEntities.Mailmessage>().First(it => it.id == msgId);
            // 如果邮件的当前状态已经是要更新的状态，则直接返回 true
            if (mailMsg.status == status)
            {
                return true;
            }

            // 更新邮件的状态
            mailMsg.status = status;
            // 如果状态更新为已读，则记录读取时间为当前时间戳
            if (status == (int)MailMsgStatus.Readed)
            {
                mailMsg.readTime = (int)UtilsHelper.Timestamp();
            }
            // 将更新后的邮件消息对象更新到数据库中，异步执行
            DBService.Instance.GetGameInstance().Updateable<Game.Datas.DBEntities.Mailmessage>(mailMsg).Where(it => it.id == msgId).ExecuteCommandAsync();
            return true;
        }

        /// <summary>
        /// 拉取指定玩家的邮件消息的方法，根据状态掩码获取不同状态的邮件。
        /// </summary>
        /// <param name="playerId">玩家的 ID。</param>
        /// <param name="statusMask">状态掩码，0 表示拉取未读邮件，1 表示拉取未读和已读邮件，2 表示拉取所有邮件。</param>
        /// <returns>符合条件的邮件消息数组。</returns>
        public Game.Datas.DBEntities.Mailmessage[] PullingMailMsg(long playerId, int statusMask)
        {
            Game.Datas.DBEntities.Mailmessage[] mailMsg = null;
            if (statusMask == 2)
            {
                // 拉取该玩家的所有邮件消息
                mailMsg = DBService.Instance.GetGameInstance().Queryable<Game.Datas.DBEntities.Mailmessage>().Where(it => it.toPlayerId == playerId).ToArray();
            }
            else if (statusMask == 1)
            {
                // 拉取该玩家的未读和已读邮件消息
                mailMsg = DBService.Instance.GetGameInstance().Queryable<Game.Datas.DBEntities.Mailmessage>().Where(it => ((it.toPlayerId == playerId) && (it.status < 2))).ToArray();
            }
            else if (statusMask == 0)
            {
                // 拉取该玩家的未读邮件消息
                mailMsg = DBService.Instance.GetGameInstance().Queryable<Game.Datas.DBEntities.Mailmessage>().Where(it => ((it.toPlayerId == playerId) && (it.status < 1))).ToArray();
            }
            return mailMsg;
        }
    }
}