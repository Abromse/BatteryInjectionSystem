using BatteryInjection.Common;
using BatteryInjection.DataAccess;
using BatteryInjection.Device;
using BatteryInjection.MES.Client;
using BatteryInjection.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace BatteryInjectionSystem.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        public event Action LoginRequest;
        private readonly DbHelper _dbHelper = new();
        private readonly PlcService _plcService = new();
        private readonly DispatcherTimer _timer = new();
        private readonly MesApiService _mesApiService;
        private BatteryCell? _currentCell;
        private readonly WeightDeviceService _weightService = new();
        private readonly UdpService _udpService = new();
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
        [ObservableProperty] private string _mesReflection = "--";

        //TCP
        [ObservableProperty] private string _weightIp = "127.0.0.1";
        [ObservableProperty] private string _weightPort = "8888";
        [ObservableProperty] private string _currentWeight = "--";

        [ObservableProperty] private string _udpStatus = "UDP未启动";
        [ObservableProperty] private string _userName = "";
        // 表格数据源
        public ObservableCollection<InjectionRecord> Records { get; } = new();

        [ObservableProperty] private InjectionRecord _selectedRecord;
        [ObservableProperty] private bool _isAdmin;
        #endregion

        public MainViewModel(string username,string role)
        {       
            _userName = username;
            _isAdmin = (role == "Admin");
            var config = JsonHelper.LoadConfig();
            Ip = config.PlcIp;
            Port = config.PlcPort.ToString();
            WeightIp = config.PlcIp;
            WeightPort = config.TcpPort.ToString();
            _mesApiService = new MesApiService(config.MesUrl);
            var result = File.ReadAllText("config.json");
            var changed = JsonSerializer.Deserialize<InjectionRecord>(result);
            // 初始化定时器：每秒读取一次 PLC
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            RefreshRecords();
            _udpService.MessageReceived += (msg) =>
            {
                Logger.Info($"底层收到了UDP消息：{msg}");
                App.Current.Dispatcher?.Invoke(() =>
                {
                    UdpStatus = $"收到UDP心跳: {msg}";
                });
            };
            _udpService.Connect(9000);
        }
        public void Shutdown()
        {
            // 停止定时器
            _timer.Stop();

            // 停止 UDP 监听
            _udpService.Stop();

            // 断开 TCP 称重仪
            _weightService.Disconnect();

            // 断开 PLC
            _plcService.Disconnect();

            Logger.Info("程序正在安全退出...");
        }

        //命令区
        [RelayCommand]
        private async Task ConnectAsync()
        {
            var ok = await _plcService.ConnectAsync(Ip, int.Parse(Port));
            ResultText = ok ? "连接成功" : "连接失败";
            if (ok)
            {
                Logger.Info($"成功连接 PLC，IP: {Ip}, 端口: {Port}");
            }
            else
            {
                Logger.Info($"连接PLC，IP: {Ip}, 端口: {Port}失败");
            }
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

        [RelayCommand(AllowConcurrentExecutions = false)]
        private async Task InjectAsync()
        {
            if (string.IsNullOrEmpty(Barcode))
            {
                MessageBox.Show("请输入电芯条码");
                return;
            }

            try
            {
                _dbHelper.InsertCell(new BatteryCell { Barcode = Barcode, Model = " ", CreatedAt = DateTime.Now });

                var beforeValues = _plcService.ReadHoldingRegisters(1, 0, 1);
                var beforeWeight = beforeValues[0];

                _plcService.WriteSingleRegister(1, 5, 1);
                await Task.Delay(3000);

                var afterValues = _plcService.ReadHoldingRegisters(1, 1, 1);
                var afterWeight = afterValues[0];

                var amount = afterWeight - beforeWeight;
                var isPass = amount > 0 && amount < 500;
                Logger.Info($"电芯 {Barcode} 注液后重量：{afterWeight}，实际注液量：{amount}，判定：{(isPass ? "合格" : "不合格")}");
                var record = new InjectionRecord
                {
                    Barcode = Barcode,
                    BeforeWeight = beforeWeight,
                    AfterWeight = afterWeight,
                    InjectionAmount = amount,
                    IsPass = isPass,
                    CreatedAt = DateTime.Now
                };
                _dbHelper.InsertInjectionRecord(record);
                RefreshRecords();
                bool isReportSuccess = await _mesApiService.ReportInjectionAsync(record);
                if (isReportSuccess)
                    Logger.Info($"电芯 {Barcode} 数据成功上报 MES");
                else
                    Logger.Error($"电芯 {Barcode} 数据上报 MES 失败！");
                if (isReportSuccess)
                {
                    MesReflection = "注液完成，且已成功上报MES！";
                }
                else
                {
                    MesReflection = "注液完成，但上报MES失败！";
                }
                MessageBox.Show($"注液完成！注液量：{amount}，判定：{(isPass ? "合格" : "不合格")}");
            }
            catch (Exception ex)
            {
                Logger.Error($"注液流程出错,条码:{Barcode},错误信息:{ex.Message}");
                MessageBox.Show($"注液流程出错：{ex.ToString()}");
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
        [RelayCommand]
        private void Exit()
        {
            Application.Current.Shutdown();
        }

        [RelayCommand(CanExecute =nameof(CanDelete))]
        private void Delete(InjectionRecord record)
        {
            if (record == null) return;
            var result = MessageBox.Show($"确认删除条码为{record.Barcode}的记录吗？", "删除提示", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {

                    _dbHelper.Delete(record.Id);
                    Logger.Info($"用户删除了条码为 {record.Barcode} 的记录");
                    RefreshRecords();
                }
                catch (Exception ex)
                {
                    Logger.Error($"删除失败:{ex.Message}");
                    MessageBox.Show($"删除失败:{ex.Message}");
                }
            }  
        }
        [RelayCommand]
        private void LoginOut()
        {
            var result = MessageBox.Show("确定要注销账号吗？", "信息提示", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                LoginRequest?.Invoke();
            }   
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
        private bool CanDelete()
        {
            return IsAdmin;
        }
        partial void OnIsAdminChanged(bool value)
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
