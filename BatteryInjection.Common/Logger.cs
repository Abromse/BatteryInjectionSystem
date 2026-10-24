
namespace BatteryInjection.Common
{
    public class Logger
    {
        private static readonly string LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Logs");
        public static void Info(string message) 
        {
            WriteLog("Info", message);
        }
        public static void Error(string message)
        {
            WriteLog("Error", message);
        }
        public static void WriteLog(string level,string message)
        {
            try
            {
                if(!Directory.Exists(LogDirectory))
                {
                    Directory.CreateDirectory(LogDirectory);
                 }
                string fileName = DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
                string filePath = Path.Combine(LogDirectory, fileName);
                string logContent = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}][{level}]{message}{Environment.NewLine}";
                File.AppendAllText(filePath, logContent);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Logger] –¥»’÷æ ß∞‹: {ex.Message}");
            }
        
        }
    }

}
