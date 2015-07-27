using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Comdat.DOZP.Core
{
    [DataContract(IsReference = true)]
    public partial class ScanFile
    {
        public ScanFile()
        {
            this.Operations = new List<Operation>();
        }

        [DataMember]
        public int ScanFileID { get; set; }

        [DataMember]
        public int BookID { get; set; }

        [DataMember]
        public PartOfBook PartOfBook { get; set; }

        [DataMember]
        public string FileName { get; set; }

        public string OcrFileName
        {
            get
            {
                if (!String.IsNullOrEmpty(FileName))
                {
                    int length = FileName.LastIndexOf('.');
                    string name = ((length == -1) ? FileName : FileName.Substring(0, length));

                    return String.Format("{0}.{1}", name, FileFormat.Pdf).ToLower();
                }
                else
                {
                    return null;
                }
            }
        }

        public string TxtFileName
        {
            get
            {
                if (!String.IsNullOrEmpty(FileName))
                {
                    int length = FileName.LastIndexOf('.');
                    string name = ((length == -1) ? FileName : FileName.Substring(0, length));

                    return String.Format("{0}.{1}", name, FileFormat.Txt).ToLower();
                }
                else
                {
                    return null;
                }
            }
        }

        [DataMember]
        public short PageCount { get; set; }

        [DataMember]
        public bool UseOCR { get; set; }

        [DataMember]
        public string OcrText { get; set; }

        [DataMember]
        public Nullable<short> OcrTime { get; set; }

        [DataMember]
        public DateTime Created { get; set; }

        [DataMember]
        public DateTime Modified { get; set; }

        [DataMember]
        public string Comment { get; set; }

        [DataMember]
        public StatusCode Status { get; set; }

        [DataMember]
        public virtual Book Book { get; set; }

        public virtual ICollection<Operation> Operations { get; set; }

        [DataMember]
        public bool ImageChanged { get; set; }

        [DataMember]
        public bool ObalkyKnihUrl { get; set; }

        /// <summary>
        /// Vráti celou cestu na serveri  k naskenovanému souboru
        /// </summary>
        /// <returns></returns>
        public string GetScanFilePath()
        {
            if (String.IsNullOrEmpty(FileName)) throw new ArgumentNullException("FileName");
            if (Book == null) throw new ArgumentNullException("Book");

            return Path.Combine(Book.GetDirectoryPath(), FileName);
        }

        /// <summary>
        /// Vráti celou cestu na serveri k zpracovanému OCR souboru
        /// </summary>
        /// <returns></returns>
        public string GetOcrFilePath()
        {
            if (String.IsNullOrEmpty(OcrFileName)) throw new ArgumentNullException("OcrFileName");
            if (Book == null) throw new ArgumentNullException("Book");

            return Path.Combine(Book.GetDirectoryPath(), OcrFileName);
        }
    }
}
