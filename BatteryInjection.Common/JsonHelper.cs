using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BatteryInjection.Common
{
    public class JsonHelper
    {
        public static AppConfig LoadConfig()
        {
            string fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            if (!File.Exists(fileName))
            {
                Logger.Error("未读取到配置信息，采用默认配置");
                return new AppConfig();
            }
            try
            {
                string JsonString = File.ReadAllText(fileName);
                var config = JsonSerializer.Deserialize<AppConfig>(JsonString);
                return config ?? new AppConfig();
            }
            catch (Exception ex)
            {
                Logger.Error($"读取配置文件失败：{ex.Message}");
                return new AppConfig();
            }
        }
    }
}
