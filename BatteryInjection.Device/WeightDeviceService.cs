using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BatteryInjection.Device
{
    public class WeightDeviceService
    {
        private TcpClient? _tcpClient;
        private NetworkStream? _stream;
        private CancellationTokenSource? _cts;

        // 定义一个事件，当收到称重数据时触发
        public event Action<decimal>? WeightReceived;

        public bool IsConnected => _tcpClient?.Connected == true;

        public async Task<bool> ConnectAsync(string ip, int port)
        {
            try
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(ip, port);
                _stream = _tcpClient.GetStream();
                _cts = new CancellationTokenSource();

                // 开启后台线程持续接收数据
                _ = Task.Run(() => ReceiveDataAsync(_cts.Token));
                return true;
            }
            catch
            {
                return false;
            }
        }
        public void Disconnect()
        {
            _cts?.Cancel();
            _stream?.Close();
            _tcpClient?.Close();
            _tcpClient = null;
            _stream = null;
        }
        private async Task ReceiveDataAsync(CancellationToken token)
        {
            var buffer = new byte[1024];
            try
            {
                while (!token.IsCancellationRequested)
                {
                    int bytesRead = await _stream!.ReadAsync(buffer, token);
                    if (bytesRead == 0) break; // 连接断开

                    // 假设协议格式为："ST,+,00123.45,g\r\n"
                    string rawData = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();

                    // 解析重量（截取中间的数字部分）
                    var parts = rawData.Split(',');
                    if (parts.Length >= 3 && decimal.TryParse(parts[2], out decimal weight))
                    {
                        // 如果有正负号处理
                        if (parts[1] == "-") weight = -weight;

                        // 触发事件通知 ViewModel
                        WeightReceived?.Invoke(weight);
                    }
                }
            }
            catch
            {
                // 忽略断开时的异常
            }
        }
    }
}
