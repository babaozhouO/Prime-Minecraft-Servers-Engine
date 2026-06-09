using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator
{
    /// <summary>
    /// 来自DeepSeek-v4-pro，实现了消息传递功能
    /// </summary>
    public class DataPackBus
    {
        private readonly ConcurrentDictionary<Type, Delegate> _handlers = new();
        internal event Action<string> PulishedDataType = delegate { };
        /// <summary>
        /// 订阅指定类型的消息。当该类型的消息被发布时，handler 将被调用。
        /// 支持同一类型多个订阅者，handler 会被依次调用。
        /// </summary>
        /// <typeparam name="T">消息类型。</typeparam>
        /// <param name="handler">处理消息的回调委托。</param>
        public void Subscribe<T>(Action<T> handler)
        {
            _handlers.AddOrUpdate(
                typeof(T),
                handler,
                (_, existing) => Delegate.Combine(existing, handler));
        }

        /// <summary>
        /// 取消订阅指定类型的消息。使用循环重试机制保证并发更新成功。
        /// </summary>
        /// <typeparam name="T">消息类型。</typeparam>
        /// <param name="handler">要移除的处理回调委托。</param>
        public void Unsubscribe<T>(Action<T> handler)
        {
            var key = typeof(T);
            // 用循环保证并发更新成功（通常 1~2 次就完成）
            while (true)
            {
                if (!_handlers.TryGetValue(key, out var existing))
                    return; // 没有订阅，直接退出

                var newDel = Delegate.Remove(existing, handler);

                if (newDel == null)
                {
                    // 没有剩余订阅者，直接尝试删除整个键
                    if (_handlers.TryRemove(key, out _))
                        return;
                    // 删除失败（被其他线程修改），继续重试
                }
                else
                {
                    // 还有订阅者，更新为移除后的委托链
                    if (_handlers.TryUpdate(key, newDel, existing))
                        return;
                    // 更新失败（被其他线程抢先），继续重试
                }
            }
        }

        /// <summary>
        /// 发布指定类型的消息。所有订阅了该类型的 handler 将被同步调用。
        /// 发布时会触发 PulishedDataType 事件记录消息类型。
        /// </summary>
        /// <typeparam name="T">消息类型。</typeparam>
        /// <param name="message">要发布的消息实例。</param>
        public void Publish<T>(T message)
        {
            if (_handlers.TryGetValue(typeof(T), out var del) && del is Action<T> action)
            {
                PulishedDataType($"收到包：{typeof(T)}");
                action(message); 

            }
        }
        /// <summary>
        /// 释放所有已注册的订阅处理程序，清空内部字典。
        /// </summary>
        public void Dispose()
        {
            _handlers.Clear();
        }
    }
}

