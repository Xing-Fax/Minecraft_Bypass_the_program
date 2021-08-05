using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft.related_functions
{
    class Associated_process_scan
    {
        /// <summary>
        /// 获取与MinecraftUWP关联的RuntimeBroker进程
        /// </summary>
        /// <returns>返回数据列表，如果指定进程不存在则返回字符串"0"</returns>
        public static string process_scan()
        {
            //显示 Microsoft Store 应用及其关联的进程，按照CSV格式输出，并逐行写入到数组内
            string[] Return_result = CMD.RunCmd("tasklist /APPS /FO CSV").Split('\n');//获得返回信息
            for (int i = 0; i < Return_result.Length; i++)//遍历全部结果，找到与Microsoft相关联的RuntimeBroker进程
            {
                if (Intercept.Substring(Return_result[i], "RuntimeBroker.exe", "Microsoft.MinecraftUWP") != "")
                {   //不等于空，说明符合条件，获取其字符串
                    return Return_result[i];
                }
            }
            return "0";
        }
    }
}
