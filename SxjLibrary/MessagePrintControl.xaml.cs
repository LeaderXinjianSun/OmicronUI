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

namespace SxjLibrary
{
    /// <summary>
    /// MessagePrintControl.xaml 的交互逻辑
    /// </summary>
    public partial class MessagePrintControl : UserControl
    {
        public MessagePrintControl()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty MsgTextPropety = DependencyProperty.Register("MsgText",typeof(string),typeof(MessagePrintControl));
        public string MsgText
        {
            get { return (string)GetValue(MsgTextPropety); }
            set { SetValue(MsgTextPropety, value); }
        }
        private void MsgTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            MsgTextBox.ScrollToEnd();
        }
    }
}
