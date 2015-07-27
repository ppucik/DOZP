using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;

namespace Comdat.DOZP.Core
{
    [Bindable(true)]
    public class ScanImages : ObservableCollection<ScanImage>
    {
        #region Private variables
        private static int _counter = 1;
        private string _scanFolderPath = null;
        private string _scanFileName = null;
        private int? _coverDpi = 300;
        private bool _coverBW = false;
        private int? _contentsDpi = 300;
        private bool _contentsBW = true;
        #endregion

        #region Constructors

        public ScanImages(string scanFolderPath)
            : base()
        {
            this.ScanFolderPath = scanFolderPath;
        }

        //public ScanImages(string scanFolderPath, string scanFileName)
        //    : this(scanFolderPath)
        //{
        //    this.ScanFileName = scanFileName;
        //}

        #endregion

        #region Public propeties

        public string ScanFolderPath
        {
            get
            {
                return _scanFolderPath;
            }
            private set
            {
                if (String.IsNullOrEmpty(value))
                    throw new ArgumentNullException("ScanFolderPath");
                if (!Directory.Exists(value))
                    Directory.CreateDirectory(value);

                _scanFolderPath = value;
            }
        }

        public string ScanFileName
        {
            get
            {
                return _scanFileName;
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                    throw new ArgumentNullException("ScanFileName");

                _scanFileName = value;
            }
        }

        public bool HasCover
        {
            get
            {
                return (this.Count(i => i.IsCover == true) == 1);
            }
        }

        public bool HasCoverNoUrl
        {
            get
            {
                return (this.Count(i => i.IsCover == true && !i.IsExternalUrl) == 1);
            }
        }

        public string CoverFullName
        {
            get
            {
                if (HasCover)
                    return Path.Combine(ScanFolderPath, String.Format("{0}.jpg", ScanFileName));
                else
                    return null;
            }
        }

        public bool CoverBW
        {
            get
            {
                return _coverBW;
            }
            set
            {
                _coverBW = value;
            }
        }

        public int? CoverDpi
        {
            get
            {
                return _coverDpi;
            }
            set
            {
                _coverDpi = value;
            }
        }

        public bool HasContents
        {
            get
            {
                return (ContetsPages > 0);
            }
        }

        public int ContetsPages
        {
            get
            {
                return (this.Count(i => i.IsCover == false));
            }
        }

        public string ContentsFullName
        {
            get
            {
                if (ContetsPages > 0)
                    return Path.Combine(ScanFolderPath, String.Format("{0}.tif", ScanFileName));
                else
                    return null;
            }
        }

        public bool ContentsBW
        {
            get
            {
                return _contentsBW;
            }
            set
            {
                _contentsBW = value;
            }
        }

        public int? ContentsDpi
        {
            get
            {
                return _contentsDpi;
            }
            set
            {
                _contentsDpi = value;
            }
        }

        public bool IsNextEvenPage(bool isCover)
        {
            return (!isCover && (ContetsPages + 1) % 2 == 0);
        }

        #endregion

        #region Public methods

        public ScanImage GetNewScanImage(bool isCover = false)
        {
            if (String.IsNullOrEmpty(ScanFileName)) throw new ArgumentException("ScanFileName");

            string fileName = String.Empty;

            if (isCover)
                fileName = String.Format("{0}.jpg", ScanFileName);
            else
                fileName = String.Format("{0}#{1:00}.tif", ScanFileName, _counter++);

            string fullName = Path.Combine(ScanFolderPath, fileName);
            ScanImage image = new ScanImage(fullName, isCover);

            if (isCover)
            {
                image.BlackAndWhite = this.CoverBW;
                image.Dpi = this.CoverDpi;
            }
            else
            {
                image.BlackAndWhite = this.ContentsBW;
                image.Dpi = this.ContentsDpi;
            }

            return image;
        }

        public bool CanMoveUp(ScanImage image)
        {
            if (image == null || image.IsCover || Count == 0)
                return false;
            else
                return (this.IndexOf(image) > (HasCover ? 1 : 0));
        }

        public bool CanMoveDown(ScanImage image)
        {
            if (image == null || image.IsCover || Count == 0)
                return false;
            else
                return (this.IndexOf(image) < (Count - 1));
        }

