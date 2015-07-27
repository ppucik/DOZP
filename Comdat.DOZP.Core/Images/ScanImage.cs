using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Comdat.DOZP.Core
{
    public class ScanImage : INotifyPropertyChanged
    {
        #region Constants
        public static readonly int[] RESOLUTIONS = new int[] { 100, 150, 200, 300, 400, 500, 600 };
        const int THUMBNAIL_HEIGHT = 300;
        #endregion

        #region Private variables
        private string _fullName = null;
        private int _height = 1000;
        private BitmapSource _source = null;
        private Size _originalSize = Size.Empty;
        private bool _isCover = false;
        private string _page = null;
        private int? _dpi = 300;
        private bool _blackAndWhite = true;
        private bool _sourceChanged = false;
        #endregion

        #region Constructors

        public ScanImage(string fullName)
        {
            this.FullName = fullName;
        }

        public ScanImage(string fullName, int height)
            : this(fullName)
        {
            this.Height = height;
        }

        public ScanImage(string fullName, bool isCover)
            : this(fullName)
        {
            this.IsCover = isCover;
        }

        public ScanImage(string fullName, bool isCover, bool blackAndWhite, int? dpi)
            : this(fullName, isCover)
        {
            this.BlackAndWhite = blackAndWhite;
            this.Dpi = dpi;
        }

        ~ScanImage()
        {
            _source = null;
        }

        #endregion

        #region Public propeties

        public string FullName
        {
            get
            {
                return _fullName;
            }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException("FullName");

                if (_fullName != value)
                {
                    _fullName = value;
                    OnPropertyChanged("FullName");
                }
            }
        }

        public int Height
        {
            get
            {
                return _height;
            }
            private set
            {
                if (_height != value)
                {
                    _height = value;
                    OnPropertyChanged("Height");
                }
            }
        }

        public bool IsCover
        {
            get
            {
                return _isCover;
            }
            private set
            {
                _isCover = value;
            }
        }

        public string Page
        {
            get
            {
                if (IsCover)
                    return (IsExternalUrl ? "ObalkyKnih.cz" : "Obálka");
                else
                    return _page;
            }
            internal set
            {
                if (_page != value)
                {
                    _page = value;
                    OnPropertyChanged("Page");
                }
            }
        }

        public int? Dpi
        {
            get
            {
                return _dpi;
            }
            set
            {
                _dpi = value;
            }
        }

        public bool BlackAndWhite
        {
            get
            {
                return _blackAndWhite;
            }
            set
            {
                _blackAndWhite = value;
            }
        }

        public BitmapSource Source
        {
            get
            {
                if (_source == null)
                {
                    _source = ImageFunctions.Load(FullName, Height);
                }

                return _source;
            }
            set
            {
                if (_source != value)
                {
                    if (value != null)
                    {
                        _source = ImageFunctions.Load(value, Height);
                        _originalSize = new Size(value.PixelWidth, value.PixelHeight);
                    }

                    else
                    {
                        _source = null;
                        _originalSize = Size.Empty;
                    }

                    SourceChanged = true;
                    OnPropertyChanged("Source");
                }
            }
        }

        public bool SourceChanged
        {
            get
            {
                return _sourceChanged;
            }
            private set
            {
                _sourceChanged = value;
            }
        }

        public Size OriginalSize
        {
            get
            {
                return _originalSize;
            }
            private set
            {
                _originalSize = value;
            }
        }

        public bool IsExternalUrl
        {
            get
            {
                return (!String.IsNullOrEmpty(FullName) && FullName.StartsWith("http", StringComparison.OrdinalIgnoreCase));
            }
        }

        #endregion

        #region Public methods

        public BitmapSource GetThumbnail(int height)
        {
            if (Source != null)
                return ImageFunctions.Load(Source, height);
            else
                return null;
        }

        public BitmapSource GetOriginal()
        {
            BitmapSource original = null;

            if (!String.IsNullOrEmpty(FullName))
            {
                original = ImageFunctions.Load(FullName);
                this.Source = original;
            }

            return original;
        }

        public void Save(BitmapSource original)
        {
            if (this.IsExternalUrl) return;
            if (original == null) throw new ArgumentNullException("original");

            this.Source = original;

            if (this.IsCover)
            {
                ImageFunctions.SaveJpeg(original, FullName);
            }
            else
            {
                TiffCompressOption compression = (BlackAndWhite ? TiffCompressOption.Ccitt4 : TiffCompressOption.Lzw);
                ImageFunctions.SaveTiff(original, FullName, Dpi.Value, compression);
            }

            SourceChanged = false;
        }

        //public void LoadOriginal()
        //{
        //    _source = null;
        //    this.Source = ImageFunctions.Load(this.FullName);
        //}

        //public void Rotate180()
        //{
        //     this.Source = ImageFunctions.Rotate180(this.Source);
        //}

        //public void RotateLeft()
        //{
        //    this.Source = ImageFunctions.RotateLeft(this.Source);
        //}

        //public void RotateRight()
        //{
        //    this.Source = ImageFunctions.RotateRight(this.Source);
        //}

        //public void FlipHorizontal()
        //{
        //    this.Source = ImageFunctions.FlipHorizontal(this.Source);
        //}

        //public void FlipVertical()
        //{
        //    this.Source = ImageFunctions.FlipVertical(this.Source);
        //}

        //public void Deskew(bool fast = true)
        //{
        //    if (fast)
        //    {
        //        float skewAngle = ImageFunctions.GetDeskewAngle(this.Thumbnail);
        //        this.Source = ImageFunctions.Deskew(this.Source, -skewAngle);
        //    }
        //    else
        //    {
        //        this.Source = ImageFunctions.Deskew(this.Source);
        //    }
        //}

        //public void Crop(Rect cropZone)
        //{
        //    this.Source = ImageFunctions.Crop(this.Source, cropZone);
        //}

        //public void AdjustColor(int brightness, double contrast, double gamma)
        //{
        //    this.Source = ImageFunctions.AdjustColor(this.Source, brightness, contrast, gamma);
        //}

        //public void Save()
        //{
        //    if (this.IsExternalUrl) return;

        //    if (this.IsCover)
        //    {
        //        ImageFunctions.SaveJpeg(_source, _fullName);
        //    }
        //    else
        //    {
        //        TiffCompressOption compression = (BlackAndWhite ? TiffCompressOption.Ccitt4 : TiffCompressOption.Lzw);
        //        ImageFunctions.SaveTiff(_source, _fullName, _dpi.Value, compression);
        //    }

        //    _source = null;
        //}

        public bool Equals(ScanImage obj)
        {
            return (obj != null && obj.FullName.Equals(this.FullName));
        }

        public override string ToString()
        {
            return this.FullName;
        }

        #endregion

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
