using System.Configuration;
using System.Data;
using System.Reflection;
using System.Windows;

namespace BatteryInjectionSystem
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // 拦截程序集加载：当 SqlSugar 试图加载 Microsoft.Data.Sqlite 10.0.11 时，
            // 强制返回我们项目里实际引用的 8.0.31 版本
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var assemblyName = new AssemblyName(args.Name);
                if (assemblyName.Name == "Microsoft.Data.Sqlite")
                {
                    // 从当前程序集所在目录加载 8.0.31 的 dll
                    var path = System.IO.Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Microsoft.Data.Sqlite.dll");
                    if (System.IO.File.Exists(path))
                    {
                        return Assembly.LoadFrom(path);
                    }
                }
                return null;
            };

            base.OnStartup(e);
        }
    }

}
