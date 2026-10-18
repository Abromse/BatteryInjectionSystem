using BatteryInjection.DataAccess;
using BatteryInjection.Device;
using BatteryInjection.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace BatteryInjectionSystem.ViewModels
{
    public partial class MainViewModel: ObservableObject
    {
        private readonly DbHelper _dbHelper = new();
        private readonly PlcService _plcService = new();
        private readonly DispatcherTimer _timer = new();
        private BatteryCell? _currentCell;
        private readonly WeightDeviceService _weightService = new();
        #region 属性绑定
        [ObservableProperty] private string _ip = "127.0.0.1";
        [ObservableProperty] private string _port = "502";
        [ObservableProperty] private string _slaveId = "1";
        [ObservableProperty] private string _startAddress = "0";
        [ObservableProperty] private string _count = "2";
        [ObservableProperty] private string _barcode = "";

        // 状态/结果显示属性
        [ObservableProperty] private string _resultText = "等待操作...";
        [ObservableProperty] private string _beforeWeight = "--";
        [ObservableProperty] private string _afterWeight = "--";
        [ObservableProperty] private string _injectionAmount = "--";

        //TCP
        [ObservableProperty] private string _weightIp = "127.0.0.1";
        [ObservableProperty] private string _weightPort = "8888";
        [ObservableProperty] private string _currentWeight = "--";

        // 表格数据源
        public ObservableCollection<InjectionRecord> Records { get; } = new();
        #endregion

        public MainViewModel()
        {
            // 初始化定时器：每秒读取一次 PLC
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _dbHelper.InitDatabase();
            RefreshRecords();
        }
        //命令区
        [RelayCommand]
        private async Task ConnectAsync()
        {
            var ok = await _plcService.ConnectAsync(Ip, int.Parse(Port));
            ResultText = ok ? "连接成功" : "连接失败";
        }

        [RelayCommand]
        private void Disconnect()
        {
            _timer.Stop();
            _plcService.Disconnect();
            ResultText = "已断开";
        }

        [RelayCommand]
        private void ReadRegisters()
        {
            try
            {
                // 1. 拿数据
                var values = _plcService.ReadHoldingRegisters(byte.Parse(SlaveId), ushort.Parse(StartAddress), ushort.Parse(Count));

                // 2. 拼界面
                var result = "读取结果：\n";
                for (int i = 0; i < values.Length; i++)
                {
                    result += $"地址 {int.Parse(StartAddress) + i}：{values[i]}\n";
                }
                ResultText = result;
            }
            catch (Exception ex)
            {
                // 3. 兜底防崩
                ResultText = $"读取失败：{ex.Message}";
            }
        }
        [RelayCommand]
        private void StartCollect()
        {
            if (!_plcService.IsConnected)
            {
                MessageBox.Show("请先连接 PLC");
                return;
            }
            _timer.Start();
        }

        [RelayCommand]
        private void StopCollect()
        {
            _timer.Stop();
        }

        [RelayCommand]
        private async Task InjectAsync()
        {
            if (string.IsNullOrEmpty(Barcode))
            {
                MessageBox.Show("请输入电芯条码");
                return;
            }

            try
            {
                _dbHelper.InsertCell(new BatteryCell { Barcode = Barcode, Model = "IFR18650-2000", CreatedAt = DateTime.Now });

                var beforeValues = _plcService.ReadHoldingRegisters(1, 0, 1);
                var beforeWeight = beforeValues[0];

                _plcService.WriteSingleRegister(1, 10, 1);
                await Task.Delay(3000);

                var afterValues = _plcService.ReadHoldingRegisters(1, 1, 1);
                var afterWeight = afterValues[0];

                var amount = afterWeight - beforeWeight;
                var isPass = amount > 0 && amount < 500;

                _dbHelper.InsertInjectionRecord(new InjectionRecord
                {
                    Barcode = Barcode,
                    BeforeWeight = beforeWeight,
                    AfterWeight = afterWeight,
                    InjectionAmount = amount,
                    IsPass = isPass,
                    CreatedAt = DateTime.Now
                });

                RefreshRecords();
                MessageBox.Show($"注液完成！注液量：{amount}，判定：{(isPass ? "合格" : "不合格")}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"注液流程出错：{ex.Message}");
            }
        }

        [RelayCommand]
        private void RefreshRecords()
        {
            Records.Clear();
            var list = _dbHelper.GetAllInjectionRecords();
            foreach (var item in list) Records.Add(item);
        }

        // ================= 私有辅助方法 =================
        private void Timer_Tick(object? sender, EventArgs e)
        {
            try
            {
                var values = _plcService.ReadHoldingRegisters(1, 0, 2);
                BeforeWeight = values[0].ToString();
                AfterWeight = values[1].ToString();

                if (values[0] > 0 && values[1] > 0)
                {
                    InjectionAmount = (values[1] - values[0]).ToString();
                }
            }
            catch (Exception ex)
            {
                // 如果报错，直接显示在界面的 ResultText 上，并停止定时器
                ResultText = $"采集失败：{ex.Message}";
                _timer.Stop();
            }
        }
          // ==== Day 5 新增：连接称重设备命令 ====
    [RelayCommand]
    private async Task ConnectWeightAsync()
    {
        // 先订阅事件（防止重复订阅，先减再加）
        _weightService.WeightReceived -= OnWeightReceived;
        _weightService.WeightReceived += OnWeightReceived;

        var ok = await _weightService.ConnectAsync(WeightIp, int.Parse(WeightPort));
        ResultText = ok ? "称重仪连接成功" : "称重仪连接失败";
    }

    [RelayCommand]
    private void DisconnectWeight()
    {
        _weightService.Disconnect();
        ResultText = "称重仪已断开";
    }

    // ==== Day 5 新增：接收数据回调 ====
    private void OnWeightReceived(decimal weight)
    {
        // 注意：事件是在后台线程触发的，需要切回 UI 线程更新界面
        App.Current.Dispatcher.Invoke(() =>
        {
            CurrentWeight = weight.ToString("F2"); // 保留两位小数
        });
    }
    }
}
