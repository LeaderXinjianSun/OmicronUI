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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using BingLibrary.hjb;
using SxjLibrary;

namespace Omicron
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private dialog mydialog = new dialog();
        private string iniParameterPath = System.Environment.CurrentDirectory + "\\Parameter.ini";
        public MainWindow()
        {
            InitializeComponent();
            #region 判断系统是否已启动

            System.Diagnostics.Process[] myProcesses = System.Diagnostics.Process.GetProcessesByName("Omicron");//获取指定的进程名   
            if (myProcesses.Length > 1) //如果可以获取到知道的进程名则说明已经启动
            {
                System.Windows.MessageBox.Show("不允许重复打开软件");
                System.Windows.Application.Current.Shutdown();
            }


            #endregion
        }

        private async void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            //ActionMessages.ExecuteAction("winclose");
            mydialog.changeaccent("Red");
            MainGrid1.Visibility = Visibility.Collapsed;
            var r = await mydialog.showconfirm("确定要关闭程序吗？");
            if (r)
            {
                System.Windows.Application.Current.Shutdown();
            }
            else
            {
                MainGrid1.Visibility = Visibility.Visible;
                mydialog.changeaccent("Cobalt");
            }
        }

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            mydialog.changeaccent("Red");
            MainGrid1.Visibility = Visibility.Collapsed;
            string str = await mydialog.showinput("请输入作业员编号");
            if (str == "")
            {
                System.Windows.Application.Current.Shutdown();
            }
            else
            {
                Inifile.INIWriteValue(iniParameterPath, "SQLMSG", "BLUID", str);
                Inifile.INIWriteValue(iniParameterPath, "Loaded", "isChangeUserId", "True");
                mydialog.changeaccent("Cobalt");
                MainGrid1.Visibility = Visibility.Visible;
            }
        }
    }
}
