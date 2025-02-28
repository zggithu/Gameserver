using Framework.Core.Net;
using Framework.Core.Serializer;
using Framework.Core.task;
using Framework.Core.Utils;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Core.task
{
    /// <summary>
    /// 逻辑服务器的基类，提供了服务器初始化、事件处理和主循环等核心功能。
    /// 具体的逻辑服务器可以继承该类并实现相应的抽象方法。
    /// </summary>
    public abstract class BaseLogicServer
    {
        // 使用 NLog 记录日志，获取当前类的日志记录器
        protected NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        // 存储逻辑事件的队列，用于处理网络事件
        protected Queue<LogicEvent> eventQueue = null;
        // 存储 OnUpdate 方法的 MethodInfo 对象，用于反射调用
        MethodInfo OnUpdate = null;

        // 目标帧率，默认值为 20
        protected int fps = 20;
        // 每帧的时间间隔（毫秒）
        protected long timePerFrame = 0;

        // 记录上一帧的时间戳（毫秒）
        private long lastFrameMillis;

        // 存储命令号与处理方法的映射关系，用于快速查找处理方法
        private Dictionary<int, MethodInfo> keyMethodDic;

        /// <summary>
        /// 设置服务器的目标帧率。
        /// </summary>
        /// <param name="fps">目标帧率。</param>
        public void SetTargetFPS(int fps)
        {
            this.fps = fps;
            // 计算每帧的时间间隔（毫秒）
            this.timePerFrame = 1000 / fps;
        }

        /// <summary>
        /// 根据模块号和命令号构建一个唯一的键。
        /// </summary>
        /// <param name="module">模块号。</param>
        /// <param name="cmd">命令号。</param>
        /// <returns>构建的键。</returns>
        private int BuildKey(short module, short cmd)
        {
            return module * 10000 + cmd;
        }

        /// <summary>
        /// 初始化逻辑服务器。
        /// </summary>
        /// <param name="eventQueue">用于存储逻辑事件的队列。</param>
        public virtual void Init(Queue<LogicEvent> eventQueue)
        {
            // 初始化事件队列
            this.eventQueue = eventQueue;
            // 设置目标帧率为 20
            this.SetTargetFPS(20);

            // 获取当前类的类型
            Type t = this.GetType();
            // 获取 OnUpdate 方法的 MethodInfo 对象
            this.OnUpdate = t.GetMethod("OnUpdate");

            // 获取 OnStart 方法的 MethodInfo 对象
            MethodInfo OnStart = t.GetMethod("OnStart");
            if (OnStart != null)
            {
                // 调用 OnStart 方法
                OnStart.Invoke(this, null);
            }

            // 初始化命令号与处理方法的映射字典
            this.keyMethodDic = new Dictionary<int, MethodInfo>();
            // 获取当前类的所有公共方法
            MethodInfo[] methods = t.GetMethods();
            foreach (MethodInfo method in methods)
            {
                // 获取方法上的 RequestMapping 特性
                RequestMapping mapperAttribute = method.GetCustomAttribute<RequestMapping>();
                if (mapperAttribute == null)
                {
                    // 如果方法没有 RequestMapping 特性，则跳过
                    continue;
                }

                // 获取消息的元数据（模块号和命令号）
                short[] meta = MessageDispatcher.Instance.GetMessageMeta(method);
                short cmd = meta[1];

                if (keyMethodDic.ContainsKey(cmd))
                {
                    // 如果命令号已经存在于字典中，抛出异常
                    throw new RuntimeBinderException($"[致命错误]:[{cmd}]重复注册");
                }

                // 将命令号与处理方法添加到字典中
                keyMethodDic.Add(cmd, method);
            }

            // 记录当前时间戳作为上一帧的时间
            this.lastFrameMillis = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        /// <summary>
        /// 检查服务器是否正在运行。
        /// </summary>
        /// <returns>如果服务器正在运行，返回 true；否则返回 false。</returns>
        public bool IsAppRunning()
        {
            return LogicWorkerPool.Instance.Running;
        }

        /// <summary>
        /// 处理网络事件队列中的所有事件。
        /// </summary>
        private void ProcessNetEvents()
        {
            // 锁定事件队列，确保线程安全
            lock (this.eventQueue)
            {
                while (this.eventQueue.Count > 0)
                {
                    // 从队列中取出一个事件
                    LogicEvent e = this.eventQueue.Dequeue();

                    if (this.keyMethodDic.ContainsKey(e.cmd))
                    {
                        // 根据命令号查找对应的处理方法并调用
                        var ret = this.keyMethodDic[e.cmd].Invoke(this, new object[] { e.s, e.msg });
                        if (ret != null)
                        {
                            // 如果处理方法有返回值，将返回的消息推送给客户端
                            MessagePusher.PushMessage(e.s, ret as Message);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 逻辑服务器的主循环，控制服务器的运行。
        /// </summary>
        public void MainLoop()
        {
            while (this.IsAppRunning())
            {
                // 获取当前时间戳（毫秒）
                long nowMillis = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

                if (nowMillis - this.lastFrameMillis < this.timePerFrame)
                {
                    // 如果距离上一帧的时间小于每帧的时间间隔，线程休眠等待
                    int sleepTime = (int)(this.timePerFrame - (nowMillis - this.lastFrameMillis));
                    Thread.Sleep(sleepTime);
                    // 重新获取当前时间戳
                    nowMillis = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                }

                // 计算当前帧与上一帧的时间差（毫秒）
                long dt = nowMillis - this.lastFrameMillis;
                // 更新上一帧的时间戳
                this.lastFrameMillis = nowMillis;

                // 处理所有的网络事件
                this.ProcessNetEvents();

                // 调用 OnUpdate 方法进行游戏逻辑的迭代
                if (this.OnUpdate != null)
                {
                    // 将时间差转换为秒
                    float duration = (float)dt / 1000.0f;
                    // 调用 OnUpdate 方法并传入时间差
                    this.OnUpdate.Invoke(this, new object[] { duration });
                }
            }
        }
    }
}