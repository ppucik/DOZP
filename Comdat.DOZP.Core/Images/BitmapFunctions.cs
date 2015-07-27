using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using AForge.Imaging.Filters;

namespace Comdat.DOZP.Core
{
    public static class BitmapFunctions
    {
        public const string TWAIN_SCAN_FILENAME = "TwainScanImage.bmp";

        public static Bitmap Load(string fileName)
        {
            return null;
        }

        public static void Save(Bitmap bitmap, string path)
        {
            if (bitmap == null) throw new ArgumentNullException("bitmap");
            if (String.IsNullOrEmpty(path)) throw new ArgumentNullException("path");

            bitmap.Save(Path.Combine(path, TWAIN_SCAN_FILENAME), ImageFormat.Bmp);
        }

        public static Bitmap AutoColorCorrections(Bitmap source)
        {
            if (source == null) throw new ArgumentNullException("source");

            //Bitmap output = null;

            ContrastStretch filter = new ContrastStretch();
            filter.ApplyInPlace(source);

            //float brightness = 1.0f; // no change in brightness
            //float contrast = 2.0f; // twice the contrast
            //float gamma = 1.0f; // no change in gamma

            //float adjustedBrightness = brightness - 1.0f;

            //// create matrix that will brighten and contrast the image
            //float[][] ptsArray ={
            //            new float[] {contrast, 0, 0, 0, 0}, // scale red
            //            new float[] {0, contrast, 0, 0, 0}, // scale green
            //            new float[] {0, 0, contrast, 0, 0}, // scale blue
            //            new float[] {0, 0, 0, 1.0f, 0}, // don't scale alpha
            //            new float[] {adjustedBrightness, adjustedBrightness, adjustedBrightness, 0, 1}};

            //using (ImageAttributes imageAttrs = new ImageAttributes())
            //{
            //    imageAttrs.ClearColorMatrix();
            //    imageAttrs.SetColorMatrix(new ColorMatrix(ptsArray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            //    imageAttrs.SetGamma(gamma, ColorAdjustType.Bitmap);

            //    using (Graphics g = Graphics.FromImage(output))
            //    {
            //        g.DrawImage(source, new Rectangle(0, 0, output.Width, output.Height), 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, imageAttrs);
            //    }
            //}

            return source;
        }

        //private static double AutoDetectGamma(Bitmap bitmap)
        //{
        //    int width = bitmap.Width;
        //    int height = bitmap.Height;
        //    int colors = bitmap.BitsPerPixel / 8;
        //    int stride = width * colors;

        //    byte[] bitmapArray = new byte[height * stride];
        //    bitmap.CopyPixels(bitmapArray, stride, 0);

        //    long sum = 0;

        //    for (int i = 0; i < stride * height; i++)
        //    {
        //        sum += bitmapArray[i];
        //    }

        //    long range = 256;
        //    double average = ((double)sum / (height * stride));
        //    double gamma = Math.Log(range / 2.0) / Math.Log(average);

        //    return gamma;
        //}

        //public static byte[] ImageToByte(Image img)
        //{
        //    ImageConverter converter = new ImageConverter();
        //    return (byte[])converter.ConvertTo(img, typeof(byte[]));
        //}
    }
}
