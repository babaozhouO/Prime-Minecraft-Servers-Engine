using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Frontend_AvaloniaUI.ViewModels
{
    public partial class MainViewModel
    {
        // X 轴日期格式化器
        public Func<DateTime, string> DateFormatter { get; set; } =
            date => date.ToString("HH:mm:ss");
        public Func<double, string> PercentLabeler { get; set; } =
            value => $"{value:F1}%";
        public Func<double, string> MemoryLabeler { get; set; } =
            value => $"{value:F1}GB";
        public double? TotalMemory
        {
            get => _totalMemory;
            set => this.RaiseAndSetIfChanged(ref _totalMemory, value);
        }
        private double? _totalMemory = 16.0d;
    }

}
