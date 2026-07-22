using Avalonia.Threading;
using LiveChartsCore.Defaults;
using PMCSsE_Communicator.DataPacks.Pack_nothing;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;

namespace PMCSsE_Frontend_AvaloniaUI.ViewModels
{
    public class CPUChartData(string nameAndID, DateTimePoint firstPoint)
    {
        public string NameAndID { get; } = nameAndID;
        public ObservableCollection<DateTimePoint> CPUPoints { get; } = [firstPoint];
    }
    public partial class MainViewModel
    {
        public readonly DispatcherTimer UpdateChartDataTimer = new() { Interval = TimeSpan.FromSeconds(1), IsEnabled = true };
        private void UpdateChartDataTimer_Tick(object? sender, EventArgs e)
        {
            UpdateChartDataTimer.Stop();
            if (RequireToGetServerState == true && nativeClient != null)
            {
                nativeClient.RequestBackend(PMCSsE_Communicator.RequestTypeEnum.GetServerState, new Pack_GetServerState());
            }
            UpdateChartDataTimer.Start();
        }
        public bool? RequireToGetServerState
        {
            get => _requireToGetServerState;
            set => this.RaiseAndSetIfChanged(ref _requireToGetServerState, value);
        }
        private bool? _requireToGetServerState = true;
        public object ChartSyncLock { get; } = new();
        public ObservableCollection<CPUChartData> CPUSeries { get; } = [];
        public ObservableCollection<DateTimePoint> MemorySerie { get; } = [];
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
