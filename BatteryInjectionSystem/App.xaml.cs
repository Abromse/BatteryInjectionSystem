using BatteryInjection.Common;
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
            Logger.Info("======== 电池注液上位机系统启动 ========");
            this.DispatcherUnhandledException += (sender, args) =>
            {
                string msg = args.Exception.ToString();
                Logger.Error(msg);
                MessageBox.Show($"系统发生未处理异常：\n{args.Exception.Message}", "错误提示", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Exception ex = args.ExceptionObject as Exception;
                if (ex != null)
                {
                    Logger.Error($"【后台线程致命异常】程序即将崩溃！\n{ex.ToString()}");
                    MessageBox.Show($"无法恢复的错误:{ex.Message}", "致命错误", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
            };

           while (true)
            {
                LoginWindow loginWindow = new LoginWindow();
                bool? result = loginWindow.ShowDialog();
                if (result == true)
                {
                    MainWindow main = new MainWindow(loginWindow.loginUserName, loginWindow.role);
                    main.ShowDialog();
                    if (main.DialogResult == true)
                    {
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    Application.Current.Shutdown();
                    break;
                }
            }
        }
        protected override void OnExit(ExitEventArgs e)
        {   
            Logger.Info("======== 电池注液上位机系统已安全退出 ========");
            base.OnExit(e);
        }

    }

}
