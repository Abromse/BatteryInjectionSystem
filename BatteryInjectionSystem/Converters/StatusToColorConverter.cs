using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace BatteryInjectionSystem.Converters
{
    public partial class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string status = value?.ToString();         
            if (status != null && (status.Contains("成功") || status.Contains("已连接")))
            {
                return Brushes.Green;
            }
            else if (status != null && (status.Contains("失败") || status.Contains("断开") || status.Contains("错误")))
            {
                return Brushes.Red;
            }
            return Brushes.Orange;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
