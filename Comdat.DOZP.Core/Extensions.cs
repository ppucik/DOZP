using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Comdat.DOZP.Core
{
    public static class Extensions
    {
        public static string FirstIndexOf(this string s, char c)
        {
            if (!String.IsNullOrEmpty(s) & s.IndexOf(c) > 0)
                return s.Substring(0, s.IndexOf(c));
            else
                return s;
        }

        public static string Left(this string s, int length)
        {
            if (!String.IsNullOrEmpty(s))
                return s.Substring(0, Math.Min(length, s.Length));
            else
                return s;
        }

        public static string Substring(this string s, string from = null, string until = null, StringComparison comparison = StringComparison.InvariantCulture)
        {
            var fromLength = (from ?? String.Empty).Length;
            var startIndex = (!String.IsNullOrEmpty(from) ? s.IndexOf(from, comparison) + fromLength : 0);

            if (startIndex < fromLength)
                throw new ArgumentException("Failed to find an instance of the first anchor", "from");

            var endIndex = (!String.IsNullOrEmpty(until) ? s.IndexOf(until, startIndex, comparison) : s.Length);

            if (endIndex < 0)
                throw new ArgumentException("Failed to find an instance of the last anchor", "until");

            return s.Substring(startIndex, endIndex - startIndex);
        }

        public static string TrimStart(this string s, string trim)
        {
            if (!String.IsNullOrEmpty(trim) && s.StartsWith(trim, StringComparison.OrdinalIgnoreCase))
                return s.Substring(trim.Length);
            else
                return s;
        }

        public static string TrimEnd(this string s, string trim)
        {
            if (!String.IsNullOrEmpty(trim) && s.EndsWith(trim, StringComparison.OrdinalIgnoreCase))
                return s.Substring(0, s.Length - trim.Length);
            else
                return s;
        }

        public static string RemoveDiacritics(this string s)
        {
            if (String.IsNullOrEmpty(s)) return String.Empty;

            StringBuilder sb = new StringBuilder();
            s = s.Normalize(NormalizationForm.FormD);

            for (int i = 0; i < s.Length; i++)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(s[i]) != UnicodeCategory.NonSpacingMark) sb.Append(s[i]);
            }

            return sb.ToString();
        }

        //public static int? ToZeroNull(this int num)
        //{
        //    return (num > 0 ? (int?)num : null);
        //}

        public static string ToDisplay(this bool b, bool upper = false)
        {
            string s = (b ? "Ano" : "Ne");

            return (upper ? s.ToUpper() : s);
        }

        public static string ToDisplay(this Enum e)
        {
            string description = e.ToString();
            Type type = e.GetType();
            MemberInfo[] member = type.GetMember(e.ToString());

            if ((member != null) && (member.Length > 0))
            {
                object[] attributes = member[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if ((attributes != null) && (attributes.Length > 0))
                {
                    description = ((DescriptionAttribute)attributes[0]).Description;
                }
            }

            return description;
        }

        public static string ToFileSize(this long l)
        {
            return String.Format(new FileSizeFormatProvider(), "{0:fs}", l);
        }

        public static byte[] ToByteArray(this Stream stream)
        {
            using (stream)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        public static byte[] ToByteArray(this Bitmap bitmap, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, format);
                return ms.ToArray();
            }
        }

        //public static byte[] ToByteArray(Bitmap bitmap)
        //{
        //    BitmapData bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
        //    int numbytes = bmpdata.Stride * bitmap.Height;
        //    byte[] bytedata = new byte[numbytes];
        //    IntPtr ptr = bmpdata.Scan0;

        //    System.Runtime.InteropServices.Marshal.Copy(ptr, bytedata, 0, numbytes);

        //    bitmap.UnlockBits(bmpdata);

        //    return bytedata;
        //}
    }
}
