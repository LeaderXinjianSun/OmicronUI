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
using ViewROI;
using HalconDotNet;
using System.IO;
using System.Windows.Forms;

namespace Omicron.ViewModel
{
    [BingAutoNotify]
    public class MainDataContext : DataSource
    {
        #region 属性
        public virtual string AboutPageVisibility { set; get; } = "Collapsed";
        public virtual string HomePageVisibility { set; get; } = "Visible";
        //ParameterPageVisibility
        public virtual string ParameterPageVisibility { set; get; } = "Collapsed";
        public virtual string Msg { set; get; } = "";
        private MessagePrint messagePrint = new MessagePrint();
        private dialog mydialog = new dialog();

        public virtual HImage hImage { set; get; }
        public virtual ObservableCollection<HObject> hObjectList { set; get; }
        public virtual ObservableCollection<ROI> ROIList { set; get; } = new ObservableCollection<ROI>();
        public virtual int ActiveIndex { set; get; }
        public virtual bool Repaint { set; get; }
        public virtual string HcVisionScriptFileName { set; get; }
        #endregion
        #region 变量
        private HdevEngine hdevEngine = new HdevEngine();
        private string iniParameterPath = System.Environment.CurrentDirectory + "\\Parameter.ini";
        #endregion
        #region 画面切换
        public void ChoseHomePage()
        {
            AboutPageVisibility = "Collapsed";
            HomePageVisibility = "Visible";
            ParameterPageVisibility = "Collapsed";
            Msg = messagePrint.AddMessage("Selected HomePage");
        }
        public void ChoseAboutPage()
        {
            AboutPageVisibility = "Visible";
            HomePageVisibility = "Collapsed";
            ParameterPageVisibility = "Collapsed";
        }
        public void ChoseParameterPage()
        {
            AboutPageVisibility = "Collapsed";
            HomePageVisibility = "Collapsed";
            ParameterPageVisibility = "Visible";
        }
        #endregion
        #region Halcon
        public void cameraHcInit()
        {
            string filename = System.IO.Path.GetFileName(HcVisionScriptFileName);
            string fullfilename = System.Environment.CurrentDirectory + @"\" + filename;
            if (!(File.Exists(fullfilename)))
            {
                File.Copy(HcVisionScriptFileName, fullfilename);
            }
            else
            {
                FileInfo fileinfo1 = new FileInfo(HcVisionScriptFileName);
                FileInfo fileinfo2 = new FileInfo(fullfilename);
                TimeSpan ts = fileinfo1.LastWriteTime - fileinfo2.LastWriteTime;
                if (ts.TotalMilliseconds > 0)
                {
                    File.Copy(HcVisionScriptFileName, fullfilename, true);
                }
            }
            hdevEngine.initialengine(System.IO.Path.GetFileNameWithoutExtension(fullfilename));
            hdevEngine.loadengine();
        }
        public void CameraHcInspect()
        {
            Async.RunFuncAsync(cameraHcInspect, null);
        }
        public void cameraHcInspect()
        {
            ObservableCollection<HObject> objectList = new ObservableCollection<HObject>();
            hdevEngine.inspectengine();
            hImage = hdevEngine.getImage("Image");
            //var fill1 = hdevEngine.getmeasurements("fill1");
            //var fill2 = hdevEngine.getmeasurements("fill2");
            //var fill3 = hdevEngine.getmeasurements("fill3");
            //var fill4 = hdevEngine.getmeasurements("fill4");
            //var fill5 = hdevEngine.getmeasurements("fill5");
            //var fill6 = hdevEngine.getmeasurements("fill6");
            //FindFill1 = fill1.ToString() == "1";
            //FindFill2 = fill2.ToString() == "1";
            //FindFill3 = fill3.ToString() == "1";
            //FindFill4 = fill4.ToString() == "1";
            //FindFill5 = fill5.ToString() == "1";
            //FindFill6 = fill6.ToString() == "1";
            //objectList.Add(hdevEngine.getRegion("Regions1"));
            //objectList.Add(hdevEngine.getRegion("Regions2"));
            //objectList.Add(hdevEngine.getRegion("Regions3"));
            //objectList.Add(hdevEngine.getRegion("Regions4"));
            //objectList.Add(hdevEngine.getRegion("Regions5"));
            //objectList.Add(hdevEngine.getRegion("Regions6"));
            hObjectList = objectList;
        }

        #endregion
        #region 功能与方法
        public void Selectfile(object p)
        {

            OpenFileDialog dlg = new OpenFileDialog();
            switch (p.ToString())
            {
                case "1":
                    dlg.Filter = "视觉文件(*.hdev)|*.hdev|所有文件(*.*)|*.*";
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        HcVisionScriptFileName = dlg.FileName;
                        Inifile.INIWriteValue(iniParameterPath, "Camera", "HcVisionScriptFileName", HcVisionScriptFileName);
                    }
                    break;
                default:
                    break;
            }
            //dlg.InitialDirectory = System.Environment.CurrentDirectory;

            dlg.Dispose();
        }
        #endregion
        #region 初始化
        [Initialize]
        public async void WindowLoaded()
        {
            var r = ReadParameter();
            if (r)
            {
                Msg = messagePrint.AddMessage("读取参数成功");
            }
            else
            {
                Msg = messagePrint.AddMessage("读取参数失败");
            }
            cameraHcInit();
            await Task.Delay(100);
        }
        #endregion
        #region 读写操作
        private bool ReadParameter()
        {
            try
            {
                HcVisionScriptFileName = Inifile.INIGetStringValue(iniParameterPath, "Camera", "HcVisionScriptFileName", @"C:\test.hdev");

                return true;
            }
            catch (Exception ex)
            {
                Log.Default.Error("ReadParameter", ex);
                return false;
            }
        }
        private bool WriteParameter()
        {
            try
            {

                return true;
            }
            catch (Exception ex)
            {
                Log.Default.Error("WriteParameter", ex);
                return false;
            }
        }
        #endregion

    }
}