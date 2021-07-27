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


namespace Minecraft
{

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //拖动窗体
        private void Rectangle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) { DragMove(); }
        }

        private void Looking_for_process(object source, System.Timers.ElapsedEventArgs e)
        {

            //显示 Microsoft Store 应用及其关联的进程，按照CSV格式输出，并逐行写入到数组内
            string[] Return_result = related_functions.CMD.RunCmd("tasklist /APPS /FO CSV").Split('\n');//获得返回信息

            string result = "无结果";//存储有效结果

            for (int i = 0; i < Return_result.Length; i++)//遍历全部结果，找到与Microsoft相关联的RuntimeBroker进程
            {
                if (related_functions.Intercept.Substring(Return_result[i], "RuntimeBroker.exe", "Microsoft.MinecraftUWP") != "")
                {   //不等于空，说明符合条件，获取其字符串
                    result = Return_result[i];
                }
            }

            if (result != "无结果")//在截取到结果后
            {
                t1.Stop();//暂停计时器
                Thread.Sleep(10000);//检测到进程后延时10秒...

                //"RuntimeBroker.exe (runtimebroker07f4358a809ac99a64a67c1)","11680","21,608 K","Microsoft.MinecraftUWP_1.17.1004.0_x64__8wekyb3d8bbwe"
                //获得数据后，从其中分离出PID，11680即为RuntimeBroker.exe的PID

                //通过截取字符串的方法从数据中获得PID
                string PID = related_functions.Intercept.Substring(result, "\",\"", "\",\"");
                //使用taskkill命令结束获取到的PID进程
                related_functions.CMD.RunCmd("taskkill /pid " + PID + " /f");
                //同步线程更新状态
                Dispatcher.Invoke(new Action(delegate
                {   
                    日志.Text += "[" + DateTime.Now.ToLongTimeString().ToString() + "]: PID = " + related_functions.Intercept.Substring(result, "\",\"", "\",\"") + "\n";
                    日志.Text += "[" + DateTime.Now.ToLongTimeString().ToString() + "]: 启动完成！！！\n";
                    t1.Close();//释放计时器
                }));
            }

        }
        /// <summary>
        /// 初始化Timer类，用于扫描进程PID，单位间隔：10000毫秒
        /// </summary>
        System.Timers.Timer t1 = new System.Timers.Timer(10000);

        public MainWindow()
        {
            InitializeComponent();

            //以下代码用作校验程序签名是否完整，调试时请将其注释

            //if (related_functions.Fingerprint_verification.Document_verification() != true)
            //{
            //    MessageBox.Show("签名校验失败,程序可能被篡改,轻击确定以退出程序","警告");
            //    Environment.Exit(0);
            //}

            //到处结束


            日志.Text = "[" + DateTime.Now.ToLongTimeString().ToString() + "]: 程序启动...\n";

            t1.Elapsed += new System.Timers.ElapsedEventHandler(Looking_for_process) ;//到达时间的时候执行事件
            t1.AutoReset = true;//设置是执行一次（false）还是一直执行(true)
            t1.Enabled = false;//是否执行System.Timers.Timer.Elapsed事件

        }

        private void 启动_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                日志.Text += "[" + DateTime.Now.ToLongTimeString().ToString() + "]: 读取嵌入文件字节组\n";
                byte[] byDll = Encoding.Default.GetBytes(Properties.Resources.MC_ON);//获取嵌入文件的字节数组  
                日志.Text += "[" + DateTime.Now.ToLongTimeString().ToString() + "]: 开始缩放内嵌注册表文件" ;
                string strPath = Environment.GetEnvironmentVariable("TMP") + @"\MC_ON.reg";//设置释放路径

                日志.Text += "[" + DateTime.Now.ToLongTimeString().ToString() + "]: 写入文件流...\n";
                using (FileStream fs = new FileStream(strPath, FileMode.Create))//开始写入文件流
                {
                    fs.Write(byDll, 0, byDll.Length);
                }

                日志.Text += "[" + DateTime.Now.ToLongTimeString().ToString() + "]: 写入注册表...\n";
                //导入注册表
                related_functions.Import_function.ExecuteReg(Environment.GetEnvironmentVariable("TMP") + @"\MC_ON.reg");

                日志.Text += "[" + DateTime.Now.ToLongTimeString().ToString() + "]: 禁用 ClipSVC 服务...\n";
                //禁用ClipSVC服务
                related_functions.CMD.RunCmd("net stop ClipSVC");

                日志.Text += "[" + DateTime.Now.ToLongTimeString().ToString() + "]: 删除临时文件...\n";
                //删除临时文件
                File.Delete(Environment.GetEnvironmentVariable("TMP") + @"\MC_ON.reg");

                //提示用户启动游戏
                日志.Text += "[" + DateTime.Now.ToLongTimeString().ToString() + "]: 您现在启动您的游戏...\n";
                //启动结束...
                t1.Start();
            }
            catch(Exception ex)
            {
                日志.Text = ex.ToString();
            }

        }

        private void 关闭_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                主窗体.Visibility = Visibility.Collapsed;//隐藏主窗体
                byte[] byDll = Encoding.Default.GetBytes(Properties.Resources.MC_OFF);//获取嵌入文件的字节数组  
                string strPath = Environment.GetEnvironmentVariable("TMP") + @"\MC_OFF.reg";//设置释放路径
                using (FileStream fs = new FileStream(strPath, FileMode.Create))//开始写入文件流
                {
                    fs.Write(byDll, 0, byDll.Length);
                }
                //恢复注册表
                related_functions.Import_function.ExecuteReg(Environment.GetEnvironmentVariable("TMP") + @"\MC_OFF.reg");
                //恢复ClipSVC服务
                related_functions.CMD.RunCmd("net start ClipSVC");
                //删除临时文件
                File.Delete(Environment.GetEnvironmentVariable("TMP") + @"\MC_OFF.reg");
                //关闭进程
                Environment.Exit(0);
            }
            catch(Exception ex)
            {
                日志.Text = ex.ToString();
            }
        }

        private void 最小化_Click(object sender, RoutedEventArgs e)
        {
            主窗体.WindowState = WindowState.Minimized;
        }

        private void Label_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/xingchuanzhen/Minecraft_Bypass_the_program");
        }
    }
}
