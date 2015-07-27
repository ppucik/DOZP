using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Comdat.DOZP.Core
{
    public class App
    {
        public const string URL = @"http://ffas04.ff.cuni.cz/";
        public const string NAMESPACE = URL + @"/2015/01";
        public const string REPOSITORY_DIR= @"C:\DOZP";
        public const string FTP_DIR = @"C:\FTP";

        private static ApplicationConfiguration _configuration;
        public static string TITLE = "";

        static App()
        {
            /// Load the properties from the Config file
            Configuration = new ApplicationConfiguration();
            TITLE = Configuration.ApplicationTitle;
        }

        /// <summary>
        /// Configuration object
        /// </summary>
        public static ApplicationConfiguration Configuration
        {
            get { return _configuration; }
            set { _configuration = value; }
        }
    }

    public class ApplicationConfiguration
    {
        private string _applicationTitle = "Knihovna FF UK - OBSAHY";
        private string _companyName = "Comdat s.r.o.";
        private string _ownerName = "Knihovna FF UK v Praze";
        private string _cookieName = "ComdatDOZP";

        public ApplicationConfiguration()
        {
            //ReadKeysFromConfig();
        }

        public string ApplicationTitle
        {
            get { return _applicationTitle; }
            set { _applicationTitle = value; }
        }

        public string CompanyName
        {
            get { return _companyName; }
            set { _companyName = value; }
        }

        public string OwnerName
        {
            get { return _ownerName; }
            set { _ownerName = value; }
        }

        public string CookieName
        {
            get { return _cookieName; }
            set { _cookieName = value; }
        }
    }
}
