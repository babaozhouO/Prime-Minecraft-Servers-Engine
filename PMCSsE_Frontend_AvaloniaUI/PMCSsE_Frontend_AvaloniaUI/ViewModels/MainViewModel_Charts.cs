using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace PMCSsE_Frontend_AvaloniaUI.ViewModels
{
    public partial class MainViewModel
    {
        public bool ChartInitialized = false;
        public object SyncLock { get; } = new();
        public ISeries[] CPUSeries { get; set; } = [];
        public ObservableCollection<DateTimePoint> CPUPoints { get; } = [new DateTimePoint(DateTime.Today, 20), new DateTimePoint(DateTime.Now, 50)];
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
