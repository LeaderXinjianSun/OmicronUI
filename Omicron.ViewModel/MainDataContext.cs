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
using 臻鼎科技OraDB;
using Omicron.Model;
using System.Data;
using System.Windows.Threading;
using OfficeOpenXml;
using System.Net;

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
        public virtual string ScanOperatePageVisibility { set; get; } = "Collapsed";
        public virtual string BarcodeRecordVisibility { set; get; } = "Collapsed";
        public virtual string AlarmRecodePageVisibility { set; get; } = "Collapsed";
        public virtual string Msg { set; get; } = "";
        private MessagePrint messagePrint = new MessagePrint();
        private dialog mydialog = new dialog();
        public virtual string LastReUpdateStr { set; get; }

        public virtual ObservableCollection<CA9SQLDATA> BarcodeRecord { set; get; } = new ObservableCollection<CA9SQLDATA>();
        public virtual ObservableCollection<AlarmTableItem> AlarmRecord { set; get; } = new ObservableCollection<AlarmTableItem>();
        Queue<CA9SQLDATA> _BarcodeRecord = new Queue<CA9SQLDATA>();
        Queue<AlarmTableItem> AlarmTableItemQueue = new Queue<AlarmTableItem>();
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
        public virtual string LoginButtonString { set; get; } = "登录";
        public virtual string LoginUserName { set; get; } = "Leader";
        public virtual string LoginPassword { set; get; } = "jsldr";
        public virtual bool isLogin { set; get; } = false;
        public virtual string isHideView { set; get; } = "Visible";

        public virtual string ScanPortName { set; get; }
        public virtual string BLID { set; get; }
        public virtual string BLUID { set; get; }
        public virtual string BLMID { set; get; }
        public virtual string BLNAME { set; get; }

        public virtual bool IsScanConnect { set; get; }
        public virtual bool IsTCPConnect { set; get; }

        public virtual string BarcodeString { set; get; } = "G5Y709504KWHP195YZM3";

        public virtual string SQL_ora_server { set; get; }
        public virtual string SQL_ora_user { set; get; }
        public virtual string SQL_ora_pwd { set; get; }

        public virtual DataTable PanelDt { set; get; }
        public virtual DataTable SinglDt { set; get; }

        public virtual string BarcodeRecordSaveFolderPath { set; get; }
        public virtual string AlarmSaveFolderPath { set; get; }

        public virtual ushort SQLReUpdateCount { set; get; } = 0;

        public virtual int YieldCount { set; get; }
        public virtual int AlarmCount { set; get; }
        public virtual double AlmPer { set; get; }

        public static DispatcherTimer dispatcherTimer = new DispatcherTimer();

        #endregion
        #region 变量
        private HdevEngine hdevEngine = new HdevEngine();
        private string iniParameterPath = System.Environment.CurrentDirectory + "\\Parameter.ini";
        private TaiDaPLC td;
        private string StartAction = "M120";
        private string EndAction = "M127";
        private string Position1 = "M121";
        private string Position2 = "M122";
        private string Position3 = "M123";
        private string Position4 = "M124";
        private string Position5 = "M125";
        private string Position6 = "M126";


        private string Position7 = "M211";
        private string Position8 = "M212";
        private string Position9 = "M213";
        private string Position10 = "M214";
        private string Position11 = "M215";
        private string Position12 = "M216";
        static string[] arrField = new string[1];
        static string[] arrValue = new string[1];
        private DateTimeUtility.SYSTEMTIME lastReUpdate = new DateTimeUtility.SYSTEMTIME();
        private bool isChangeUserId = false;
        private bool isChangeUserId1 = false;
        private string AlarmLastDateNameStr = "";

        bool Alarm_allowClean = true;

        int AlarmLastClearHourofYear = 0;
        bool isIn8or20 = false;
        bool _isIn8or20 = false;

        readonly AsyncLock m_lock = new AsyncLock();
        //using (var releaser = await m_lock.LockAsync())
        //{
        //    await FileIO.WriteTextAsync(configureFile, jsonString);
        //}
        int updateCount = 0;
        #endregion
        #region 构造函数
        public MainDataContext()
        {
            //td.ReConnectUp += ReConnectUpEventHandle;

            Scan.StateChanged += Scan_StateChanged;
            dispatcherTimer.Tick += new EventHandler(DispatcherTimerTickUpdateUi);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
            try
            {
                ScanPortName = Inifile.INIGetStringValue(iniParameterPath, "SerialPort", "ScanCom", "COM1");
                Scan.ini(ScanPortName);
                Scan.Connect();
                Msg = messagePrint.AddMessage("扫码枪 连接成功");
            }
            catch
            {
                Msg = messagePrint.AddMessage("扫码枪 连接失败");

            }

        }


        #endregion
        #region 事件响应函数
        private void ReConnectUpEventHandle()
        {
            IsPLCConnect = false;
        }
        private void Scan_StateChanged(object sender, EventArgs e)
        {
            IsScanConnect = Scan.State;
        }
        private async void DispatcherTimerTickUpdateUi(Object sender, EventArgs e)
        {
            if (isChangeUserId1 == false)
            {
                isChangeUserId = bool.Parse(Inifile.INIGetStringValue(iniParameterPath, "Loaded", "isChangeUserId", "False"));
            }

            if (isChangeUserId)
            {
                isChangeUserId = false;
                isChangeUserId1 = true;
                Inifile.INIWriteValue(iniParameterPath, "Loaded", "isChangeUserId", isChangeUserId.ToString());
                BLUID = Inifile.INIGetStringValue(iniParameterPath, "SQLMSG", "BLUID", "Null");
            }
            if (_BarcodeRecord.Count > 0)
            {
                lock (this)
                {
                    foreach (CA9SQLDATA item in _BarcodeRecord)
                    {
                        BarcodeRecord.Add(item);
                    }
                    _BarcodeRecord.Clear();
                }
            }
            if (AlarmTableItemQueue.Count > 0)
            {
                lock (this)
                {
                    foreach (AlarmTableItem item in AlarmTableItemQueue)
                    {
                        AlarmRecord.Add(item);
                    }
                    AlarmTableItemQueue.Clear();
                }
            }

            DateTimeUtility.SYSTEMTIME ds1 = new DateTimeUtility.SYSTEMTIME();
            DateTimeUtility.GetLocalTime(ref ds1);
            TimeSpan ts1 = ds1.ToDateTime() - lastReUpdate.ToDateTime();
            if (ts1.TotalHours > 4)
            {
                string filepath2 = BarcodeRecordSaveFolderPath + @"\" + "NotUpdate" + ".csv";
                ReUpdateBar(filepath2, false);


                DateTimeUtility.GetLocalTime(ref lastReUpdate);
                SaveLastSamplTimetoIni();
                LastReUpdateStr = lastReUpdate.ToDateTime().ToString();
            }

            if (AlarmLastClearHourofYear > DateTime.Now.DayOfYear * 24 + DateTime.Now.Hour)
            {
                AlarmLastClearHourofYear = 0;
                Inifile.INIWriteValue(iniParameterPath, "Alarm", "AlarmLastClearHourofYear", AlarmLastClearHourofYear.ToString());
            }
            isIn8or20 = DateTime.Now.Hour == 8 || DateTime.Now.Hour == 20;
            if (isIn8or20 != _isIn8or20)
            {
                if (isIn8or20)
                {
                    if (AlarmLastClearHourofYear == DateTime.Now.DayOfYear * 24 + DateTime.Now.Hour)
                    {
                        Alarm_allowClean = bool.Parse(Inifile.INIGetStringValue(iniParameterPath, "Alarm", "Alarm_allowClean", "False"));
                        if (Alarm_allowClean)
                        {
                            AutoClean();
                            Alarm_allowClean = false;
                            Inifile.INIWriteValue(iniParameterPath, "Alarm", "Alarm_allowClean", Alarm_allowClean.ToString());
                            AlarmLastClearHourofYear = DateTime.Now.DayOfYear * 24 + DateTime.Now.Hour;
                            Inifile.INIWriteValue(iniParameterPath, "Alarm", "AlarmLastClearHourofYear", AlarmLastClearHourofYear.ToString());
                        }
                    }
                    else
                    {
                        AutoClean();
                        Alarm_allowClean = true;
                        Inifile.INIWriteValue(iniParameterPath, "Alarm", "Alarm_allowClean", Alarm_allowClean.ToString());
                        AlarmLastClearHourofYear = DateTime.Now.DayOfYear * 24 + DateTime.Now.Hour;
                        Inifile.INIWriteValue(iniParameterPath, "Alarm", "AlarmLastClearHourofYear", AlarmLastClearHourofYear.ToString());
                    }
                }
                else
                {
                    Alarm_allowClean = true;
                    Inifile.INIWriteValue(iniParameterPath, "Alarm", "Alarm_allowClean", Alarm_allowClean.ToString());
                }
                _isIn8or20 = isIn8or20;
            }
            if (DateTime.Now.DayOfYear * 24 + DateTime.Now.Hour - AlarmLastClearHourofYear > 12)
            {
                AutoClean();
                Alarm_allowClean = true;
                Inifile.INIWriteValue(iniParameterPath, "Alarm", "Alarm_allowClean", Alarm_allowClean.ToString());
                if (DateTime.Now.Hour >= 8 && DateTime.Now.Hour < 20)
                {
                    AlarmLastClearHourofYear = DateTime.Now.DayOfYear * 24 + 8;
                }
                else
                {
                    if (DateTime.Now.Hour >= 0 && DateTime.Now.Hour < 8)
                    {
                        AlarmLastClearHourofYear = (DateTime.Now.DayOfYear - 1) * 24 + 20;
                    }
                    else
                    {
                        AlarmLastClearHourofYear = DateTime.Now.DayOfYear * 24 + 20;
                    }
                }
                Inifile.INIWriteValue(iniParameterPath, "Alarm", "AlarmLastClearHourofYear", AlarmLastClearHourofYear.ToString());
            }

            updateCount++;
            if (updateCount > 10)
            {
                updateCount = 0;
                using (var releaser = await m_lock.LockAsync())
                {
                    await UploadtoDt();
                }
            }
        }
        public void AutoClean()
        {
            AlarmCount = 0;
            YieldCount = 0;
            AlmPer = 0;
            Inifile.INIWriteValue(iniParameterPath, "Alarm", "AlarmCount", AlarmCount.ToString());
            Inifile.INIWriteValue(iniParameterPath, "Alarm", "YieldCount", YieldCount.ToString());
            Inifile.INIWriteValue(iniParameterPath, "Alarm", "AlmPer", AlmPer.ToString());
        }
        #endregion
        #region 画面切换
        public void ChoseHomePage()
        {
            AboutPageVisibility = "Collapsed";
            HomePageVisibility = "Visible";
            ParameterPageVisibility = "Collapsed";
            ScanOperatePageVisibility = "Collapsed";
            BarcodeRecordVisibility = "Collapsed";
            AlarmRecodePageVisibility = "Collapsed";
            //Msg = messagePrint.AddMessage("Selected HomePage");
            isLogin = false;
            LoginButtonString = "登录";
        }
        public void ChoseScanOperatePage()
        {
            AboutPageVisibility = "Collapsed";
            HomePageVisibility = "Collapsed";
            ParameterPageVisibility = "Collapsed";
            ScanOperatePageVisibility = "Visible";
            BarcodeRecordVisibility = "Collapsed";
            AlarmRecodePageVisibility = "Collapsed";
            //Msg = messagePrint.AddMessage("Selected HomePage");
            isLogin = false;
            LoginButtonString = "登录";
        }
        public void ChoseAboutPage()
        {
            AboutPageVisibility = "Visible";
            HomePageVisibility = "Collapsed";
            ParameterPageVisibility = "Collapsed";
            ScanOperatePageVisibility = "Collapsed";
            BarcodeRecordVisibility = "Collapsed";
            AlarmRecodePageVisibility = "Collapsed";
            isLogin = false;
            LoginButtonString = "登录";
        }
        public void ChoseBarcodeRecord()
        {
            AboutPageVisibility = "Collapsed";
            HomePageVisibility = "Collapsed";
            ParameterPageVisibility = "Collapsed";
            ScanOperatePageVisibility = "Collapsed";
            BarcodeRecordVisibility = "Visible";
            AlarmRecodePageVisibility = "Collapsed";
            isLogin = false;
            LoginButtonString = "登录";
        }
        public void ChoseAlarmRecodePage()
        {
            AboutPageVisibility = "Collapsed";
            HomePageVisibility = "Collapsed";
            ParameterPageVisibility = "Collapsed";
            ScanOperatePageVisibility = "Collapsed";
            BarcodeRecordVisibility = "Collapsed";
            AlarmRecodePageVisibility = "Visible";
            isLogin = false;
            LoginButtonString = "登录";
        }
        public void ChoseParameterPage()
        {
            AboutPageVisibility = "Collapsed";
            HomePageVisibility = "Collapsed";
            ParameterPageVisibility = "Visible";
            ScanOperatePageVisibility = "Collapsed";
            BarcodeRecordVisibility = "Collapsed";
            AlarmRecodePageVisibility = "Collapsed";
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
            try
            {
                if (!Directory.Exists(@"E:\images"))
                {
                    Directory.CreateDirectory(@"E:\images");
                }
                string[] filenames = Directory.GetFiles(@"E:\images");
                if (filenames.Length > 1000)
                {
                    foreach (string item in filenames)
                    {
                        File.Delete(item);
                    }
                }
            }
            catch (Exception ex)
            {

                Log.Default.Error(@"CreateDirectory E:\images", ex.Message);
            }
        }
        public void CameraHcInspect()
        {
            Msg = messagePrint.AddMessage("手动拍照");
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

            FindFill1 = (fill1.ToString() == "0");
            FindFill2 = (fill2.ToString() == "0");
            FindFill3 = (fill3.ToString() == "0");
            FindFill4 = (fill4.ToString() == "0");
            FindFill5 = (fill5.ToString() == "0");
            FindFill6 = (fill6.ToString() == "0");

            var mo1 = hdevEngine.getmeasurements("mo1");
            var mo2 = hdevEngine.getmeasurements("mo2");
            var mo3 = hdevEngine.getmeasurements("mo3");
            var mo4 = hdevEngine.getmeasurements("mo4");
            var mo5 = hdevEngine.getmeasurements("mo5");
            var mo6 = hdevEngine.getmeasurements("mo6");

            FindMo1 = mo1.ToString() == "1";
            FindMo2 = mo2.ToString() == "1";
            FindMo3 = mo3.ToString() == "1";
            FindMo4 = mo4.ToString() == "1";
            FindMo5 = mo5.ToString() == "1";
            FindMo6 = mo6.ToString() == "1";

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


            switch (p.ToString())
            {
                case "1":
                    OpenFileDialog dlg = new OpenFileDialog();
                    dlg.Filter = "视觉文件(*.hdev)|*.hdev|所有文件(*.*)|*.*";
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        HcVisionScriptFileName = dlg.FileName;
                        Inifile.INIWriteValue(iniParameterPath, "Camera", "HcVisionScriptFileName", HcVisionScriptFileName);
                    }
                    dlg.Dispose();
                    break;
                case "2":
                    FolderBrowserDialog dlgf = new FolderBrowserDialog();
                    dlgf.Description = "请选择文件路径";
                    if (dlgf.ShowDialog() == DialogResult.OK)
                    {
                        BarcodeRecordSaveFolderPath = dlgf.SelectedPath;
                        Inifile.INIWriteValue(iniParameterPath, "SavePath", "BarcodeRecordSaveFolderPath", BarcodeRecordSaveFolderPath);
                    }
                    dlgf.Dispose();
                    break;
                case "3":
                    OpenFileDialog dlg1 = new OpenFileDialog();
                    dlg1.Filter = "记录文件(*.csv)|*.csv|所有文件(*.*)|*.*";
                    if (dlg1.ShowDialog() == DialogResult.OK)
                    {
                        string csvFileName = dlg1.FileName;


                        if (csvFileName == BarcodeRecordSaveFolderPath + @"\" + "NotUpdate" + ".csv")
                        {
                            ReUpdateBar(csvFileName, false);
                            DateTimeUtility.GetLocalTime(ref lastReUpdate);
                            SaveLastSamplTimetoIni();
                            LastReUpdateStr = lastReUpdate.ToDateTime().ToString();
                        }
                        else
                        {
                            ReUpdateBar(csvFileName, true);
                        }
                    }
                    dlg1.Dispose();
                    break;
                case "4":
                    FolderBrowserDialog dlgf1 = new FolderBrowserDialog();
                    dlgf1.Description = "请选择文件路径";
                    if (dlgf1.ShowDialog() == DialogResult.OK)
                    {
                        AlarmSaveFolderPath = dlgf1.SelectedPath;
                        Inifile.INIWriteValue(iniParameterPath, "SavePath", "AlarmSaveFolderPath", AlarmSaveFolderPath);
                    }
                    dlgf1.Dispose();
                    break;
                default:
                    break;
            }
            //dlg.InitialDirectory = System.Environment.CurrentDirectory;


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
        public async void LoginAction()
        {
            List<string> r;
            if (isLogin == false)
            {
                isHideView = "Collapsed";
                r = await mydialog.showlogin();
                isHideView = "Visible";
                if (r[0] == LoginUserName && r[1] == LoginPassword)
                {
                    isLogin = !isLogin;
                }

            }
            else
            {
                isLogin = !isLogin;
            }


            if (isLogin == true)
            {
                LoginButtonString = "登出";
            }
            else
            {
                LoginButtonString = "登录";
                ChoseHomePage();
            }
        }
        public void ScanAction()
        {
            Scan.GetBarCode(ScanActionGetBarProcessCallback);
        }
        private void ScanActionGetBarProcessCallback(string bar)
        {
            string resultstr = bar == "Error" ? "失败" : bar;
            string[] strs = resultstr.Split('\r');
            Msg = messagePrint.AddMessage("扫码 " + strs[0]);
            BarcodeString = strs[0];
        }

        public async void SearchAction()
        {
            if (BarcodeString.Length > 7)
            {
                CA9SQLDATA cA9SQLDATA = new CA9SQLDATA();
                cA9SQLDATA.BLDATE = DateTime.Now.ToString();


                cA9SQLDATA.BLID = BLID.ToUpper();
                cA9SQLDATA.BLNAME = BLNAME.ToUpper();
                cA9SQLDATA.BLUID = BLUID.ToUpper();
                cA9SQLDATA.BLMID = BLMID.ToUpper();
                cA9SQLDATA.Bar = BarcodeString.ToUpper();
                using (var releaser = await m_lock.LockAsync())
                {
                    await LookforDt(cA9SQLDATA);
                }
                
            }

        }
        private void SaveCSVfileRecord(CA9SQLDATA tr, bool flag)
        {
            string filepath = "";
            if (flag)
            {
                if (!Directory.Exists(BarcodeRecordSaveFolderPath + @"\" + DateTime.Now.ToLongDateString().ToString()))
                {
                    Directory.CreateDirectory(BarcodeRecordSaveFolderPath + @"\" + DateTime.Now.ToLongDateString().ToString());
                }
                filepath = BarcodeRecordSaveFolderPath + @"\" + DateTime.Now.ToLongDateString().ToString() + @"\" + DateTime.Now.ToLongDateString().ToString() + ".csv";
            }
            else
            {
                filepath = BarcodeRecordSaveFolderPath + @"\" + "NotUpdate" + ".csv";
            }
            //filepath = BarcodeRecordSaveFolderPath + "\\" + DateTime.Now.ToLongDateString().ToString() + ".csv";
            try
            {

                if (!File.Exists(filepath))
                {
                    string[] heads = { "BLDATE", "BLID", "BLNAME", "BLUID", "BLMID", "Bar" };
                    Csvfile.savetocsv(filepath, heads);
                }
                string[] conte = { tr.BLDATE, tr.BLID, tr.BLNAME, tr.BLUID, tr.BLMID, tr.Bar };
                Csvfile.savetocsv(filepath, conte);
            }
            catch (Exception ex)
            {
                Msg = messagePrint.AddMessage("写入CSV文件失败");
                Log.Default.Error("写入CSV文件失败", ex.Message);
            }
        }
        private void SaveLastSamplTimetoIni()
        {
            try
            {
                Inifile.INIWriteValue(iniParameterPath, "ReUpdate", "wDay", lastReUpdate.wDay.ToString());
                Inifile.INIWriteValue(iniParameterPath, "ReUpdate", "wDayOfWeek", lastReUpdate.wDayOfWeek.ToString());
                Inifile.INIWriteValue(iniParameterPath, "ReUpdate", "wHour", lastReUpdate.wHour.ToString());
                Inifile.INIWriteValue(iniParameterPath, "ReUpdate", "wMilliseconds", lastReUpdate.wMilliseconds.ToString());
                Inifile.INIWriteValue(iniParameterPath, "ReUpdate", "wMinute", lastReUpdate.wMinute.ToString());
                Inifile.INIWriteValue(iniParameterPath, "ReUpdate", "wMonth", lastReUpdate.wMonth.ToString());
                Inifile.INIWriteValue(iniParameterPath, "ReUpdate", "wSecond", lastReUpdate.wSecond.ToString());
                Inifile.INIWriteValue(iniParameterPath, "ReUpdate", "wYear", lastReUpdate.wYear.ToString());

            }
            catch (Exception ex)
            {

                Log.Default.Error("SaveLastSamplTimetoIni", ex);
            }
        }
        private async void ReUpdateBar(string csvFileName, bool isdelete)
        {
            DataTable dt = new DataTable();
            DataTable dt1;
            Queue<CA9SQLDATA> FailCA9SQLDATA = new Queue<CA9SQLDATA>();
            dt.Columns.Add("BLDATE", typeof(string));
            dt.Columns.Add("BLID", typeof(string));
            dt.Columns.Add("BLNAME", typeof(string));
            dt.Columns.Add("BLUID", typeof(string));
            dt.Columns.Add("BLMID", typeof(string));
            dt.Columns.Add("Bar", typeof(string));
            Func<Task> taskFunc = () =>
            {
                return Task.Run(async () =>
                {
                    try
                    {
                        if (File.Exists(csvFileName))
                        {
                            dt1 = Csvfile.csv2dt(csvFileName, 1, dt);
                            if (dt1.Rows.Count > 0)
                            {
                                SQLReUpdateCount = 0;
                                foreach (DataRow item in dt1.Rows)
                                {
                                    CA9SQLDATA _CA9SQLDATA = new CA9SQLDATA();
                                    _CA9SQLDATA.BLDATE = item[0].ToString();
                                    _CA9SQLDATA.BLID = item[1].ToString();
                                    _CA9SQLDATA.BLNAME = item[2].ToString();
                                    _CA9SQLDATA.BLUID = item[3].ToString();
                                    _CA9SQLDATA.BLMID = item[4].ToString();
                                    _CA9SQLDATA.Bar = item[5].ToString();
                                    if (isdelete)
                                    {
                                        using (var releaser = await m_lock.LockAsync())
                                        {
                                            await LookforDt(_CA9SQLDATA);
                                        }
                                        SQLReUpdateCount++;
                                    }
                                    else
                                    {
                                        bool r;
                                        using (var releaser = await m_lock.LockAsync())
                                        {
                                            r = await LookforDt(_CA9SQLDATA);
                                        }
                                        if (r)
                                        {
                                            SaveCSVfileRecord(_CA9SQLDATA, true);
                                            SQLReUpdateCount++;
                                        }
                                        else
                                        {
                                            FailCA9SQLDATA.Enqueue(_CA9SQLDATA);
                                        }
                                    }
                                }
                                if (!isdelete)
                                {
                                    File.Delete(csvFileName);
                                    foreach (CA9SQLDATA item in FailCA9SQLDATA)
                                    {
                                        SaveCSVfileRecord(item, false);
                                    }
                                }
                                Msg = messagePrint.AddMessage("重传记录完成");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Default.Error("重传记录", ex.Message);
                    }
                });
            };
            await taskFunc();

        }
        private int UpdateAlarmFromExcel(string filename, AlarmTuple[] alarmTupleArray)
        {
            int itemsCount = 0;
            FileInfo existingFile = new FileInfo(filename);
            using (ExcelPackage package = new ExcelPackage(existingFile))
            {
                // get the first worksheet in the workbook
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];

                int rowCount = worksheet.Dimension.End.Row;
                int colCount = worksheet.Dimension.End.Column;
                for (int i = 1; i <= rowCount; i++)
                {
                    if (worksheet.Cells[i, 1] != null && worksheet.Cells[i, 1].Value != null && worksheet.Cells[i, 2] != null && worksheet.Cells[i, 2].Value != null)
                    {
                        alarmTupleArray[itemsCount].CoilName = worksheet.Cells[i, 1].Value.ToString();
                        alarmTupleArray[itemsCount].AlarmContent = worksheet.Cells[i, 2].Value.ToString();
                        alarmTupleArray[itemsCount].CoilStatus = false;
                        alarmTupleArray[itemsCount].LastCoilStatus = false;
                        itemsCount++;
                    }

                }

            }

            return itemsCount;
        }
        private void SaveCSVfileAlarm(AlarmTableItem item)
        {
            if (DateTime.Now.Hour < 8)
            {
                if (AlarmLastDateNameStr != DateTime.Now.AddDays(-1).ToLongDateString())
                {
                    AlarmLastDateNameStr = DateTime.Now.AddDays(-1).ToLongDateString();
                    Inifile.INIWriteValue(iniParameterPath, "Alarm", "AlarmLastDateNameStr", AlarmLastDateNameStr);
                }
            }
            else
            {
                if (AlarmLastDateNameStr != DateTime.Now.ToLongDateString())
                {
                    AlarmLastDateNameStr = DateTime.Now.ToLongDateString();
                    Inifile.INIWriteValue(iniParameterPath, "Alarm", "AlarmLastDateNameStr", AlarmLastDateNameStr);
                }
            }
            string Bancistr = DateTime.Now.Hour >= 8 && DateTime.Now.Hour < 20 ? "白班" : "夜班";
            string filepath = AlarmSaveFolderPath + "\\Alarm" + AlarmLastDateNameStr + Bancistr + ".csv";
            if (!Directory.Exists(AlarmSaveFolderPath))
            {
                Directory.CreateDirectory(AlarmSaveFolderPath);
            }
            try
            {
                if (!File.Exists(filepath))
                {
                    string[] heads = { "AlarmDate", "MachineID", "UserID", "AlarmMessage" };
                    Csvfile.savetocsv(filepath, heads);
                }
                string[] conte = { item.AlarmDate, item.MachineID, item.UserID, item.AlarmMessage };
                Csvfile.savetocsv(filepath, conte);
            }
            catch (Exception ex)
            {
                Msg = messagePrint.AddMessage("SaveCSVfileAlarm.写入CSV文件失败");
                Log.Default.Error("SaveCSVfileAlarm.写入CSV文件失败", ex.Message);
            }
        }
        #region 数据库操作
        private void setLocalTime(string strDateTime)
        {
            DateTimeUtility.SYSTEMTIME st = new DateTimeUtility.SYSTEMTIME();
            DateTime dt = Convert.ToDateTime(strDateTime);
            st.FromDateTime(dt);
            DateTimeUtility.SetLocalTime(ref st);
        }
        private void ConnectDBTest()
        {
            try
            {
                OraDB oraDB = new OraDB(SQL_ora_server, SQL_ora_user, SQL_ora_pwd);
                if (oraDB.isConnect())
                {
                    string dbtime = oraDB.sfc_getServerDateTime();
                    setLocalTime(dbtime);
                    Msg = messagePrint.AddMessage("获取数据库时间： " + dbtime);

                    IsTCPConnect = true;
                }
                else
                {
                    Msg = messagePrint.AddMessage("数据库未连接");

                    IsTCPConnect = false;
                }
                oraDB.disconnect();
            }
            catch (Exception ex)
            {
                Msg = messagePrint.AddMessage("获取数据库时间失败");
                IsTCPConnect = false;
            }
        }
        private async Task<bool> LookforDt(CA9SQLDATA cA9SQLDATA)
        {
            return await((Func<Task<bool>>)(() =>
            {
                return Task.Run(() =>
                {
                    try
                    {
                        OraDB oraDB = new OraDB(SQL_ora_server, SQL_ora_user, SQL_ora_pwd);
                        string tablename = "sfcdata.barautbind";
                        if (oraDB.isConnect())
                        {
                            IsTCPConnect = true;
                            arrField[0] = "SCBARCODE";
                            arrValue[0] = cA9SQLDATA.Bar;

                            DataSet s = oraDB.selectSQL(tablename.ToUpper(), arrField, arrValue);
                            SinglDt = s.Tables[0];
                            if (SinglDt.Rows.Count == 0)
                            {
                                Msg = messagePrint.AddMessage("未查询到 " + cA9SQLDATA.Bar + " 信息");
                                oraDB.disconnect();
                                return false;
                            }
                            else
                            {
                                string panelbar = (string)SinglDt.Rows[0]["SCPNLBAR"];
                                string[,] arrFieldAndNewValue = { { "BLDATE", ("to_date('" + cA9SQLDATA.BLDATE + "', 'yyyy/mm/dd hh24:mi:ss')").ToUpper() }, { "BLID", cA9SQLDATA.BLID }, { "BLNAME", cA9SQLDATA.BLNAME }, { "BLUID", cA9SQLDATA.BLUID }, { "BLMID", cA9SQLDATA.BLMID } };

                                string[,] arrFieldAndOldValue = { { "SCPNLBAR", panelbar } };
                                oraDB.updateSQL2(tablename.ToUpper(), arrFieldAndNewValue, arrFieldAndOldValue);
                                Msg = messagePrint.AddMessage("数据更新完成");
                                arrField[0] = "SCPNLBAR";
                                arrValue[0] = panelbar.ToUpper();
                                DataSet s1 = oraDB.selectSQL(tablename.ToUpper(), arrField, arrValue);
                                PanelDt = s1.Tables[0];
                                oraDB.disconnect();
                                return true;
                            }

                        }
                        else
                        {
                            IsTCPConnect = false;
                            Msg = messagePrint.AddMessage("数据库连接失败");
                            oraDB.disconnect();
                            return false;
                        }

                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine(ex.Message);
                        Msg = messagePrint.AddMessage("查询数据库出错");
                        return false;

                    }
                });
            }))();

        }
        private async Task UploadtoDt()
        {
            string UpdateTime = DateTime.Now.ToString("yyyyMMdd") + DateTime.Now.ToString("HHmmss");
            string SCDATE = DateTime.Now.ToString("yyyyMMdd");
            string SCTIME = DateTime.Now.ToString("HHmmss");
            string hostName = Dns.GetHostName();
            string ipstring = "NULL";
            System.Net.IPAddress[] addressList = Dns.GetHostAddresses(hostName);//会返回所有地址，包括IPv4和IPv6 
            foreach (IPAddress item in addressList)
            {
                ipstring = item.ToString();
                string[] ss = ipstring.Split(new string[] { "." }, StringSplitOptions.None);
                if (ss.Length == 4 && ss[0] == "10")
                {
                    break;
                }
            } 

            await ((Func<Task>)(() =>
            {
                return Task.Run(() =>
                {
                    try
                    {
                        OraDB oraDB = new OraDB(SQL_ora_server, SQL_ora_user, SQL_ora_pwd);
                        string tablename = "sfcdata.barautbind";
                        if (oraDB.isConnect())
                        {
                            IsTCPConnect = true;
                            arrField[0] = "BB01";
                            arrValue[0] = BLMID.ToUpper();

                            DataSet s = oraDB.selectSQL(tablename.ToUpper(), arrField, arrValue);
                            SinglDt = s.Tables[0];
                            if (SinglDt.Rows.Count == 0)
                            {
                                string[] arrFieldAndNewValue = { "BB01", "BB02", "BB03", "BB04", "BB05", "BB06", "BB07", "SCDATE", "SCTIME" };
                                string[] arrFieldAndOldValue = { BLMID.ToUpper(), BLUID.ToUpper(), YieldCount.ToString(), AlarmCount.ToString(), AlmPer.ToString(), UpdateTime, ipstring, SCDATE, SCTIME };
                                oraDB.insertSQL(tablename.ToUpper(), arrFieldAndNewValue, arrFieldAndOldValue);
                                Msg = messagePrint.AddMessage("UploadtoDt数据插入完成");
                                oraDB.disconnect();
                                return false;
                            }
                            else
                            {
                                string[,] arrFieldAndNewValue = { { "BB02", BLUID.ToUpper() }, { "BB03", YieldCount.ToString() }, { "BB04", AlarmCount.ToString() }, { "BB05", AlmPer.ToString() }, { "BB06", UpdateTime }, { "BB07", ipstring }, { "SCDATE", SCDATE }, { "SCTIME", SCTIME } };

                                string[,] arrFieldAndOldValue = { { "BB01", BLMID.ToUpper() } };
                                oraDB.updateSQL(tablename.ToUpper(), arrFieldAndNewValue, arrFieldAndOldValue);
                                Msg = messagePrint.AddMessage("UploadtoDt数据更新完成");
                                oraDB.disconnect();
                                return true;
                            }

                        }
                        else
                        {
                            IsTCPConnect = false;
                            Msg = messagePrint.AddMessage("UploadtoDt数据库连接失败");
                            oraDB.disconnect();
                            return false;
                        }

                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine(ex.Message);
                        Msg = messagePrint.AddMessage("UploadtoDt查询数据库出错");
                        return false;

                    }
                });
            }))();
        }
        #endregion
        #endregion
        #region 初始化
        [Initialize(InitType.Initialize)]
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
            string filepath1 = BarcodeRecordSaveFolderPath + @"\" + DateTime.Now.ToLongDateString().ToString() + @"\" + DateTime.Now.ToLongDateString().ToString() + ".csv";
            //string filepath2 = BarcodeRecordSaveFolderPath + @"\" + "NotUpdate" + ".csv";
            DataTable dt = new DataTable();
            DataTable dt1;
            dt.Columns.Add("BLDATE", typeof(string));
            dt.Columns.Add("BLID", typeof(string));
            dt.Columns.Add("BLNAME", typeof(string));
            dt.Columns.Add("BLUID", typeof(string));
            dt.Columns.Add("BLMID", typeof(string));
            dt.Columns.Add("Bar", typeof(string));
            try
            {
                if (File.Exists(filepath1))
                {
                    dt1 = Csvfile.csv2dt(filepath1, 1, dt);
                    if (dt1.Rows.Count > 0)
                    {
                        foreach (DataRow item in dt1.Rows)
                        {
                            CA9SQLDATA _CA9SQLDATA = new CA9SQLDATA();
                            _CA9SQLDATA.BLDATE = item[0].ToString();
                            _CA9SQLDATA.BLID = item[1].ToString();
                            _CA9SQLDATA.BLNAME = item[2].ToString();
                            _CA9SQLDATA.BLUID = item[3].ToString();
                            _CA9SQLDATA.BLMID = item[4].ToString();
                            _CA9SQLDATA.Bar = item[5].ToString();
                            lock (this)
                            {
                                _BarcodeRecord.Enqueue(_CA9SQLDATA);
                            }
                        }
                        Msg = messagePrint.AddMessage("读取记录完成");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Default.Error("WindowLoadedcsv2dt", ex.Message);
            }
            cameraHcInit();
            await Task.Delay(100);
            CameraHcInspect();
            Msg = messagePrint.AddMessage("检测相机初始化完成");
            td = new TaiDaPLC(PortName, 19200, System.IO.Ports.Parity.Even, 7, System.IO.Ports.StopBits.One);
            td.ReConnectUp += ReConnectUpEventHandle;
            await Task.Delay(100);
            ConnectDBTest();
        }
        #endregion
        #region 读写操作
        private bool ReadParameter()
        {
            try
            {
                HcVisionScriptFileName = Inifile.INIGetStringValue(iniParameterPath, "Camera", "HcVisionScriptFileName", @"C:\test.hdev");
                PortName = Inifile.INIGetStringValue(iniParameterPath, "SerialPort", "Com", "COM1");
                ScanPortName = Inifile.INIGetStringValue(iniParameterPath, "SerialPort", "ScanCom", "COM1");
                ModbusState = Inifile.INIGetStringValue(iniParameterPath, "SerialPort", "ModbusState", "01");
                BLID = Inifile.INIGetStringValue(iniParameterPath, "SQLMSG", "BLID", "Null");
                BLUID = Inifile.INIGetStringValue(iniParameterPath, "SQLMSG", "BLUID", "Null");
                BLMID = Inifile.INIGetStringValue(iniParameterPath, "SQLMSG", "BLMID", "Null");
                BLNAME = Inifile.INIGetStringValue(iniParameterPath, "SQLMSG", "BLNAME", "Null");
                SQL_ora_server = Inifile.INIGetStringValue(iniParameterPath, "Oracle", "Server", "mesdb07");
                SQL_ora_user = Inifile.INIGetStringValue(iniParameterPath, "Oracle", "User", "sfcabar");
                SQL_ora_pwd = Inifile.INIGetStringValue(iniParameterPath, "Oracle", "Passwold", "sfcabar*168");
                BarcodeRecordSaveFolderPath = Inifile.INIGetStringValue(iniParameterPath, "SavePath", "BarcodeRecordSaveFolderPath", "C:\\");
                AlarmSaveFolderPath = Inifile.INIGetStringValue(iniParameterPath, "SavePath", "AlarmSaveFolderPath", "C:\\");
                lastReUpdate.wDay = ushort.Parse(Inifile.INIGetStringValue(iniParameterPath, "ReUpdate", "wDay", "13"));
                lastReUpdate.wDayOfWeek = ushort.Parse(Inifile.INIGetStringValue(iniParameterPath, "ReUpdate", "wDayOfWeek", "0"));
                lastReUpdate.wHour = ushort.Parse(Inifile.INIGetStringValue(iniParameterPath, "ReUpdate", "wHour", "17"));
                lastReUpdate.wMilliseconds = ushort.Parse(Inifile.INIGetStringValue(iniParameterPath, "ReUpdate", "wMilliseconds", "273"));
                lastReUpdate.wMinute = ushort.Parse(Inifile.INIGetStringValue(iniParameterPath, "ReUpdate", "wMinute", "5"));
                lastReUpdate.wMonth = ushort.Parse(Inifile.INIGetStringValue(iniParameterPath, "ReUpdate", "wMonth", "11"));
                lastReUpdate.wSecond = ushort.Parse(Inifile.INIGetStringValue(iniParameterPath, "ReUpdate", "wSecond", "55"));
                lastReUpdate.wYear = ushort.Parse(Inifile.INIGetStringValue(iniParameterPath, "ReUpdate", "wYear", "2016"));
                LastReUpdateStr = lastReUpdate.ToDateTime().ToString();
                YieldCount = int.Parse(Inifile.INIGetStringValue(iniParameterPath, "Alarm", "YieldCount", "0"));
                AlarmCount = int.Parse(Inifile.INIGetStringValue(iniParameterPath, "Alarm", "AlarmCount", "0"));
                AlmPer = double.Parse(Inifile.INIGetStringValue(iniParameterPath, "Alarm", "AlmPer", "0"));
                AlarmLastDateNameStr = Inifile.INIGetStringValue(iniParameterPath, "Alarm", "AlarmLastDateNameStr", "0");
                AlarmLastClearHourofYear = int.Parse(Inifile.INIGetStringValue(iniParameterPath, "Alarm", "AlarmLastClearHourofYear", "0"));
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
                Inifile.INIWriteValue(iniParameterPath, "SerialPort", "ScanCom", ScanPortName);
                Inifile.INIWriteValue(iniParameterPath, "Oracle", "Server", SQL_ora_server);
                Inifile.INIWriteValue(iniParameterPath, "Oracle", "User", SQL_ora_user);
                Inifile.INIWriteValue(iniParameterPath, "Oracle", "Passwold", SQL_ora_pwd);
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
        public void RunPLC()
        {
            AlarmTuple[] AlarmTupleArray = new AlarmTuple[200];

            int alramItemsCount = 0;

            string alarmconfigfile = System.Environment.CurrentDirectory + "\\CA9报警.xlsx";


            try
            {
                alramItemsCount = UpdateAlarmFromExcel(alarmconfigfile, AlarmTupleArray);

                Msg = messagePrint.AddMessage(alramItemsCount.ToString() + " Alarm Messages have Configed...");



            }
            catch (Exception ex)
            {
                Log.Default.Error("alarmconfigfile fail", ex.Message);
                Msg = messagePrint.AddMessage("Alarm Messages Config Error !!!");
            }

            while (true)
            {
                //await Task.Delay(100);
                System.Threading.Thread.Sleep(100);
                try
                {
                    if (td == null) continue;
                    if (!td.State)
                    {
                        td.Closed();
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
                            //视觉
                            if (td.ReadM(ModbusState, StartAction))
                            {
                                td.SetM(ModbusState, StartAction, false);
                                Msg = messagePrint.AddMessage(StartAction + " ," + "0");
                                td.SetM(ModbusState, EndAction, false);
                                Msg = messagePrint.AddMessage(EndAction + " ," + "0");
                                try
                                {
                                    //Inspect();
                                    Msg = messagePrint.AddMessage("PLC触发拍照");
                                    Async.RunFuncAsync(cameraHcInspect, PLCTakePhoteCallback);
                                }
                                catch
                                {
                                    td.SetM(ModbusState, Position1, false);
                                    td.SetM(ModbusState, Position2, false);
                                    td.SetM(ModbusState, Position3, false);
                                    td.SetM(ModbusState, Position4, false);
                                    td.SetM(ModbusState, Position5, false);
                                    td.SetM(ModbusState, Position6, false);
                                    td.SetM(ModbusState, Position7, false);
                                    td.SetM(ModbusState, Position8, false);
                                    td.SetM(ModbusState, Position9, false);
                                    td.SetM(ModbusState, Position10, false);
                                    td.SetM(ModbusState, Position11, false);
                                    td.SetM(ModbusState, Position12, false);
                                    Msg = messagePrint.AddMessage("视觉脚本异常");
                                }

                            }
                            //扫码
                            if (td.ReadM(ModbusState, "M195"))
                            {
                                td.SetM(ModbusState, "M196", false);
                                td.SetM(ModbusState, "M197", false);
                                td.SetM(ModbusState, "M195", false);
                                Scan.GetBarCode(PLCGetBarProcessCallback);
                            }
                            //报警
                            for (int i = 0; i < alramItemsCount; i++)
                            {
                                AlarmTupleArray[i].CoilStatus = td.ReadM(ModbusState, AlarmTupleArray[i].CoilName);
                                if (AlarmTupleArray[i].LastCoilStatus != AlarmTupleArray[i].CoilStatus)
                                {
                                    AlarmTupleArray[i].LastCoilStatus = AlarmTupleArray[i].CoilStatus;
                                    if (AlarmTupleArray[i].CoilStatus)
                                    {
                                        AlarmTableItem _alarmTableItem = new AlarmTableItem();
                                        _alarmTableItem.AlarmDate = DateTime.Now.ToString();
                                        _alarmTableItem.AlarmMessage = AlarmTupleArray[i].AlarmContent;
                                        _alarmTableItem.MachineID = BLMID;
                                        _alarmTableItem.UserID = BLUID;
                                        AlarmCount++;
                                        if (YieldCount <= 0)
                                        {
                                            AlmPer = 0;
                                        }
                                        else
                                        {
                                            AlmPer = Math.Round((double)AlarmCount/ YieldCount, 2);
                                        }
                                        Inifile.INIWriteValue(iniParameterPath, "Alarm", "AlarmCount", AlarmCount.ToString());
                                        Inifile.INIWriteValue(iniParameterPath, "Alarm", "AlmPer", AlmPer.ToString());
                                        SaveCSVfileAlarm(_alarmTableItem);
                                        lock (this)
                                        {
                                            AlarmTableItemQueue.Enqueue(_alarmTableItem);
                                        }
                                        //记录报警
                                    }
                                }
                            }

                        }
                    }


                }
                catch (Exception ex)
                {
                    td.State = false;

                }
                IsPLCConnect = td.State;

            }
        }
        private async void PLCTakePhoteCallback()
        {

            if (FindMo1)
            {
                td.SetM(ModbusState, Position1, true);
                Msg = messagePrint.AddMessage(Position1 + " ," + "1");
            }
            else
            {
                td.SetM(ModbusState, Position1, false);
                Msg = messagePrint.AddMessage(Position1 + " ," + "0");
            }

            if (FindMo2)
            {
                td.SetM(ModbusState, Position2, true);
                Msg = messagePrint.AddMessage(Position2 + " ," + "1");
            }
            else
            {
                td.SetM(ModbusState, Position2, false);
                Msg = messagePrint.AddMessage(Position2 + " ," + "0");
            }

            if (FindMo3)
            {
                td.SetM(ModbusState, Position3, true);
                Msg = messagePrint.AddMessage(Position3 + " ," + "1");
            }
            else
            {
                td.SetM(ModbusState, Position3, false);
                Msg = messagePrint.AddMessage(Position3 + " ," + "0");
            }

            if (FindMo4)
            {
                td.SetM(ModbusState, Position4, true);
                Msg = messagePrint.AddMessage(Position4 + " ," + "1");
            }
            else
            {
                td.SetM(ModbusState, Position4, false);
                Msg = messagePrint.AddMessage(Position4 + " ," + "0");
            }

            if (FindMo5)
            {
                td.SetM(ModbusState, Position5, true);
                Msg = messagePrint.AddMessage(Position5 + " ," + "1");
            }
            else
            {
                td.SetM(ModbusState, Position5, false);
                Msg = messagePrint.AddMessage(Position5 + " ," + "0");
            }

            if (FindMo6)
            {
                td.SetM(ModbusState, Position6, true);
                Msg = messagePrint.AddMessage(Position6 + " ," + "1");
            }
            else
            {
                td.SetM(ModbusState, Position6, false);
                Msg = messagePrint.AddMessage(Position6 + " ," + "0");
            }

            if (FindFill1)
            {
                td.SetM(ModbusState, Position7, true);
                Msg = messagePrint.AddMessage(Position7 + " ," + "1");
            }
            else
            {
                td.SetM(ModbusState, Position7, false);
                Msg = messagePrint.AddMessage(Position7 + " ," + "0");
            }

            if (FindFill2)
            {
                td.SetM(ModbusState, Position8, true);
                Msg = messagePrint.AddMessage(Position8 + " ," + "1");
            }
            else
            {
                td.SetM(ModbusState, Position8, false);
                Msg = messagePrint.AddMessage(Position8 + " ," + "0");
            }

            if (FindFill3)
            {
                td.SetM(ModbusState, Position9, true);
                Msg = messagePrint.AddMessage(Position9 + " ," + "1");
            }
            else
            {
                td.SetM(ModbusState, Position9, false);
                Msg = messagePrint.AddMessage(Position9 + " ," + "0");
            }

            if (FindFill4)
            {
                td.SetM(ModbusState, Position10, true);
                Msg = messagePrint.AddMessage(Position10 + " ," + "1");
            }
            else
            {
                td.SetM(ModbusState, Position10, false);
                Msg = messagePrint.AddMessage(Position10 + " ," + "0");
            }

            if (FindFill5)
            {
                td.SetM(ModbusState, Position11, true);
                Msg = messagePrint.AddMessage(Position11 + " ," + "1");
            }
            else
            {
                td.SetM(ModbusState, Position11, false);
                Msg = messagePrint.AddMessage(Position11 + " ," + "0");
            }

            if (FindFill6)
            {
                td.SetM(ModbusState, Position12, true);
                Msg = messagePrint.AddMessage(Position12 + " ," + "1");
            }
            else
            {
                td.SetM(ModbusState, Position12, false);
                Msg = messagePrint.AddMessage(Position12 + " ," + "0");
            }

            await Task.Delay(1);
            td.SetM(ModbusState, EndAction, true);
            Msg = messagePrint.AddMessage(EndAction + " ," + "1");
            YieldCount++;
            Inifile.INIWriteValue(iniParameterPath, "Alarm", "YieldCount", YieldCount.ToString());

        }
        private async void PLCGetBarProcessCallback(string bar)
        {
            string resultstr = bar == "Error" ? "失败" : bar;
            string[] strs = resultstr.Split('\r');
            Msg = messagePrint.AddMessage("扫码 " + strs[0]);
            BarcodeString = strs[0];
            td.SetM(ModbusState, "M195", false);
            if (bar == "Error")
            {
                td.SetM(ModbusState, "M197", true);
            }
            else
            {
                td.SetM(ModbusState, "M196", true);
                CA9SQLDATA cA9SQLDATA = new CA9SQLDATA();
                cA9SQLDATA.BLDATE = DateTime.Now.ToString();
                cA9SQLDATA.BLID = BLID.ToUpper();
                cA9SQLDATA.BLNAME = BLNAME.ToUpper();
                cA9SQLDATA.BLUID = BLUID.ToUpper();
                cA9SQLDATA.BLMID = BLMID.ToUpper();
                cA9SQLDATA.Bar = BarcodeString.ToUpper();

                lock (this)
                {
                    _BarcodeRecord.Enqueue(cA9SQLDATA);
                }
                if (BarcodeString.Length > 7)
                {
                    try
                    {
                        bool r;
                        using (var releaser = await m_lock.LockAsync())
                        {
                            r = await LookforDt(cA9SQLDATA);
                        }
                        if (r)
                        {
                            SaveCSVfileRecord(cA9SQLDATA, true);
                        }
                        else
                        {
                            SaveCSVfileRecord(cA9SQLDATA, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        SaveCSVfileRecord(cA9SQLDATA, false);
                        Log.Default.Error("LookforDt_PLC", ex.Message);
                    }
                }
            }
        }
        #endregion
    }
    public class CA9SQLDATA
    {
        public string BLDATE { get; set; }   //折线作业时间
        public string BLID { get; set; }     //折线治具编号
        public string BLNAME { get; set; }  //折线治具名称
        public string BLUID { get; set; }  //折线人员
        public string BLMID { get; set; }   //折线机台编号
        public string Bar { get; set; }    //单pcs条码
    }
    public struct AlarmTuple
    {
        public string CoilName;
        public bool CoilStatus;
        public bool LastCoilStatus;
        public string AlarmContent;
    }
    public class AlarmTableItem
    {
        public string AlarmDate { set; get; }
        public string MachineID { set; get; }
        public string UserID { set; get; }
        public string AlarmMessage { set; get; }
    }
    //public class MachineStatus
    //{
    //    public string MachineID { set; get; }
    //    public string UserID { set; get; }
    //    public int YieldCount { set; get; }
    //    public int AlarmCount { set; get; }
    //    public double AlmPer { set; get; }
    //    public string UpdateTime { set; get; }
    //    public string RemoteIP { set; get; }
    //}
}