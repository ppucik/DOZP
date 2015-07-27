using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;

namespace Comdat.DOZP.App
{
    public class ScannerManager
    {
        public static void Test(string sourceName)
        {
            string query = String.Format("SELECT * FROM Win32_Printer WHERE Name LIKE '%{0}'", sourceName);
            query = "SELECT * FROM Win32_USBControllerDevice";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection coll = searcher.Get();
            
            foreach (ManagementObject printer in coll)
            {


                Debug.WriteLine(String.Format("{0}", printer.ToString()));
            }
        }
    }
}