        new public void Add(ScanImage image)
        {
            if (image == null) throw new ArgumentNullException("image");
            if (image.IsCover && HasCover) throw new ArgumentException("Přidat je možné pouze jednu obálku");

            if (image.IsCover)
            {
                Insert(0, image);
            }
            else
            {
                base.Add(image);
            }

            RenumberPages();
        }

        public void MoveUp(ScanImage image)
        {
            if (image == null) throw new ArgumentNullException("image");
            if (image.IsCover) throw new ArgumentException("Obálku nelze přesunout vpřed");

            int index = IndexOf(image);

            if (index > 0)
            {
                Move(index, index - 1);
            }

            RenumberPages();
        }

        public void MoveDown(ScanImage image)
        {
            if (image == null) throw new ArgumentNullException("image");
            if (image.IsCover) throw new ArgumentException("Obálku nelze přesunout vzad");

            int index = IndexOf(image);

            if (index >= 0 && index < Count - 1)
            {
                Move(index, index + 1);
            }

            RenumberPages();
        }

        new public void Remove(ScanImage image)
        {
            if (image == null) throw new ArgumentNullException("image");

            ImageFunctions.DeleteFile(image.FullName);
            base.Remove(image);

            RenumberPages();
        }

        public void LoadContents(string fileName)
        {
            if (String.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName");

            string inputFileName = Path.Combine(ScanFolderPath, fileName);
            ScanFileName = Path.GetFileNameWithoutExtension(inputFileName);
            BitmapSource[] sources = ImageFunctions.LoadSplitTiff(inputFileName);

            for (int i = 0; i < sources.Count(); i++)
            {
                ScanImage image = GetNewScanImage();
                image.Source = sources[i];
                image.Save(sources[i]);
                this.Add(image);
            }
        }

        public void Save()
        {
            if (Count == 0) return;

            ////ulozenie obalky v nizsej kvalite
            //if (HasCover)
            //{
            //    BitmapSource cover = ImageFunctions.Load(CoverFullName);
            //    ImageFunctions.SaveJpeg(null, CoverFullName, ImageFunctions.JpegQuality.Optimal);
            //}

            //spojeni stranek obsahu do jedneho souboru
            if (HasContents)
            {
                string[] fileNames = this.Where(f => !f.IsCover).Select(f => f.FullName).ToArray();
                TiffCompressOption compression = (ContentsBW ? TiffCompressOption.Ccitt4 : TiffCompressOption.Lzw);
                ImageFunctions.SaveMergeTiff(fileNames, ContentsFullName, ContentsDpi.Value, compression);
            }
        }

        public void Repair()
        {
            if (String.IsNullOrEmpty(ScanFileName)) throw new ArgumentException("ScanFileName");

            this.Clear();

            //obalka
            string coverFileName = Path.Combine(ScanFolderPath, String.Format("{0}.jpg", ScanFileName));
            if (File.Exists(coverFileName))
            {
                this.Add(new ScanImage(coverFileName, true, this.CoverBW, this.CoverDpi));
            }

            //obsah
            string[] contentsFileNames = Directory.GetFiles(ScanFolderPath, String.Format("{0}#??.tif", ScanFileName));
            if (contentsFileNames != null && contentsFileNames.Count() > 0)
            {
                contentsFileNames = contentsFileNames.OrderBy(f => f).ToArray();

                foreach (string contentsFile in contentsFileNames)
                {
                    Match m = Regex.Match(contentsFile, @"#(?<index>\d+?)\.tif");
                    if (m.Success)
                    {
                        int index = Int32.Parse(m.Groups["index"].Value);
                        if (index > _counter) _counter = index;
                    }

                    this.Add(new ScanImage(contentsFile, false, this.ContentsBW, this.ContentsDpi));
                }

                _counter++;
            }
        }

        public void Delete()
        {
            if (String.IsNullOrEmpty(ScanFolderPath)) return;

            ImageFunctions.DeleteFiles(ScanFolderPath);
            this.Clear();
        }

        #endregion

        #region Private methods

        private void RenumberPages()
        {
            int i = 1;

            foreach (var item in this)
            {
                item.Page = (item.IsCover ? "0" : i++.ToString());
            }
        }

        #endregion
    }
}
