using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Comdat.DOZP.Core
{
    public class OcrFile
    {
        #region Private variables
        private string _ocrFolderPath = null;
        private string _ocrFileName = null;
        private string _ocrText = null;
        #endregion

        #region Constructors

        public OcrFile(string ocrFolderPath)
        {
            this.OcrFolderPath = ocrFolderPath;
        }

        public OcrFile(string ocrFolderPath, string ocrFileName)
            : this(ocrFolderPath)
        {
            this.OcrFileName = ocrFileName;
        }

        #endregion

        #region Public propeties

        public string OcrFolderPath
        {
            get
            {
                return _ocrFolderPath;
            }
            private set
            {
                if (String.IsNullOrEmpty(value))
                    throw new ArgumentNullException("OcrFolderPath");

                _ocrFolderPath = value;
            }
        }

        public string OcrFileName
        {
            get
            {
                return _ocrFileName;
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                    throw new ArgumentNullException("OcrFileName");

                _ocrFileName = value;
            }
        }

        public string OcrFilePath
        {
            get
            {
                if (!String.IsNullOrEmpty(OcrFolderPath) && !String.IsNullOrEmpty(OcrFileName))
                    return Path.Combine(OcrFolderPath, OcrFileName);
                else
                    return null;
            }
        }

        public string TxtFilePath
        {
            get
            {
                if (!String.IsNullOrEmpty(OcrFileName))
                {
                    string fileName = GetFileNameWithoutExtension(OcrFileName);
                    return Path.Combine(OcrFolderPath, String.Format("{0}.txt", fileName));
                }
                else
                {
                    return null;
                }
            }
        }

        public string BakFilePath
        {
            get
            {
                if (!String.IsNullOrEmpty(OcrFileName))
                {
                    string fileName = GetFileNameWithoutExtension(OcrFileName);
                    return Path.Combine(OcrFolderPath, String.Format("{0}.bak", fileName));
                }
                else
                {
                    return null;
                }
            }
        }

        public string OcrText
        {
            get
            {
                return _ocrText;
            }
            set
            {
                _ocrText = value;
            }
        }

        public bool IsFormated
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(OcrText)) 
                    return (OcrText.Contains("--") && !OcrText.Contains(Environment.NewLine));
                else
                    return true;
            }
        }

        #endregion

        #region Public methods

        public string Format(string ocrText)
        {
            if (String.IsNullOrWhiteSpace(ocrText)) return null;

            List<string> result = new List<string>();
            string[] lines = Regex.Split(ocrText, @"[\r\n]+");

            for (int i = 0; i < lines.Length; i++)
            {
                string line = Regex.Replace(lines[i], @"[\t\s]+", " ");
                line = Regex.Replace(line, @"^([\s\d\.,]+)|([_,\.\-\s\d]+)$", "");

                line = Regex.Replace(line, @"[.,_]+", " ");
                line = Regex.Replace(line, @"\s+", " ");

                if (!String.IsNullOrWhiteSpace(line))
                    result.Add(line);
            }

            this.OcrText = String.Join(" -- ", result);

            //return String.Join(Environment.NewLine, result);
            return this.OcrText;
        }

        public void Load(string ocrFileName)
        {
            if (String.IsNullOrEmpty(ocrFileName)) throw new ArgumentNullException("ocrFileName");

            this.OcrFileName = ocrFileName;
            //this.OcrText = File.ReadAllText(TxtFilePath);
        }

        public void BackupText()
        {
            if (File.Exists(TxtFilePath) && !File.Exists(BakFilePath))
            {
                File.Copy(TxtFilePath, BakFilePath, true);
            }
        }

        public bool FileExists(FileFormat format)
        {
            switch (format)
            {
                case FileFormat.Pdf:
                    return File.Exists(OcrFilePath);
                case FileFormat.Txt:
                    return File.Exists(TxtFilePath);
                case FileFormat.Bak:
                    return File.Exists(BakFilePath);
                default:
                    return false;
            }
        }

        public void SaveText(string filePath)
        {
            if (String.IsNullOrEmpty(filePath)) throw new ArgumentNullException("filePath");
            if (String.IsNullOrEmpty(this.OcrText)) throw new ArgumentException("Neexistuje zpracovaný OCR text.");

            File.WriteAllText(filePath, this.OcrText);
        }

        public void Delete()
        {
            if (String.IsNullOrEmpty(OcrFolderPath)) return;
            if (Directory.Exists(OcrFolderPath) == false) return;

            ImageFunctions.DeleteFiles(OcrFolderPath);
        }

        private string GetFileNameWithoutExtension(string fileName)
        {
            if (!String.IsNullOrEmpty(fileName))
            {
                int length = OcrFileName.LastIndexOf('.');
                return ((length == -1) ? OcrFileName : OcrFileName.Substring(0, length));
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}
