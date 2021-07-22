using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft.related_functions
{
    class Intercept
    {
        /// <summary>
        /// 截取字符串
        /// </summary>
        /// <param name="sourse">目标</param>
        /// <param name="startstr">从这里</param>
        /// <param name="endstr">到这里</param>
        /// <returns>返回不包含"从这里","到这里"的字符串</returns>
        public static string Substring(string sourse, string startstr, string endstr)
        {
            string result = string.Empty;
            int startindex, endindex;
            try
            {
                startindex = sourse.IndexOf(startstr);
                if (startindex == -1)
                {
                    return result;
                }
                string tmpstr = sourse.Substring(startindex + startstr.Length);
                endindex = tmpstr.IndexOf(endstr);
                if (endindex == -1)
                {
                    return result;
                }
                result = tmpstr.Remove(endindex);
            }
            catch { }
            return result;
        }
    }
}
