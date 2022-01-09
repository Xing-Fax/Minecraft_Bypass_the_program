using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Minecraft
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static string[] com_line_args;
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            com_line_args = e.Args;
        }
    }
}
