using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Media.Animation;
using System.ComponentModel;

namespace Minecraft
{

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Timers.Timer t1 = new System.Timers.Timer(30000);
        System.Timers.Timer t2 = new System.Timers.Timer(1000);
        NotifyIcon fyIcon = new NotifyIcon();
        DateTime beforDT;
        bool Start_state = false;

        private void Grid_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) { DragMove(); }
        }

        private void 最小化_Click(object sender, RoutedEventArgs e)
        {
            BeginStoryboard((Storyboard)FindResource("窗体关闭"));
        }

        private void Label_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/xingchuanzhen/Minecraft_Bypass_the_program");
        }

        private void Notify_NotifyIcon(string str, bool Icon)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                fyIcon.BalloonTipText = str;
                fyIcon.ShowBalloonTip(0);
                if (Icon == true)
                {
                    fyIcon.Dispose();
                    Environment.Exit(0);
                }
            }));
        }

        private void Log_Write(string str)
        {
            Dispatcher.Invoke(new Action(delegate{ 日志.Text += "[" + DateTime.Now.ToLongTimeString().ToString() + "]: " + str + "\n"; }));
        }

        private void Looking_for_RuntimeBroker(object source, System.Timers.ElapsedEventArgs e)
        {
            string Return = related_functions.CMD.RunCmd("tasklist /APPS /FO CSV");
            string[] Return_result = Return.Split('\n');
            for (int i = 0; i < Return_result.Length; i++)
            {
                if (related_functions.Intercept.Substring(Return_result[i], "RuntimeBroker.exe", "Microsoft.MinecraftUWP") != "")
                {   
                    string PID = related_functions.Intercept.Substring(Return_result[i], "\",\"", "\",\"");
                    related_functions.CMD.RunCmd("taskkill /pid " + PID + " /f");
                }
            }
            if (!Return.Contains ("Microsoft.MinecraftUWP"))
            {
                Dispatcher.Invoke(new Action(delegate { t2.Stop(); }));
                Dispatcher.Invoke(new Action(delegate { 关闭_Click(null, null); }));
            }
        }

        private void Looking_for_process(object source, System.Timers.ElapsedEventArgs e)
        {
            string Return = related_functions.CMD.RunCmd("tasklist /APPS /FO CSV");
            string[] Return_result = Return.Split('\n');
            for (int i = 0; i < Return_result.Length; i++)
            {
                if (related_functions.Intercept.Substring(Return_result[i], "RuntimeBroker.exe", "Microsoft.MinecraftUWP") != "")
                {  
                    string PID = related_functions.Intercept.Substring(Return_result[i], "\",\"", "\",\"");
                    related_functions.CMD.RunCmd("taskkill /pid " + PID + " /f");

                    Log_Write("PID = " + related_functions.Intercept.Substring(Return_result[i], "\",\"", "\",\""));
                    Log_Write("启动完成！！！");
                    Notify_NotifyIcon("启动完毕！", false);
                    Dispatcher.Invoke(new Action(delegate { BeginStoryboard((Storyboard)FindResource("窗体关闭")); }));
                    t1.Dispose();
                    Dispatcher.Invoke(new Action(delegate { t2.Start(); }));
                    beforDT = DateTime.Now;
                    Start_state = true;
                }
            }
        }

        private void OnNotifyIconDoubleClick(object sender, EventArgs e)
        {
            if (主窗体.Visibility ==  Visibility.Collapsed )
            { BeginStoryboard((Storyboard)FindResource("窗体打开")); }
            else
            { BeginStoryboard((Storyboard)FindResource("窗体关闭")); }
        }

        public MainWindow()
        {
            InitializeComponent();

            fyIcon.Icon = Properties.Resources.图标;
            fyIcon.Visible = true;
            fyIcon.Text = "正常运行";
            fyIcon.BalloonTipTitle = "温馨提示";
            fyIcon.DoubleClick += OnNotifyIconDoubleClick;

            bool isAppRunning = false;
            Mutex mutex = new Mutex(true,Process.GetCurrentProcess().ProcessName, out isAppRunning);
            if (!isAppRunning)
                Notify_NotifyIcon("已经有一个实例程序正在运行\n双击托盘图标以打开程序...", true);

            if (related_functions.Fingerprint_verification.Document_verification() != true)
                Notify_NotifyIcon("程序签名指纹校验失败！已关闭此程序!", true);

            Log_Write("程序启动...");
            t1.Elapsed += new System.Timers.ElapsedEventHandler(Looking_for_process) ;
            t1.AutoReset = false;

            t2.Elapsed += new System.Timers.ElapsedEventHandler(Looking_for_RuntimeBroker);
            t2.AutoReset = true;
        }

        void Start_in_the_background(object sender, DoWorkEventArgs e)
        {
            Log_Write("读取嵌入文件字节组");
            byte[] byDll = Encoding.Default.GetBytes(Properties.Resources.MC_ON);
            Log_Write("开始释放内嵌注册表文件");
            string strPath = Environment.GetEnvironmentVariable("TMP") + @"\MC_ON.reg";
            Log_Write("写入文件流...");
            using (FileStream fs = new FileStream(strPath, FileMode.Create))
            {
                fs.Write(byDll, 0, byDll.Length);
            }
            Log_Write("写入注册表...");
            related_functions.Import_function.ExecuteReg(Environment.GetEnvironmentVariable("TMP") + @"\MC_ON.reg");
            Log_Write("禁用 AppXSvc 服务...");
            related_functions.CMD.RunCmd("net stop AppXSvc");
            Log_Write("禁用 ClipSVC 服务...");
            related_functions.CMD.RunCmd("net stop ClipSVC");
            Log_Write("删除临时文件...");
            File.Delete(Environment.GetEnvironmentVariable("TMP") + @"\MC_ON.reg");
            Log_Write("调起 MinecraftUWP");
            Process.Start("minecraft:");
            t1.Start();
        }

        private void 启动_Click(object sender, RoutedEventArgs e)
        {
            string UWP = related_functions.CMD.RunCmd(@"CD C:\Program Files\WindowsApps & C: & DIR");
            if (UWP.Contains("Microsoft.MinecraftUWP"))
            {
                using (BackgroundWorker bw = new BackgroundWorker())
                {
                    bw.DoWork += new DoWorkEventHandler(Start_in_the_background);
                    bw.RunWorkerAsync();
                }
            }
            else
            {
                Log_Write("请先安装 MinecraftUWP!");
                Notify_NotifyIcon("您似乎还没有安装 MinecraftUWP!", false);
            }
        }

        void Closure_in_the_background(object sender, DoWorkEventArgs e)
        {
            byte[] byDll = Encoding.Default.GetBytes(Properties.Resources.MC_OFF);
            string strPath = Environment.GetEnvironmentVariable("TMP") + @"\MC_OFF.reg";
            using (FileStream fs = new FileStream(strPath, FileMode.Create))
            {
                fs.Write(byDll, 0, byDll.Length);
            }

            related_functions.Import_function.ExecuteReg(Environment.GetEnvironmentVariable("TMP") + @"\MC_OFF.reg");
            Thread.Sleep(6000);
            related_functions.CMD.RunCmd("net start AppXSvc");
            related_functions.CMD.RunCmd("net start ClipSVC");
            File.Delete(Environment.GetEnvironmentVariable("TMP") + @"\MC_OFF.reg");
            if (Start_state == true)
            {
                DateTime afterDT = DateTime.Now;
                TimeSpan ts = afterDT.Subtract(beforDT);
                Notify_NotifyIcon("还原完毕！已成功关闭程序！\n本次游戏时长：" + related_functions.Time_Calculate.formatLongToTimeStr(ts.TotalMilliseconds), true);
            }
            else { Notify_NotifyIcon("还原完毕！已成功关闭程序！", true); }
        }

        private void 关闭_Click(object sender, RoutedEventArgs e)
        {
            BeginStoryboard((Storyboard)FindResource("关闭程序"));
            Notify_NotifyIcon("请稍后...\n开始还原注册表和服务项...", false);
            using (BackgroundWorker bw = new BackgroundWorker())
            {
                bw.DoWork += new DoWorkEventHandler(Closure_in_the_background);
                bw.RunWorkerAsync();
            }
        }
    }
}
