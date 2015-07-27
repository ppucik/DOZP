using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Comdat.DOZP.Core
{
    public class DesktopShortcut
    {
        public static void CreateIcon()
        {
            ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;

            if (ad.IsFirstRun)
            {
                Assembly assembly = Assembly.GetEntryAssembly();
                string company = String.Empty;
                string description = String.Empty;

                if (Attribute.IsDefined(assembly, typeof(AssemblyCompanyAttribute)))
                {
                    AssemblyCompanyAttribute ascompany = (AssemblyCompanyAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyCompanyAttribute));
                    company = ascompany.Company;
                }

                if (Attribute.IsDefined(assembly, typeof(AssemblyDescriptionAttribute)))
                {
                    AssemblyDescriptionAttribute asdescription = (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyDescriptionAttribute));
                    description = asdescription.Description;
                }

                if (!string.IsNullOrEmpty(company))
                {
                    string desktopPath = String.Empty;
                    desktopPath = String.Concat(
                                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                                    "\\",
                                    description,
                                    ".appref-ms");

                    string shortcutName = String.Empty;
                    shortcutName = String.Concat(
                                     Environment.GetFolderPath(Environment.SpecialFolder.Programs),
                                     "\\",
                                     company,
                                     "\\",
                                     description,
                                     ".appref-ms");

                    File.Copy(shortcutName, desktopPath, true);
                }
            }
        }
    }
}
