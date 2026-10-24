using BatteryInjection.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BatteryInjection.Device
{
    public class UdpService
    {
        private UdpClient? _udpClient;
        private CancellationTokenSource? _cts;
        public event Action<string>? MessageReceived;

        public void Connect(int port)
        {
            try
            {
                _udpClient = new UdpClient(port);
                _cts = new CancellationTokenSource();
                _ = Task.Run(() => ReceiveData(_cts.Token));

                Debug.WriteLine($"[UDP] 服务已启动，监听端口 {port}");
            }
            catch (Exception ex)
            {

                throw new Exception($"UDP 启动失败，端口 {port} 可能被占用。错误：{ex.Message}");
            }
        }

        public void Stop()
        {
            _cts.Cancel();
            _udpClient?.Dispose();  
            _udpClient = null;
        }
        private async Task ReceiveData(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var buffer = await _udpClient.ReceiveAsync(token);
                    var result = Encoding.UTF8.GetString(buffer.Buffer);
                    MessageReceived?.Invoke(result);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"错误信息:{ex.Message}");
            }
        }
    }
}
