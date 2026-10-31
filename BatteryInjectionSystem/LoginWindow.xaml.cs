using BatteryInjection.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BatteryInjectionSystem
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        private bool _isPasswordVisible = false;
        public string role { get; set; }="";
        public string loginUserName { get; set; } = "";
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void TogglePasswordVisibility(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;
            if (_isPasswordVisible)
            {
                pwdBorder.Visibility = Visibility.Collapsed; 
                txtBorder.Visibility = Visibility.Visible;
                pwdtext.Text = pwdBox.Password;
                pwdBox.Visibility = Visibility.Collapsed;
                pwdtext.Visibility = Visibility.Visible;
                EyeIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Eye;
                pwdtext.Focus();
                pwdtext.CaretIndex = pwdtext.Text.Length;
            }
            else
            {
                txtBorder.Visibility = Visibility.Collapsed;
                pwdBorder.Visibility = Visibility.Visible;
                pwdBox.Password = pwdtext.Text;
                pwdtext.Visibility = Visibility.Collapsed;
                pwdBox.Visibility = Visibility.Visible;
                EyeIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.EyeOff;
                pwdtext.Focus();   
            }
        }

        private void Login(object sender, RoutedEventArgs e)
        {
            string user = usertext.Text.Trim();
            string pwd = "";
            if (pwdBorder.Visibility == Visibility.Visible)
            {
                pwd = pwdBox.Password.Trim();  // 密码框可见，拿密码框的
            }
            else
            {
                pwd = pwdtext.Text.Trim();     // 明文框可见，拿明文框的
            }
            var dbhelper = new DbHelper();
            dbhelper.InitDatabase();
            dbhelper.IniUser();
            var loginuser = dbhelper.Login(user, pwd);
            if(loginuser !=null)
            {
                loginUserName = loginuser.Username;
                role = loginuser.Role;
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("输入的账号或者密码错误，请重新输入");
            }
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void GuestLogin(object sender, RoutedEventArgs e)
        {
            var dbhelper = new DbHelper();
            dbhelper.InitDatabase();
            dbhelper.IniUser();
            dbhelper.IniCommonUser();            
            var guestuser = dbhelper.GetGuestUser();
            if (guestuser != null)
            {
                loginUserName = guestuser.Username;
                role = guestuser.Role;
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("输入的账号或者密码错误，请重新输入");
            }
        }
    }
}
