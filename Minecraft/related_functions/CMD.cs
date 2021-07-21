using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft.related_functions
{
    class CMD
    {
        /// <summary>
        /// 执行cmd命令，返回执行结果
        /// </summary>
        /// <param name="command">命令内容</param>
        /// <returns></returns>
        public static string RunCmd(string command)
        {
            //例Process
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";         //确定程序名
            p.StartInfo.Arguments = "/c " + command;   //确定程式命令行
            p.StartInfo.UseShellExecute = false;      //Shell的使用
            p.StartInfo.RedirectStandardInput = true;  //重定向输入
            p.StartInfo.RedirectStandardOutput = true; //重定向输出
            p.StartInfo.RedirectStandardError = true;  //重定向输出错误
            p.StartInfo.CreateNoWindow = true;        //设置置不显示示窗口
            p.Start();
            return p.StandardOutput.ReadToEnd();      //输出出流取得命令行结果果
        }
    }
}
