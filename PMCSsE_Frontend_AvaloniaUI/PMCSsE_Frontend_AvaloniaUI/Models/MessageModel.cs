using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Frontend_AvaloniaUI.Models
{
    /// <summary>
    /// 消息视图模型，包含消息文本和根据日志级别映射的颜色。
    /// </summary>
    public class MessageModel
    {
        internal MessageModel(string message, byte level)
        {
            Message = message;
            Color = level switch
            {
                0 => new SolidColorBrush { Color = Colors.White },
                1 => new SolidColorBrush { Color = Colors.Yellow },
                2 => new SolidColorBrush { Color = Colors.Red },
                3 => new SolidColorBrush { Color = new Color(255,124,252,0) },
                _ => new SolidColorBrush { Color = Colors.White },
            };
        }
        /// <summary>
        /// 消息文本内容。
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 消息显示颜色笔刷。
        /// </summary>
        public IBrush Color { get; set; }
    }
}
