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

        private void theout1(object source, System.Timers.ElapsedEventArgs e)
        {
            string[] str = related_functions.CMD.RunCmd("tasklist /APPS").Split('\n');//获得返回信息

            string str2 = "";//存储有效结果

            for (int i = 0; i < str.Length; i++)
            {
                if (related_functions.Intercept.Substring(str[i], "RuntimeBroker.exe", "Microsoft.MinecraftUWP") != "")
                {
                    str2 = str[i];
                }
            }

            if (str2 != "")//在有结果的情况下
            {
                t1.Stop();//暂停计时器
                Thread.Sleep(10000);//检测到进程后延时10秒...
                //执行taskkill命令结束进程
                related_functions.CMD.RunCmd("taskkill /pid " + related_functions.Intercept.Substring(str2, "   ", "   ").Trim() + " /f");
                
                Dispatcher.Invoke(new Action(delegate
                {//同步线程更新状态
                    日志.Text += "[" + DateTime.Now.ToString() + "]: 启动完成！！！\n\n\n";
                    t1.Close();//释放计时器
                }));
            }
        }

        System.Timers.Timer t1 = new System.Timers.Timer(10000);
        public MainWindow()
        {
            InitializeComponent();

            日志.Text = "[" + DateTime.Now.ToString() + "]: 程序启动...\n";

            t1.Elapsed += new System.Timers.ElapsedEventHandler(theout1);//到达时间的时候执行事件
            t1.AutoReset = true;//设置是执行一次（false）还是一直执行(true)
            t1.Enabled = false;//是否执行System.Timers.Timer.Elapsed事件

        }

        private void 启动_Click(object sender, RoutedEventArgs e)
        {
            日志.Text += "[" + DateTime.Now.ToString() + "]: 读取嵌入文件字节组\n";
            byte[] byDll = Encoding.Default.GetBytes(Properties.Resources.MC_ON);//获取嵌入文件的字节数组  
            日志.Text += "[" + DateTime.Now.ToString() + "]: 设置释放文件路径：" + Environment.GetEnvironmentVariable("TMP") + @"\MC_ON.reg" + "\n";
            string strPath = Environment.GetEnvironmentVariable("TMP") + @"\MC_ON.reg";//设置释放路径

            日志.Text += "[" + DateTime.Now.ToString() + "]: 写入文件流...\n";
            using (FileStream fs = new FileStream(strPath, FileMode.Create))//开始写入文件流
            {
                fs.Write(byDll, 0, byDll.Length);
            }

            日志.Text += "[" + DateTime.Now.ToString() + "]: 写入注册表...\n";
            //导入注册表
            related_functions.Import_function.ExecuteReg(Environment.GetEnvironmentVariable("TMP") + @"\MC_ON.reg");

            日志.Text += "[" + DateTime.Now.ToString() + "]: 禁用ClipSVC服务...\n";
            //禁用ClipSVC服务
            related_functions.CMD.RunCmd("net stop ClipSVC");

            日志.Text += "[" + DateTime.Now.ToString() + "]: 删除临时文件...\n";
            //删除临时文件
            File.Delete(Environment.GetEnvironmentVariable("TMP") + @"\MC_ON.reg");

            日志.Text += "[" + DateTime.Now.ToString() + "]: 您现在启动您的游戏...\n";
            //启动结束...
            t1.Start();
        }

        private void 关闭_Click(object sender, RoutedEventArgs e)
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

        private void 最小化_Click(object sender, RoutedEventArgs e)
        {
            日志.Text += "[" + DateTime.Now.ToString() + "]: 最小化程序...\n";
            主窗体.WindowState = WindowState.Minimized;
        }
    }
}
