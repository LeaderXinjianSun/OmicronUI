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
using System.IO.Ports;

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

        public virtual ObservableCollection<string> Ports { set; get; } = new ObservableCollection<string>();
        public virtual string PortName { set; get; }
        public virtual bool IsPLCConnect { set; get; }
        public virtual string ModbusState { set; get; }
        public virtual bool FindFill1 { set; get; }
        public virtual bool FindFill2 { set; get; }
        public virtual bool FindFill3 { set; get; }
        public virtual bool FindFill4 { set; get; }
        public virtual bool FindFill5 { set; get; }
        public virtual bool FindFill6 { set; get; }
        public virtual bool FindMo1 { set; get; }
        public virtual bool FindMo2 { set; get; }
        public virtual bool FindMo3 { set; get; }
        public virtual bool FindMo4 { set; get; }
        public virtual bool FindMo5 { set; get; }
        public virtual bool FindMo6 { set; get; }
        #endregion
        #region 变量
        private HdevEngine hdevEngine = new HdevEngine();
        private string iniParameterPath = System.Environment.CurrentDirectory + "\\Parameter.ini";
        private TaiDaPLC td;
        #endregion
        #region 构造函数
        public MainDataContext()
        {
            //td.ReConnectUp += ReConnectUpEventHandle;
        }
        #endregion
        #region 事件响应函数
        private void ReConnectUpEventHandle()
        {
            IsPLCConnect = false;
        }
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
            var fill1 = hdevEngine.getmeasurements("fill1");
            var fill2 = hdevEngine.getmeasurements("fill2");
            var fill3 = hdevEngine.getmeasurements("fill3");
            var fill4 = hdevEngine.getmeasurements("fill4");
            var fill5 = hdevEngine.getmeasurements("fill5");
            var fill6 = hdevEngine.getmeasurements("fill6");

            var fill7 = hdevEngine.getmeasurements("fill7");
            var fill8 = hdevEngine.getmeasurements("fill8");
            var fill9 = hdevEngine.getmeasurements("fill9");
            var fill10 = hdevEngine.getmeasurements("fill10");
            var fill11 = hdevEngine.getmeasurements("fill11");
            var fill12 = hdevEngine.getmeasurements("fill12");

            FindFill1 = (fill1.ToString() == "1") & (fill7.ToString() == "1");
            FindFill2 = (fill2.ToString() == "1") & (fill8.ToString() == "1");
            FindFill3 = (fill3.ToString() == "1") & (fill9.ToString() == "1");
            FindFill4 = (fill4.ToString() == "1") & (fill10.ToString() == "1");
            FindFill5 = (fill5.ToString() == "1") & (fill11.ToString() == "1");
            FindFill6 = (fill6.ToString() == "1") & (fill12.ToString() == "1");

            var mo1 = hdevEngine.getmeasurements("mo1");
            var mo2 = hdevEngine.getmeasurements("mo2");
            var mo3 = hdevEngine.getmeasurements("mo3");
            var mo4 = hdevEngine.getmeasurements("mo4");
            var mo5 = hdevEngine.getmeasurements("mo5");
            var mo6 = hdevEngine.getmeasurements("mo6");

            FindMo1 = mo1.ToString() == "0";
            FindMo2 = mo2.ToString() == "0";
            FindMo3 = mo3.ToString() == "0";
            FindMo4 = mo4.ToString() == "0";
            FindMo5 = mo5.ToString() == "0";
            FindMo6 = mo6.ToString() == "0";

            objectList.Add(hdevEngine.getRegion("Regions1"));
            objectList.Add(hdevEngine.getRegion("Regions2"));
            objectList.Add(hdevEngine.getRegion("Regions3"));
            objectList.Add(hdevEngine.getRegion("Regions4"));
            objectList.Add(hdevEngine.getRegion("Regions5"));
            objectList.Add(hdevEngine.getRegion("Regions6"));

            //objectList.Add(hdevEngine.getRegion("Regions_Intensity1"));
            //objectList.Add(hdevEngine.getRegion("Regions_Intensity2"));
            //objectList.Add(hdevEngine.getRegion("Regions_Intensity3"));
            //objectList.Add(hdevEngine.getRegion("Regions_Intensity4"));
            //objectList.Add(hdevEngine.getRegion("Regions_Intensity5"));
            //objectList.Add(hdevEngine.getRegion("Regions_Intensity6"));

            //objectList.Add(hdevEngine.getRegion("Regions_Intensity7"));
            //objectList.Add(hdevEngine.getRegion("Regions_Intensity8"));
            //objectList.Add(hdevEngine.getRegion("Regions_Intensity9"));
            //objectList.Add(hdevEngine.getRegion("Regions_Intensity10"));
            //objectList.Add(hdevEngine.getRegion("Regions_Intensity11"));
            //objectList.Add(hdevEngine.getRegion("Regions_Intensity12"));
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
        public void SaveParameter()
        {

            var r1 = WriteParameter();
            if (r1)
            {
                Msg = messagePrint.AddMessage("写入参数成功");
            }
            else
            {
                Msg = messagePrint.AddMessage("写入参数成功");
            }
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
            CameraHcInspect();
            Msg = messagePrint.AddMessage("检测相机初始化完成");
            td = new TaiDaPLC(PortName, 19200, System.IO.Ports.Parity.Even, 7, System.IO.Ports.StopBits.One);
            td.ReConnectUp += ReConnectUpEventHandle;
            await Task.Delay(100);
        }
        #endregion
        #region 读写操作
        private bool ReadParameter()
        {
            try
            {
                HcVisionScriptFileName = Inifile.INIGetStringValue(iniParameterPath, "Camera", "HcVisionScriptFileName", @"C:\test.hdev");
                PortName = Inifile.INIGetStringValue(iniParameterPath, "SerialPort", "Com", "COM1");
                ModbusState = Inifile.INIGetStringValue(iniParameterPath, "SerialPort", "ModbusState", "01");
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
                Inifile.INIWriteValue(iniParameterPath, "SerialPort", "Com", PortName);
                Inifile.INIWriteValue(iniParameterPath, "SerialPort", "ModbusState", ModbusState);
                return true;
            }
            catch (Exception ex)
            {
                Log.Default.Error("WriteParameter", ex);
                return false;
            }
        }
        #endregion
        #region PLC

        [Initialize]
        public async void RunPLC()
        {
            while (true)
            {
                await Task.Delay(100);
                try
                {                    
                    if (td == null) continue;
                    if (!td.State)
                    {
                        td.Connect();
                    }
                    if (td.State)
                    {
                        if (!td.ReadM(ModbusState, "M1000"))
                        {
                            td.Closed();
                            td.State = false;
                            td = new TaiDaPLC(PortName, 19200, System.IO.Ports.Parity.Even, 7, System.IO.Ports.StopBits.One);
                        }
                        else
                        {
                            //if (td.ReadM(StartAction))
                            //{
                            //    td.SetM(StartAction, false);
                            //    try
                            //    {
                            //        Inspect();
                            //    }
                            //    catch
                            //    {
                            //        td.SetM(Position1, false);
                            //        td.SetM(Position2, false);
                            //        td.SetM(Position3, false);
                            //        td.SetM(Position4, false);
                            //        td.SetM(Position5, false);
                            //        td.SetM(Position6, false);

                            //    }
                            //    td.SetM(EndAction, true);
                            //}
                        }
                    }

                    
                }
                catch 
                {
                    td.State = false;

                }
                IsPLCConnect = td.State;

            }
        }

        #endregion

    }
}