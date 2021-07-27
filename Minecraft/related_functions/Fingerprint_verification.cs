using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft.related_functions
{
    class Fingerprint_verification
    {
        /// <summary>
        /// 检测自己身签名指纹是否匹配
        /// 有效防止软件被病毒或恶意软件篡改
        /// </summary>
        /// <returns>返回true为正常状态</returns>
        public static bool Document_verification()
        {
            try
            {
                X509Certificate cert = X509Certificate.CreateFromSignedFile(Process.GetCurrentProcess().MainModule.FileName);
                string Fingerprint = cert.GetCertHashString();
                if (Fingerprint == "36A888B9F2A505BF92AC6B2796C2188E639AB1D1")
                { return true; }
                else
                { return false; }
            }
            catch { return false; }
        }
    }
}
