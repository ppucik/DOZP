using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

//https://github.com/jaydeep17/CameraTest7/blob/master/CameraTest7/Utils.cs

namespace Comdat.DOZP.Core
{
    public class Deskew
    {
        /// <summary>
        /// Representation of a line in the image.
        /// </summary>
        private class HoughLine
        {
            // Count of points in the line.
            public int Count;
            // Index in Matrix.
            public int Index;
            // The line is represented as all x,y that solve y*cos(alpha)-x*sin(alpha)=d
            public double Alpha;
            //
            public double D;
        }

        // The Bitmap
        Bitmap _internalBmp = null;

        // The range of angles to search for lines
        const double ALPHA_START = -20;
        const double ALPHA_STEP = 0.2;
        const int STEPS = 40 * 5;
        const double STEP = 1;

        // Precalculation of sin and cos.
        double[] _sinA;
        double[] _cosA;

        // Range of d
        double _min;
        int _count;

        // Count of points that fit in a line.
        int[] _hMatrix;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bmp">Input image to deskew</param>
        public Deskew(Bitmap bmp)
        {
            _internalBmp = bmp;
        }

        /// <summary>
        /// Calculate the skew angle of the image cBmp.
        /// </summary>
        /// <returns></returns>
        public double GetSkewAngle()
        {
            // Average angle of the lines
            double sum = 0;
            int count = 0;

            // Hough Transformation
            Calc();

            // Top 20 of the detected lines in the image.
            HoughLine[] hl = GetTop(20);

            for (int i = 0; i <= 19; i++)
            {
                sum += hl[i].Alpha;
                count += 1;
            }

            return (sum / (double)count);
        }

        /// <summary>
        /// 
        /// </summary>
        private void Init()
        {
            // Precalculation of sin and cos.
            _cosA = new double[STEPS];
            _sinA = new double[STEPS];

            for (int i = 0; i < STEPS; i++)
            {
                double angle = GetAlpha(i) * Math.PI / 180.0;
                _sinA[i] = Math.Sin(angle);
                _cosA[i] = Math.Cos(angle);
            }

            // Range of d:            
            _min = -_internalBmp.Width;
            _count = (int)(2 * (_internalBmp.Width + _internalBmp.Height) / STEP);
            _hMatrix = new int[_count * STEPS];
        }

        /// <summary>
        /// Hough Transforamtion.
        /// </summary>
        private void Calc()
        {
            int hMin = _internalBmp.Height / 4;
            int hMax = _internalBmp.Height * 3 / 4;

            Init();

            for (int y = hMin; y <= hMax; y++)
            {
                for (int x = 1; x <= _internalBmp.Width - 2; x++)
                {
                    // Only lower edges are considered.
                    if (IsBlack(x, y))
                    {
                        if (!IsBlack(x, y + 1))
                        {
                            Calc(x, y);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Calculate all lines through the point (x,y).
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void Calc(int x, int y)
        {
            for (int alpha = 0; alpha <= STEPS - 1; alpha++)
            {
                double d = y * _cosA[alpha] - x * _sinA[alpha];
                int calculatedIndex = (int)CalcDIndex(d);
                int index = calculatedIndex * STEPS + alpha;

                try
                {

                    _hMatrix[index] += 1;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }
            }
        }

        /// <summary>
        /// Calculate the Count lines in the image with most points.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        private HoughLine[] GetTop(int count)
        {
            HoughLine[] hl = new HoughLine[count];

            for (int i = 0; i <= count - 1; i++)
            {
                hl[i] = new HoughLine();
            }

            for (int i = 0; i <= _hMatrix.Length - 1; i++)
            {
                if (_hMatrix[i] > hl[count - 1].Count)
                {
                    hl[count - 1].Count = _hMatrix[i];
                    hl[count - 1].Index = i;
                    int j = count - 1;

                    while (j > 0 && hl[j].Count > hl[j - 1].Count)
                    {
                        HoughLine tmp = hl[j];
                        hl[j] = hl[j - 1];
                        hl[j - 1] = tmp;
                        j -= 1;
                    }
                }
            }

            for (int i = 0; i <= count - 1; i++)
            {
                int dIndex = hl[i].Index / STEPS;
                int alphaIndex = hl[i].Index - dIndex * STEPS;
                hl[i].Alpha = GetAlpha(alphaIndex);
                hl[i].D = dIndex + _min;
            }

            return hl;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private static double GetAlpha(int index)
        {
            return ALPHA_START + index * ALPHA_STEP;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        private double CalcDIndex(double d)
        {
            return Convert.ToInt32(d - _min);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool IsBlack(int x, int y)
        {
            Color col = _internalBmp.GetPixel(x, y);
            double luminance = (col.R * 0.299) + (col.G * 0.587) + (col.B * 0.114);
            return luminance < 140;
        }
    }
}
