using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatteryInjection.Common
{
    public class AppConfig
    {
        public string? PlcIp { get; set; }
        public int PlcPort { get; set; }
        public int TcpPort { get; set; }
        public int UdpPort { get; set; }
        public string? MesUrl { get; set; }
    }
}
