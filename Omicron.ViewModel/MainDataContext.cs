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
        public virtual string[] PartsItems { set; get; } = new string[6] { "A1","A2","A3", "B1", "B2", "B3" };
        private MessagePrint messagePrint = new MessagePrint();
        private dialog mydialog = new dialog();
        public void ChoseHomePage()
        {
            AboutPageVisibility = "Collapsed";
            HomePageVisibility = "Visible";
            Msg = messagePrint.AddMessage("Selected HomePage");
        }
        public void ChoseAboutPage()
        {
            AboutPageVisibility = "Visible";
            HomePageVisibility = "Collapsed";
        }

        //[Export(MEF.Contracts.ActionMessage)]
        //[ExportMetadata(MEF.Key, "winclose")]
        //public async void WindowClose()
        //{
        //    mydialog.changeaccent("Red");
        //    var r = await mydialog.showconfirm("确定要关闭程序吗？");
        //    if (r)
        //    {
        //        System.Windows.Application.Current.Shutdown();
        //    }
        //    else
        //    {
        //        mydialog.changeaccent("Cobalt");
        //    }
        //}
    }
}