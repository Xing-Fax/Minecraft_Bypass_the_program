using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft.related_functions
{
    class Time_Calculate
    {
        /// <summary>
        /// 将毫秒转换为  时分秒格式
        /// </summary>
        /// <param name="l"></param>
        /// <returns>返回转换后的字符串</returns>
        public static string formatLongToTimeStr(double l)
        {
            int hour = 0;
            int minute = 0;
            int second = 0;
            second = (int)(l / 1000);
            
            if (second > 60)
            {
                minute = second / 60;
                second = second % 60;
            }
            if (minute > 60)
            {
                hour = minute / 60;
                minute = minute % 60;
            }
            return (hour.ToString() + "小时 " + minute.ToString() + "分钟 " + second.ToString() + "秒");
        }
    }
}
