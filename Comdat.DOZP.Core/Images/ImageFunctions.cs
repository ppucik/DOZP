using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using AForge.Imaging.Filters;
using PdfSharp.Pdf;
using PdfSharp.Drawing;

namespace Comdat.DOZP.Core
{
    public static class ImageFunctions
    {
        #region Enumerators

        public enum JpegQuality
        {
            Minimal = 10,
            Low = 25,
            Medium = 50,
            Optimal = 75,
            High = 90,
            Original = 100
        }

        #endregion

        #region Load

        public static byte[] ReadFile(string fileName)
        {
            if (String.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName", "Nebyla zadána cesta k souboru");
            if (!File.Exists(fileName)) throw new ArgumentException(String.Format("Soubor {0} neexistuje", fileName));

            byte[] buffer = null;

            using (FileStream fs = File.OpenRead(fileName))
            {
                int lenght = System.Convert.ToInt32(fs.Length);
                buffer = new byte[lenght];
                fs.Read(buffer, 0, lenght);
                fs.Close();
            }

            return buffer;
        }

        public static BitmapSource Load(string fileName, int height = 0)
        {
            if (String.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName");
            if (!File.Exists(fileName)) throw new ArgumentException(String.Format("Soubor {0} neexistuje", fileName));

            BitmapSource output = null;

            if (fileName.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                Uri uriSource = new Uri(fileName, UriKind.RelativeOrAbsolute);
                output = Load(uriSource, height);
            }
            else
            {
                if (!File.Exists(fileName)) throw new ArgumentException(String.Format("Soubor '{0}' neexistuje", fileName));
                //using (var ms = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                using (var ms = new MemoryStream(File.ReadAllBytes(fileName)))
                {
                    output = Load(ms, height);
                }
            }

            return output;
        }

        public static BitmapSource Load(BitmapSource source, int height)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (height <= 0 || height > source.PixelHeight) return source;

            double scale = height / (double)source.PixelHeight;
            BitmapSource output = new TransformedBitmap(source, new ScaleTransform(scale, scale));

            using (MemoryStream ms = new MemoryStream())
            {
                BmpBitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(output));
                enc.Save(ms);
                output = Load(ms, height);
            }

            return output;
        }

        public static BitmapSource Load(Uri uriSource, int height = 0)
        {
            if (uriSource == null) throw new ArgumentNullException("uriSource");

            BitmapImage output = new BitmapImage();
            output.BeginInit();
            output.CacheOption = BitmapCacheOption.None;
            output.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            if (height > 0) output.DecodePixelHeight = height;
            output.UriSource = uriSource;
            output.EndInit();

            return (output as BitmapSource);
        }

        public static BitmapSource Load(Stream ms, int height = 0)
        {
            if (ms == null) throw new ArgumentNullException("uriSource");

            BitmapImage output = new BitmapImage();

            ms.Position = 0;
            output.BeginInit();
            output.CacheOption = BitmapCacheOption.OnLoad;
            output.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            if (height > 0) output.DecodePixelHeight = height;
            output.StreamSource = ms;
            output.EndInit();
            output.Freeze();
            ms.Close();

            return (output as BitmapSource);
        }

