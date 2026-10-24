using BatteryInjection.Common;
using NModbus;
using NModbus.Device;
using System.Net.Sockets;

namespace BatteryInjection.Device
{
    public class PlcService
    {
        private TcpClient? _tcpClient;
        private IModbusMaster? _master;

        public bool IsConnected => _tcpClient?.Connected == true;

        /// <summary>
        /// 连接 Modbus TCP 从站
        /// </summary>  
        public async Task<bool> ConnectAsync(string ip, int port)
        {
            try
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(ip, port);
                var factory = new ModbusFactory();  
                _master = factory.CreateMaster(_tcpClient);
                return true;
            }
            catch(Exception ex)
            {
                Logger.Error($"PLC连接失败,错误信息:{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            _master?.Dispose();
            _tcpClient?.Close();
            _tcpClient = null;
            _master = null;
        }

        /// <summary>
        /// 读取保持寄存器（功能码 03）
        /// </summary>
        public ushort[] ReadHoldingRegisters(byte slaveId, ushort startAddress, ushort numRegisters)
        {
            if (_master == null) throw new InvalidOperationException("未连接");
            return _master.ReadHoldingRegisters(slaveId, startAddress, numRegisters);
        }

        /// <summary>
        /// 写单个寄存器（功能码 06）
        /// </summary>
        public void WriteSingleRegister(byte slaveId, ushort address, ushort value)
        {
            if (_master == null) throw new InvalidOperationException("未连接");
            _master.WriteSingleRegister(slaveId, address, value);
        }

    }

}
