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
        // 订阅
        public void Subscribe<T>(Action<T> handler)
        {
            _handlers.AddOrUpdate(
                typeof(T),
                handler,
                (_, existing) => Delegate.Combine(existing, handler));
        }

        // 取消订阅（修复版）
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

        // 发布
        public void Publish<T>(T message)
        {
            if (_handlers.TryGetValue(typeof(T), out var del) && del is Action<T> action)
            {
                PulishedDataType($"收到包：{typeof(T)}");
                action(message); 

            }
        }
        public void Dispose()
        {
            _handlers.Clear();
        }
    }
}