        public static BitmapSource[] LoadSplitTiff(string fileName)
        {
            if (String.IsNullOrEmpty(fileName)) new ArgumentNullException("fileName");
            if (!System.IO.File.Exists(fileName)) throw new ArgumentException(String.Format("Soubor '{0}' neexistuje, nelze načíst TIFF soubor.", fileName));

            BitmapSource[] sources = null;

            using (var ms = new MemoryStream(File.ReadAllBytes(fileName)))
            {
                TiffBitmapDecoder decoder = new TiffBitmapDecoder(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                sources = decoder.Frames.ToArray<BitmapSource>();
                ms.Close();
            }

            return sources;
        }

        public static Image LoadThumbnail(string fileName, int width, int height)
        {
            if (String.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName", "Nebyla zadána cesta k souboru");
            if (!File.Exists(fileName)) throw new ArgumentException(String.Format("Soubor {0} neexistuje", fileName));

            Image output = null;

            using (MemoryStream stream = new MemoryStream(File.ReadAllBytes(fileName)))
            {
                using (Image source = Image.FromStream(stream))
                {
                    int sourceWidth = source.Width;
                    int sourceHeight = source.Height;

                    float widthScaleFactor = ((float)width / (float)sourceWidth);
                    float heightScaleFactor = ((float)height / (float)sourceHeight);
                    float finalScaleFactor = 0;

                    if (heightScaleFactor == 0 && widthScaleFactor == 0)
                        finalScaleFactor = 1f;
                    else if (heightScaleFactor == 0)
                        finalScaleFactor = widthScaleFactor;
                    else if (widthScaleFactor == 0)
                        finalScaleFactor = heightScaleFactor;
                    else if (heightScaleFactor < widthScaleFactor)
                        finalScaleFactor = heightScaleFactor;
                    else
                        finalScaleFactor = widthScaleFactor;

                    int thumbWidth = (int)(sourceWidth * finalScaleFactor);
                    int thumbHeight = (int)(sourceHeight * finalScaleFactor);

                    output = source.GetThumbnailImage(thumbWidth, thumbHeight, delegate() { return false; }, IntPtr.Zero);
                }
            }

            return output;
        }

        #endregion

        #region Save

        public static short WriteFile(string fileName, byte[] stream, bool overwrite = false)
        {
            short pages = 0;

            if (String.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName", "Nebyla zadána cesta k souboru");
            if (stream == null) throw new ArgumentNullException("stream", "Nebyly zadány data souboru");
            if (!overwrite && System.IO.File.Exists(fileName)) throw new ArgumentException(String.Format("Soubor {0} již existuje", fileName));

            string dirPath = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            if (!Path.GetExtension(fileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                using (var ms = new MemoryStream(stream))
                {
                    using (Image image = Image.FromStream(ms))
                    {
                        if (ImageFormat.Tiff.Equals(image.RawFormat))
                            pages = (short)image.GetFrameCount(FrameDimension.Page);
                        else
                            pages = 1;
                    }
                }
            }

            using (FileStream fs = File.Open(fileName, FileMode.Create, FileAccess.Write))
            {
                //System.IO.File.WriteAllBytes(path, stream);
                fs.Write(stream, 0, stream.Length);
                fs.Close();
            }

            return pages;
        }

        public static void SaveJpeg(BitmapSource source, string fileName, JpegQuality quality = JpegQuality.High)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (String.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName");
            if ((int)quality < 1 || (int)quality > 100) throw new ArgumentOutOfRangeException("quality");
            //if (File.Exists(fileName)) File.Delete(fileName);

            if (source.Format != PixelFormats.Rgb24) source = new FormatConvertedBitmap(source, PixelFormats.Rgb24, null, 0);

            BitmapMetadata metadata = GetBitmapMetadata("jpg");

            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.QualityLevel = (int)quality;
                encoder.Frames.Add(BitmapFrame.Create(source, null, metadata, null));
                encoder.Save(fs);
                fs.Close();
            }
        }

        public static void SaveTiff(BitmapSource source, string fileName, int dpi, TiffCompressOption compression)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (String.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName");
            //if (File.Exists(fileName)) File.Delete(fileName);

            BitmapMetadata metadata = GetBitmapMetadata("tiff");

            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                TiffBitmapEncoder encoder = new TiffBitmapEncoder();
                encoder.Compression = compression;
                encoder.Frames.Add(BitmapFrame.Create(source, null, metadata, null));
                encoder.Save(fs);
                fs.Close();
            }
        }

        public static void SaveMergeTiff(string[] fileNames, string outputFileName, int dpi, TiffCompressOption compresion)
        {
            if (fileNames == null || fileNames.Count() == 0) return;
            if (String.IsNullOrEmpty(outputFileName)) new ArgumentNullException("outputFileName");
            if (File.Exists(outputFileName)) File.Delete(outputFileName);

            using (FileStream fs = new FileStream(outputFileName, FileMode.Create, FileAccess.Write))
            {
                TiffBitmapEncoder encoder = new TiffBitmapEncoder();

                foreach (var fileName in fileNames)
                {
                    BitmapSource source = Load(fileName);
                    int bytesPerPixel = (source.Format.BitsPerPixel + 7) / 8;
                    int stride = source.PixelWidth * bytesPerPixel;
                    byte[] bitmapArray = new byte[source.PixelHeight * stride];
                    source.CopyPixels(bitmapArray, stride, 0);
                    BitmapSource output = BitmapSource.Create(source.PixelWidth, source.PixelHeight, dpi, dpi, source.Format, source.Palette, bitmapArray, stride);

                    encoder.Compression = compresion;
                    encoder.Frames.Add(BitmapFrame.Create(output));
                }

                encoder.Save(fs);
                fs.Close();
            }
        }

        public static bool WriteToPdf(string tifFileName, string pdfFileName, string author = null, string title = null, string isbn = null)
        {
            if (String.IsNullOrEmpty(tifFileName)) new ArgumentNullException("tifFileName");
            if (!System.IO.File.Exists(tifFileName)) throw new ArgumentException(String.Format("Soubor '{0}' neexistuje, nelze vytvořit PDF soubor.", tifFileName));
            if (File.Exists(pdfFileName)) File.Delete(pdfFileName);

            using (PdfDocument doc = new PdfDocument())
            {
                doc.Info.Author = author;
                doc.Info.Title = title;
                doc.Info.Keywords = String.Format("ISBN:{0}", isbn);
                doc.Info.Subject = "OBSAH";
                doc.Info.Creator = App.Configuration.CompanyName;
                doc.Info.CreationDate = DateTime.Now;

                Image tiff = System.Drawing.Image.FromFile(tifFileName);
                int count = tiff.GetFrameCount(FrameDimension.Page);

                for (int pageNum = 0; pageNum < count; pageNum++)
                {
                    tiff.SelectActiveFrame(FrameDimension.Page, pageNum);
                    XImage ximg = XImage.FromGdiPlusImage(tiff);

                    PdfPage page = new PdfPage();
                    page.Width = ximg.PointWidth;
                    page.Height = ximg.PointHeight;
                    doc.Pages.Add(page);

                    XGraphics xgr = XGraphics.FromPdfPage(page);
                    xgr.DrawImage(ximg, 0, 0);
                }

                doc.Save(pdfFileName);
                doc.Close();
            }

            return true;
        }

        private static BitmapMetadata GetBitmapMetadata(string format)
        {
            BitmapMetadata metadata = new BitmapMetadata(format);
            metadata.ApplicationName = "DOZP (c) Comdat s.r.o.";
            metadata.Copyright = "UK FF";

            return metadata;
        }

        #endregion

        #region Rotate, Flip

        public static Bitmap Rotate(Bitmap bitmap, float angle)
        {
            if (bitmap == null) throw new ArgumentNullException("bitmap");
            if (angle == 0) return bitmap;

            Bitmap output = output = new Bitmap(bitmap.Width, bitmap.Height);
            output.SetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);

            using (Graphics g = Graphics.FromImage(output))
            {
                g.FillRectangle(System.Drawing.Brushes.White, 0, 0, bitmap.Width, bitmap.Height);
                g.TranslateTransform((float)bitmap.Width / 2, (float)bitmap.Height / 2);
                g.RotateTransform(angle);
                g.TranslateTransform(-(float)bitmap.Width / 2, -(float)bitmap.Height / 2);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawImage(bitmap, new PointF(0, 0));
            }

            return output;
        }

        public static Stream Rotate(byte[] image, float angle)
        {
            if (image == null) throw new ArgumentNullException("image");
            if (angle == 0) return null;

            Stream stream = new MemoryStream();

            using (var ms = new MemoryStream(image))
            {
                using (Bitmap source = new Bitmap(ms))
                {
                    using (Bitmap output = Rotate(source, -angle))
                    {
                        output.Save(stream, ImageFormat.Bmp);
                        //output.Save(@"C:\DOZP\TestRotate.tif");
                    }
                }            
            }

            return stream;
        }

        public static bool Rotate(string fileName, float angle) //, int page
        {
            if (fileName == null) throw new ArgumentNullException("fileName");
            if (!File.Exists(fileName)) throw new ArgumentException(String.Format("Soubor {0} neexistuje", fileName));
            if (angle == 0) return false;

            using (var ms = new MemoryStream(File.ReadAllBytes(fileName)))
            {
                using (Bitmap source = new Bitmap(ms))
                {
                    using (Bitmap output = Rotate(source, -angle))
                    {
                        output.Save(fileName);
                    }
                }
            }

            return true;
        }

        public static BitmapSource Rotate180(BitmapSource source)
        {
            return Transform(source, new RotateTransform(180));
        }

        public static BitmapSource RotateLeft(BitmapSource source)
        {
            return Transform(source, new RotateTransform(-90));
        }

        public static BitmapSource RotateRight(BitmapSource source)
        {
            return Transform(source, new RotateTransform(90));
        }

        public static BitmapSource FlipHorizontal(BitmapSource source)
        {
            return Transform(source, new ScaleTransform(-1, 1));
        }

        public static BitmapSource FlipVertical(BitmapSource source)
        {
            return Transform(source, new ScaleTransform(1, -1));
        }

        public static BitmapSource Transform(BitmapSource source, Transform transform)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (transform == null) throw new ArgumentNullException("transform");

            TransformedBitmap target = new TransformedBitmap();
            target.BeginInit();
            target.Source = source;
            target.Transform = transform;
            target.EndInit();
            target.Freeze();

            return CloneImage(target);
        }

