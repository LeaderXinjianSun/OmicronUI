using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BingLibrary.hjb;
using BingLibrary.hjb.Intercepts;
using System.ComponentModel.Composition;
using SxjLibrary;
using System.Windows;
using System.Collections.ObjectModel;


namespace Omicron.ViewModel
{
    [BingAutoNotify]
    public class MainDataContext : DataSource
    {
        public virtual string AboutPageVisibility { set; get; } = "Collapsed";
        public virtual string HomePageVisibility { set; get; } = "Visible";
        public virtual string Msg { set; get; } = "";
        public virtual string[] MatraxStyles { set; get; } = new string[3] { "3×3", "4×3", "5×3" };
        public virtual int MatraxStylesIndex { set; get; } = 0;
        public virtual int RsN { set; get; } = -1;
        public virtual int SelectedItemNum { set; get; } = -1;
        private MessagePrint messagePrint = new MessagePrint();
        private dialog mydialog = new dialog();
        public void ChoseHomePage()
        {
            AboutPageVisibility = "Collapsed";
            HomePageVisibility = "Visible";
            Msg = messagePrint.AddMessage("111");
        }
        public void ChoseAboutPage()
        {
            AboutPageVisibility = "Visible";
            HomePageVisibility = "Collapsed";
        }
        public void Test()
        {
            RsN++;
            if (RsN > 2)
            {
                RsN = 0;
            }
        }
        [Initialize]
        public void Init()
        {
            SelectedItemNum = 3;
            RsN = 3;
        }
        [Export(MEF.Contracts.ActionMessage)]
        [ExportMetadata(MEF.Key, "winclose")]
        public async void WindowClose()
        {
            mydialog.changeaccent("Red");
            var r = await mydialog.showconfirm("确定要关闭程序吗？");
            if (r)
            {
                System.Windows.Application.Current.Shutdown();
            }
            else
            {
                mydialog.changeaccent("Cobalt");
            }
        }
    }
}