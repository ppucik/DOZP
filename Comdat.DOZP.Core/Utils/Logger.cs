using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Comdat.DOZP.Core
{
    public sealed class Logger
    {
        public const string LOG_PATH = @"C:\DOZP";
        private static readonly Logger _instance = new Logger();
        private static StreamWriter _logFile = null;
        private static string _fileName = null;

        static Logger()
        {
        }

        Logger()
        {
        }

        ~Logger()
        {
            Close();
        }

        public static Logger Instance
        {
            get
            {
                return _instance;
            }
        }

        public static string FileName
        {
            get
            {
                return _fileName; 
            }
        }

        public static void Open()
        {
            try
            {
                Open(Path.Combine(LOG_PATH, String.Format("AppLog{0:yyyyMMdd}.txt", DateTime.Now)));
            }
            catch
            {
            }
        }

        public static void Open(string fileName)
        {
            try
            {
                _fileName = fileName;
                string path = Path.GetDirectoryName(fileName);

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                _logFile = new StreamWriter(fileName, true);
            }
            catch
            {
            }
        }

        public static void Log(string message)
        {
            try
            {
                if (_logFile != null)
                {
                    MethodBase methodInfo = new StackFrame(1).GetMethod();
                    string classAndMethod = String.Format("{0}.{1}", methodInfo.DeclaringType.Name, methodInfo.Name);
                    string memorySize = Process.GetCurrentProcess().PrivateMemorySize64.ToFileSize();
                    if (!String.IsNullOrEmpty(message)) message = message.Replace(Environment.NewLine, "; ");

                    _logFile.WriteLine(String.Format("[{0:dd/MM/yyyy HH:mm:ss}] {1} | {2} | {3}", DateTime.Now, classAndMethod, message, memorySize));
                    _logFile.Flush();
                }
            }
            catch
            {
            }
        }

        public static void Close()
        {
            try
            {
                if (_logFile != null)
                {
                    _logFile.Flush();
                    _logFile.Close();
                    _logFile = null;
                }
            }
            catch
            {
            }
        }
    }
}