        #endregion

        #region Crop

        public static Rect FindCropZone(BitmapSource source)
        {
            // define treshold of pixels
            int threshold = 30;

            int pixelWidth = source.PixelWidth;
            int pixelHeight = source.PixelHeight;
            int bytesPerPixel = (source.Format.BitsPerPixel + 7) / 8;
            int stride = source.PixelWidth * bytesPerPixel;
            byte[] bitmapArray = new byte[source.PixelHeight * stride];
            source.CopyPixels(bitmapArray, stride, 0);

            int left = pixelWidth, right = 0, top = pixelHeight, bottom = 0;
            for (int j = 0; j < pixelHeight - 1; j++)
            {
                int row = j * pixelWidth;
                for (int i = 0; i < pixelWidth - 1; i++)
                {
                    int index = (row + i) * bytesPerPixel;
                    if (Math.Abs(bitmapArray[index] - bitmapArray[index + 4]) > threshold ||
                        Math.Abs(bitmapArray[index + 1] - bitmapArray[index + 5]) > threshold ||
                        Math.Abs(bitmapArray[index + 2] - bitmapArray[index + 6]) > threshold)
                    {
                        left = (i < left) ? i : left;
                        right = (i > right) ? i + 1 : right;
                        top = (j < top) ? j : top;
                        bottom = (j > bottom) ? j + 1 : bottom;
                    }
                }
            }

            //add some margin for better look
            left = (left >= 2) ? left - 2 : left;
            top = (top >= 2) ? top - 2 : top;
            right = (right <= pixelWidth - 2) ? right + 2 : right;
            bottom = (bottom <= pixelHeight - 2) ? bottom + 2 : bottom;

            return new Rect(left, top, Math.Max(0, right - left), Math.Max(0, bottom - top));
        }

