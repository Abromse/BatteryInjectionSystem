using BatteryInjection.DataAccess;
using BatteryInjection.Models;
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

namespace BatteryInjectionSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DbHelper _dbHelper = new();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. 建表
                _dbHelper.InitDatabase();

                // 2. 插入一条测试数据
                var cell = new BatteryCell
                {
                    Barcode = "CELL-20260101-001",
                    Model = "IFR18650-2000",
                    CreatedAt = DateTime.Now
                };
                _dbHelper.InsertCell(cell);

                // 3. 查询出来
                var list = _dbHelper.GetAllCells();

                // 4. 显示结果
                var result = $"数据库初始化成功！共 {list.Count} 条电芯记录：\n";
                foreach (var item in list)
                {
                    result += $"ID={item.Id}, 条码={item.Barcode}, 型号={item.Model}, 时间={item.CreatedAt}\n";
                }
                TxtResult.Text = result;
            }
            catch (Exception ex)
            {
                TxtResult.Text = $"出错：{ex.Message}";
            }
        }
    }
}