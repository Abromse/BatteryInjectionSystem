using BatteryInjection.DataAccess;
using BatteryInjection.Device;
using BatteryInjection.Models;
using BatteryInjectionSystem.ViewModels;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BatteryInjectionSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow(string userName,string role)
        {
            InitializeComponent();
           var vm = new ViewModels.MainViewModel(userName,role);
            vm.LoginRequest += () =>
            {
                DialogResult = true;
                Close();
            };
            DataContext =vm;
        }
        protected override void OnClosed(EventArgs e)
        {
            // 如果 DataContext 是 MainViewModel，就调用它的 Shutdown 方法
            if (DataContext is MainViewModel vm)
            {
                vm.Shutdown();
            }
            base.OnClosed(e);
        }
    }
}