        public static BitmapSource Crop(BitmapSource source, Rect cropZone)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (cropZone == Rect.Empty) return source;

            Int32Rect sourceRect = new Int32Rect((int)cropZone.X, (int)cropZone.Y, (int)cropZone.Width, (int)cropZone.Height);
            CroppedBitmap target = new CroppedBitmap(source, sourceRect);

            return CloneImage(target);
        }

        #endregion

        #region Deskew

        public static float GetDeskewAngle(BitmapSource source)
        {
            if (source == null) return 0;

            float skewAngle = 0;

            using (Bitmap bmp = Convert(source))
            {
                Deskew deskew = new Deskew(bmp);
                skewAngle = (float)deskew.GetSkewAngle();
            }

            return skewAngle;
        }

        public static BitmapSource Deskew(BitmapSource source)
        {
            if (source == null) throw new ArgumentNullException("source");
            float angle = GetDeskewAngle(source);

            return Deskew(source, angle);
        }

        public static BitmapSource Deskew(BitmapSource source, float angle)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (angle == 0) return source;

            BitmapSource output = null;

            using (Bitmap inBmp = Convert(source))
            {
                using (Bitmap outBmp = Rotate(inBmp, -angle))
                {
                    inBmp.Dispose();
                    output = Convert(outBmp);
                }
            }

