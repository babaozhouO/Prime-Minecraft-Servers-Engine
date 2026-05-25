using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Frontend_AvaloniaUI.Models
{
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
        public string Message { get; set; }
        public IBrush Color { get; set; }
    }
}
