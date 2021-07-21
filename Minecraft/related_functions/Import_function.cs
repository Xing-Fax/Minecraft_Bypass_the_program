using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft.related_functions
{
    class Import_function
    {


        /// <summary>
        /// 执行注册表导入
        /// </summary>
        /// <param name="regPath">注册表文件路径</param>
        public static void ExecuteReg(string regPath)
        {
            if (File.Exists(regPath))
            {
                regPath = @"""" + regPath + @"""";
                Process.Start("regedit", string.Format(" /s {0}", regPath));
            }
        }
    }
}