            return CloneImage(output);
        }

        #endregion

        #region Color

        public static BitmapSource AutoColorCorrections(BitmapSource source)
        {
            if (source == null) throw new ArgumentNullException("source");

            //--- Stretch Contrast ---
            int pixelWidth = source.PixelWidth;
            int pixelHeight = source.PixelHeight;
            int bytesPerPixel = (source.Format.BitsPerPixel + 7) / 8;
            int stride = pixelWidth * bytesPerPixel;
            byte[] bitmapArray = new byte[pixelHeight * stride];
            source.CopyPixels(bitmapArray, stride, 0);

            // find min and max values in image
            byte min = 255;
            byte max = 0;
            int currentPixel = bitmapArray[0];

            for (int i = 1; i < pixelHeight * stride; i++)
            {
                if (i % bytesPerPixel == 0)
                {
                    currentPixel = currentPixel / bytesPerPixel;
                    min = (min < currentPixel) ? min : (byte)currentPixel;
                    max = (max > currentPixel) ? max : (byte)currentPixel;
                    currentPixel = 0;
                }
                currentPixel += bitmapArray[i];
            }

            //stretch contrast in image
            double stretch = (double)255 / (max - min);

            for (int i = 0; i < pixelHeight * stride; i++)
            {
                double newValue = ((bitmapArray[i] - min) * stretch);
                newValue = newValue < 0 ? 0 : newValue;
                newValue = newValue > 255 ? 255 : newValue;
                bitmapArray[i] = (byte)newValue;
            }

            //--- Auto Gamma corection ---

            //detect Gamma value
            long sum = 0;

            for (int i = 0; i < stride * pixelHeight; i++)
            {
                sum += bitmapArray[i];
            }

            long range = 256;
            double average = ((double)sum / (pixelHeight * stride));
            double gamma = Math.Log(range / 2.0) / Math.Log(average);

            //create Gamma array 
            byte[] gammaArray = new byte[256];

            for (int i = 0; i < 256; ++i)
            {
                gammaArray[i] = (byte)Math.Min(255, (int)((255.0 * Math.Pow(i / 255.0, 1.0 / gamma)) + 0.5));
            }

            //Gamma corection
            for (int i = 0; i < pixelHeight * stride; i++)
            {
                bitmapArray[i] = gammaArray[bitmapArray[i]];
            }

            return BitmapSource.Create(pixelWidth, pixelHeight, source.DpiX, source.DpiY, source.Format, source.Palette, bitmapArray, stride);
        }

        public static BitmapSource AdjustColor(BitmapSource source, int brightness, double contrast, double gamma)
        {
            if (source == null) throw new ArgumentNullException("source");

            int pixelWidth = source.PixelWidth;
            int pixelHeight = source.PixelHeight;
            int bytesPerPixel = (source.Format.BitsPerPixel + 7) / 8;
            int stride = pixelWidth * bytesPerPixel;
            byte[] bitmapArray = new byte[pixelHeight * stride];
            source.CopyPixels(bitmapArray, stride, 0);

            //Brightness 
            if (brightness != 0)
            {
                for (int i = 0; i < pixelHeight * stride; i++)
                {
                    int num = brightness + (int)bitmapArray[i];
                    if (num < 0) num = 0;
                    if (num > 255) num = 255;

                    bitmapArray[i] = (byte)num;
                }
            }

            //Contrast
            if (contrast != 0)
            {
                contrast = (100.0 + contrast) / 100.0;
                contrast *= contrast;

                for (int i = 0; i < pixelHeight * stride; i++)
                {
                    double tmpContrast = bitmapArray[i] / 255.0;
                    tmpContrast -= 0.5;
                    tmpContrast *= contrast;
                    tmpContrast += 0.5;
                    tmpContrast *= 255;

                    if (tmpContrast > 255)
                        tmpContrast = 255;
                    else if (tmpContrast < 0)
                        tmpContrast = 0;

                    bitmapArray[i] = (byte)tmpContrast;
                }
            }

            //Gamma
            if (gamma != 1)
            {
                byte[] gammaArray = new byte[256];

                for (int i = 0; i < 256; ++i)
                {
                    gammaArray[i] = (byte)Math.Min(255, (int)((255.0 * Math.Pow(i / 255.0, 1.0 / gamma)) + 0.5));
                }

                for (int i = 0; i < pixelHeight * stride; i++)
                {
                    bitmapArray[i] = gammaArray[bitmapArray[i]];
                }
            }

            return BitmapSource.Create(pixelWidth, pixelHeight, source.DpiX, source.DpiY, source.Format, source.Palette, bitmapArray, stride);
        }

        public static BitmapSource ColorCorrections(BitmapSource source, int brightness, int contrast, double gamma, int hue, float saturation)
        {
            if (source == null) throw new ArgumentNullException("source");

            BitmapSource output = null;

            if (source.Format == PixelFormats.Bgr32 && gamma != 1D)
            {
                source = new FormatConvertedBitmap(source, PixelFormats.Bgr24, null, 0);
            }

            using (Bitmap bmp = Convert(source))
            {
                int bpp = source.Format.BitsPerPixel;

                if (bpp == 8 || bpp == 24 || bpp == 32)
                {
                    //The filter accepts 8 bpp grayscale and 24/32 bpp color images for processing, value [-255, +255] default 10
                    if (brightness >= -255 && brightness != 0 && brightness <= 255)
                    {
                        BrightnessCorrection filter = new BrightnessCorrection(brightness);
                        filter.ApplyInPlace(bmp);
                    }

                    //The filter accepts 8 bpp grayscale and 24/32 bpp color images for processing, value [-127, +127] default 10
                    if (contrast >= -127 && contrast != 0 && contrast <= 127)
                    {
                        ContrastCorrection filter = new ContrastCorrection(contrast);
                        filter.ApplyInPlace(bmp);
                    }
                }

                if (bpp == 8 || bpp == 24)
                {
                    //The filter accepts 8 bpp grayscale and 24 bpp color images for processing, value [0.1, 5.0] default 1.0
                    if (gamma >= 0.1D && gamma != 1D && gamma <= 5D)
                    {
                        GammaCorrection filter = new GammaCorrection(gamma);
                        filter.ApplyInPlace(bmp);
                    }
                }

                if (bpp == 8 || bpp == 24 || bpp == 32)
                {
                    //The filter accepts 8 bpp grayscale and 24/32 bpp color images for processing, value [-180, +180] default 0
                    if (hue >= -180 && hue != 0 && hue <= 180)
                    {
                        HueModifier filter = new HueModifier(hue);
                        filter.ApplyInPlace(bmp);
                    }
                }

                if (bpp == 24 || bpp == 32)
                {
                    //The filter accepts 24 and 32 bpp color images for processing, value specified percentage [-1, +1] default 0.1
                    if (saturation >= -100f && saturation != 0f && saturation <= 100f)
                    {
                        SaturationCorrection filter = new SaturationCorrection(saturation / 100);
                        filter.ApplyInPlace(bmp);
                    }
                }

                output = Convert(bmp);
            }

            return CloneImage(output);
        }

        public static bool ColorCorrections(string fileName, int brightness, int contrast, double gamma, int hue, float saturation)
        {
            if (String.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName");

            bool result = false;

            using (var ms = new MemoryStream(File.ReadAllBytes(fileName)))
            {
                using (Bitmap bmp = new Bitmap(ms))
                {
                    //The filter accepts 8 bpp grayscale and 24/32 bpp color images for processing, value [-255, +255] default 10
                    if (brightness >= -255 && brightness != 0 && brightness <= 255)
                    {
                        BrightnessCorrection filter = new BrightnessCorrection(brightness);
                        filter.ApplyInPlace(bmp);
                        result = true;
                    }

                    //The filter accepts 8 bpp grayscale and 24/32 bpp color images for processing, value [-127, +127] default 10
                    if (contrast >= -127 && contrast != 0 && contrast <= 127)
                    {
                        ContrastCorrection filter = new ContrastCorrection(contrast);
                        filter.ApplyInPlace(bmp);
                        result = true;
                    }

                    //The filter accepts 8 bpp grayscale and 24 bpp color images for processing, value [0.1, 5.0] default 1.0
                    if (gamma >= 0.1D && gamma != 1D && gamma <= 5D)
                    {
                        GammaCorrection filter = new GammaCorrection(gamma);
                        filter.ApplyInPlace(bmp);
                        result = true;
                    }

                    //The filter accepts 8 bpp grayscale and 24/32 bpp color images for processing, value [-180, +180] default 0
                    if (hue >= -180 && hue != 0 && hue <= 180)
                    {
                        HueModifier filter = new HueModifier(hue);
                        filter.ApplyInPlace(bmp);
                        result = true;
                    }

                    //The filter accepts 24 and 32 bpp color images for processing, value specified percentage [-1, +1] default 0.1
                    if (saturation >= -100f && saturation != 0f && saturation <= 100f)
                    {
                        SaturationCorrection filter = new SaturationCorrection(saturation / 100);
                        filter.ApplyInPlace(bmp);
                        result = true;
                    }

                    bmp.Save(fileName);
                }
            }

            return result;
        }

        #endregion

        #region Convert, Clone

        public static BitmapSource Convert(Bitmap bitmap)
        {
            if (bitmap == null) return null;

            BitmapSource output = null;

            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Bmp);
                output = Load(ms);
            }

            return output;
        }

        public static Bitmap Convert(BitmapSource source)
        {
            if (source == null) return null;

            Bitmap output = null;

            using (MemoryStream ms = new MemoryStream())
            {
                BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(source));
                encoder.Save(ms);
                ms.Position = 0;
                output = new Bitmap(ms);
                ms.Close();
            }

            return output;
        }

        public static BitmapData GetBitmapData(BitmapSource source)
        {
            BitmapData data = new BitmapData();
            data.Width = source.PixelWidth;
            data.Height = source.PixelHeight;
            int bytesPerPixel = (source.Format.BitsPerPixel + 7) / 8;
            data.Stride = source.PixelWidth * bytesPerPixel;

            return data;
        }

        public static BitmapSource CloneImage(BitmapSource source)
        {
            if (source == null) return null;

            int color = (source.Format.BitsPerPixel + 7) / 8;
            int stride = source.PixelWidth * color;
            byte[] pixels = new byte[source.PixelHeight * stride];
            source.CopyPixels(pixels, stride, 0);

            return BitmapSource.Create(source.PixelWidth, source.PixelHeight, source.DpiX, source.DpiY, source.Format, source.Palette, pixels, stride);
        }

        #endregion

        #region Util

        public static bool DeleteFile(string fileName)
        {
            if (String.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName");
            if (!File.Exists(fileName)) return false;

            try
            {
                File.Delete(fileName);
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine(String.Format("Nelze vymazat soubor '{0}'", fileName));
                return false;
            }

            return true;
        }

        public static bool DeleteFiles(string dirPath)
        {
            if (String.IsNullOrEmpty(dirPath)) throw new ArgumentNullException("dirPath");
            if (!Directory.Exists(dirPath)) return false;

            try
            {
                DirectoryInfo dir = new DirectoryInfo(dirPath);

                foreach (var file in dir.GetFiles())
                {
                    file.Delete();
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine(String.Format("Nelze vymazat soubory ve složce '{0}'", dirPath));
                return false;
            }

            return true;
        }

        #endregion
    }
}
