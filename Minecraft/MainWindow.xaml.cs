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

namespace Minecraft
{

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //拖动窗体
        private void Grid_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) { DragMove(); }
        }

        /*Microsoft会在检测许可证失败后的30分钟内重新检查
        Microsoft检测到RuntimeBroker.exe进程消失后的20秒内重新启动该进程
        所以为了防止第二次检查，每隔0.5秒扫描并结束一次"RuntimeBroker.exe"
        此操作可能会造成Microsoft无响应，在一次结束"RuntimeBroker.exe"时Microsoft会恢复响应*/
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
                Dispatcher.Invoke(new Action(delegate { 关闭_Click(null, null); }));
        }

        /*Microsoft启动时会使用RuntimeBroker.exe进程来通过ClipSVC服务来检测应用许可证状态
        此时ClipSVC的运行状态已经被破坏掉，ClipSVC将无法重新启动
        由于ClipSVC无法正常启动会造成RuntimeBroker一直处于等待状态，造成程序假死，游戏加载会卡到64%
        只要将与Microsoft关联的RuntimeBroker.exe进程结束掉即可恢复正常状态，稍后RuntimeBroker会重新启动*/
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
                    Dispatcher.Invoke(new Action(delegate
                    {
                        日志.Text += "[" + DateTime.Now.ToLongTimeString().ToString() + "]: PID = " + related_functions.Intercept.Substring(Return_result[i], "\",\"", "\",\"") + "\n";
                        日志.Text += "[" + DateTime.Now.ToLongTimeString().ToString() + "]: 启动完成！！！\n";
                        fyIcon.BalloonTipText = "启动完毕！";
                        fyIcon.ShowBalloonTip(0);
                        t1.Dispose();
                        t1.Close();
                        BeginStoryboard((Storyboard)FindResource("窗体关闭"));
                    }));
                    t2.Start();
                    beforDT = DateTime.Now;
                    Start_state = true;
                }
            }
        }

        /// <summary>
        /// 初始化Timer类，用于扫描进程PID并结束，单位间隔：30秒
        /// </summary>
        System.Timers.Timer t1 = new System.Timers.Timer(30000);
        /// <summary>
        /// 初始化Timer类，用于定时扫描进程"RuntimeBroker.exe"并结束，单位间隔：1秒
        /// </summary>
        System.Timers.Timer t2 = new System.Timers.Timer(1000);
        /// <summary>
        /// Windows10原生Toast通知
        /// </summary>
        NotifyIcon fyIcon = new NotifyIcon();
        /// <summary>
        /// 用于计算游戏时长
        /// </summary>
        DateTime beforDT;
        /// <summary>
        /// 决定游戏是否启动过
        /// </summary>
        bool Start_state = false;


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
            //设置一个名称为进程名的互斥体
            Mutex mutex = new Mutex(true,Process.GetCurrentProcess().ProcessName, out isAppRunning);
            if (!isAppRunning)
            {
                fyIcon.BalloonTipText = "已经有一个实例程序正在运行\n双击托盘图标以打开程序...";
                fyIcon.ShowBalloonTip(0);
                fyIcon.Dispose();
                Environment.Exit(0);
            }

            //以下代码用作校验程序签名是否完整，调试时请将其注释

            //if (related_functions.Fingerprint_verification.Document_verification() != true)
            //{
            //    fyIcon.BalloonTipText = "程序签名指纹校验失败！已关闭此程序";
            //    fyIcon.ShowBalloonTip(0);
            //    fyIcon.Dispose();
            //    Environment.Exit(0);
            //}

            //到处结束

            日志.Text = "[" + DateTime.Now.ToLongTimeString().ToString() + "]: 程序启动...\n";

            t1.Elapsed += new System.Timers.ElapsedEventHandler(Looking_for_process) ;//到达时间的时候执行事件
            t1.AutoReset = false;//设置是执行一次（false）还是一直执行(true)
            t1.Enabled = false;//是否执行System.Timers.Timer.Elapsed事件

            t2.Elapsed += new System.Timers.ElapsedEventHandler(Looking_for_RuntimeBroker);//到达时间的时候执行事件
            t2.AutoReset = true;//设置是执行一次（false）还是一直执行(true)
            t2.Enabled = false;//是否执行System.Timers.Timer.Elapsed事件
        }

        /*将内嵌入注册表文件释放到用户临时文件夹后执行导入
        此注册表是用来破坏ClipSVC服务的正常运行，此时ClipSVC服务将无法重新启动
        删除释放的文件后，提示用户启动游戏
        并启动计时器，来后台监测与Microsoft关联的RuntimeBroker进程*/
        private void 启动_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //检测系统中是否安装MinecraftUWP
                string UWP = related_functions.CMD.RunCmd(@"CD C:\Program Files\WindowsApps & C: & DIR");//列出已经安装的UWP程序
                if (UWP.Contains("Microsoft.MinecraftUWP"))//判断是否包含Win10我的世界
                {
                    日志.Text += "[" + DateTime.Now.ToLongTimeString().ToString() + "]: 读取嵌入文件字节组\n";
                    byte[] byDll = Encoding.Default.GetBytes(Properties.Resources.MC_ON);//获取嵌入文件的字节数组  
                    日志.Text += "[" + DateTime.Now.ToLongTimeString().ToString() + "]: 开始释放内嵌注册表文件\n";
                    string strPath = Environment.GetEnvironmentVariable("TMP") + @"\MC_ON.reg";//设置释放路径
                    日志.Text += "[" + DateTime.Now.ToLongTimeString().ToString() + "]: 写入文件流...\n";
                    using (FileStream fs = new FileStream(strPath, FileMode.Create))//开始写入文件流
                    {
                        fs.Write(byDll, 0, byDll.Length);
                    }
                    日志.Text += "[" + DateTime.Now.ToLongTimeString().ToString() + "]: 写入注册表...\n";
                    //导入注册表
                    related_functions.Import_function.ExecuteReg(Environment.GetEnvironmentVariable("TMP") + @"\MC_ON.reg");

                    日志.Text += "[" + DateTime.Now.ToLongTimeString().ToString() + "]: 禁用 AppXSvc 服务...\n";
                    //禁用AppXSvc服务
                    related_functions.CMD.RunCmd("net stop AppXSvc");

                    日志.Text += "[" + DateTime.Now.ToLongTimeString().ToString() + "]: 禁用 ClipSVC 服务...\n";
                    //禁用ClipSVC服务
                    related_functions.CMD.RunCmd("net stop ClipSVC");

                    日志.Text += "[" + DateTime.Now.ToLongTimeString().ToString() + "]: 删除临时文件...\n";
                    //删除临时文件
                    File.Delete(Environment.GetEnvironmentVariable("TMP") + @"\MC_ON.reg");
                    //启动游戏
                    日志.Text += "[" + DateTime.Now.ToLongTimeString().ToString() + "]: 调起 MinecraftUWP\n";
                    Process.Start("minecraft:");
                    t1.Start();
                }
                else
                {
                    日志.Text += "[" + DateTime.Now.ToLongTimeString().ToString() + "]: 请先安装 MinecraftUWP\n";
                    fyIcon.BalloonTipText = "您似乎还没有安装 MinecraftUWP";
                    fyIcon.ShowBalloonTip(0);
                }
            }
            catch (Exception ex)
            {
                fyIcon.BalloonTipText = "程序启动时遇到异常!";
                fyIcon.ShowBalloonTip(0);
                System.Windows.MessageBox.Show(ex.ToString());
                //删除通知栏图标
                fyIcon.Dispose();
                //关闭进程
                Environment.Exit(0);
            }
        }

        //导入原注册表并恢复ClipSVC服务的正常运行
        private void 关闭_Click(object sender, RoutedEventArgs e)
        {
            BeginStoryboard((Storyboard)FindResource("关闭程序"));
            fyIcon.BalloonTipText = "请稍后...\n开始还原注册表和服务项...";
            fyIcon.ShowBalloonTip(0);
            try
            {
                byte[] byDll = Encoding.Default.GetBytes(Properties.Resources.MC_OFF);//获取嵌入文件的字节数组  
                string strPath = Environment.GetEnvironmentVariable("TMP") + @"\MC_OFF.reg";//设置释放路径
                using (FileStream fs = new FileStream(strPath, FileMode.Create))//开始写入文件流
                {
                    fs.Write(byDll, 0, byDll.Length);
                }
                //恢复注册表
                related_functions.Import_function.ExecuteReg(Environment.GetEnvironmentVariable("TMP") + @"\MC_OFF.reg");
                Thread.Sleep(6000);//睡眠6秒
                //恢复AppXSvc服务
                related_functions.CMD.RunCmd("net start AppXSvc");
                //恢复ClipSVC服务
                related_functions.CMD.RunCmd("net start ClipSVC");
                //删除临时文件
                File.Delete(Environment.GetEnvironmentVariable("TMP") + @"\MC_OFF.reg");
                //计算游戏时长
                if (Start_state == true)//是否启动了游戏
                {
                    DateTime afterDT = DateTime.Now;
                    TimeSpan ts = afterDT.Subtract(beforDT);
                    fyIcon.BalloonTipText = "还原完毕！已成功关闭程序！\n本次游戏时长：" + related_functions.Time_Calculate.formatLongToTimeStr(ts.TotalMilliseconds);
                }
                else//没有启动游戏，仅启动了程序
                {
                    fyIcon.BalloonTipText = "还原完毕！已成功关闭程序！";
                }

                fyIcon.ShowBalloonTip(0);
                //删除通知栏图标
                fyIcon.Dispose();
                //关闭进程
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                fyIcon.BalloonTipText = "程序关闭时遇到异常!" ;
                fyIcon.ShowBalloonTip(0);
                System.Windows.MessageBox.Show(ex.ToString());
                //删除通知栏图标
                fyIcon.Dispose();
                //关闭进程
                Environment.Exit(0);
            }
        }

        private void 最小化_Click(object sender, RoutedEventArgs e)
        {
            BeginStoryboard((Storyboard)FindResource("窗体关闭"));
        }

        private void Label_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/xingchuanzhen/Minecraft_Bypass_the_program");
        }
    }
}
