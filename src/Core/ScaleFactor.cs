/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;

namespace PaintDotNet
{
    /// <summary>
    /// Encapsulates functionality for zooming/scaling coordinates.
    /// Includes methods for Size[F]'s, Point[F]'s, Rectangle[F]'s,
    /// and various scalars
    /// </summary>
    public struct ScaleFactor
    {
        public int Denominator { get; }

        public int Numerator { get; }

        public double Ratio
        {
            get
            {
                return (double)Numerator / (double)Denominator;
            }
        }

        public static readonly ScaleFactor OneToOne = new ScaleFactor(1, 1);
        public static readonly ScaleFactor MinValue = new ScaleFactor(1, 100);
        public static readonly ScaleFactor MaxValue = new ScaleFactor(32, 1);

        private void Clamp() 
        {
            if (this < MinValue)
            {
                this = MinValue;
            }
            else if (this > MaxValue)
            {
                this = MaxValue;
            }
        }

        public static ScaleFactor UseIfValid(int numerator, int denominator, ScaleFactor lastResort)
        {
            if (numerator <= 0 || denominator <= 0)
            {
                return lastResort;
            }
            else
            {
                return new ScaleFactor(numerator, denominator);
            }
        }

        public static ScaleFactor Min(int n1, int d1, int n2, int d2, ScaleFactor lastResort)
        {
            ScaleFactor a = UseIfValid(n1, d1, lastResort);
            ScaleFactor b = UseIfValid(n2, d2, lastResort);
            return ScaleFactor.Min(a, b);
        }

        public static ScaleFactor Max(int n1, int d1, int n2, int d2, ScaleFactor lastResort)
        {
            ScaleFactor a = UseIfValid(n1, d1, lastResort);
            ScaleFactor b = UseIfValid(n2, d2, lastResort);
            return ScaleFactor.Max(a, b);
        }

        public static ScaleFactor Min(ScaleFactor lhs, ScaleFactor rhs)
        {
            if (lhs < rhs)
            {
                return lhs;
            }
            else
            {
                return rhs;
            }
        }

        public static ScaleFactor Max(ScaleFactor lhs, ScaleFactor rhs)
        {
            if (lhs > rhs)
            {
                return lhs;
            }
            else
            {
                return lhs;
            }
        }

        public static bool operator==(ScaleFactor lhs, ScaleFactor rhs)
        {
            return (lhs.Numerator * rhs.Denominator) == (rhs.Numerator * lhs.Denominator);
        }

        public static bool operator!=(ScaleFactor lhs, ScaleFactor rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator<(ScaleFactor lhs, ScaleFactor rhs)
        {
            return (lhs.Numerator * rhs.Denominator) < (rhs.Numerator * lhs.Denominator);
        }

        public static bool operator<=(ScaleFactor lhs, ScaleFactor rhs)
        {
            return (lhs.Numerator * rhs.Denominator) <= (rhs.Numerator * lhs.Denominator);
        }

        public static bool operator>(ScaleFactor lhs, ScaleFactor rhs)
        {
            return (lhs.Numerator * rhs.Denominator) > (rhs.Numerator * lhs.Denominator);
        }

        public static bool operator>=(ScaleFactor lhs, ScaleFactor rhs)
        {
            return (lhs.Numerator * rhs.Denominator) >= (rhs.Numerator * lhs.Denominator);
        }

        public override bool Equals(object obj)
        {
            if (obj is ScaleFactor rhs) 
            {
                return this == rhs;
            }
            else
            {
                return false;
            }
        } 

        public override int GetHashCode()
        {
            return Numerator.GetHashCode() ^ Denominator.GetHashCode();
        }

        private static readonly string percentageFormat = PdnResources.GetString("ScaleFactor.Percentage.Format");
        public override string ToString()
        {
            try
            {
                return string.Format(percentageFormat, unchecked(Math.Round(unchecked(100 * Ratio))));
            }

            catch (ArithmeticException)
            {
                return "--";
            }
        }

        public int ScaleScalar(int x)
        {
            return (int)(((long)x * Numerator) / Denominator);
        }

        public int UnscaleScalar(int x)
        {
            return (int)(((long)x * Denominator) / Numerator);
        }

        public float ScaleScalar(float x)
        {
            return (x * (float)Numerator) / (float)Denominator;
        }

        public float UnscaleScalar(float x)
        {
            return (x * (float)Denominator) / (float)Numerator;
        }

        public double ScaleScalar(double x)
        {
            return (x * (double)Numerator) / (double)Denominator;
        }

        public double UnscaleScalar(double x)
        {
            return (x * (double)Denominator) / (double)Numerator;
        }

        public Point ScalePoint(Point p)
        {
            return new Point(ScaleScalar(p.X), ScaleScalar(p.Y));
        }

        public PointF ScalePoint(PointF p)
        {
            return new PointF(ScaleScalar(p.X), ScaleScalar(p.Y));
        }

        public PointF ScalePointJustX(PointF p)
        {
            return new PointF(ScaleScalar(p.X), p.Y);
        }

        public PointF ScalePointJustY(PointF p)
        {
            return new PointF(p.X, ScaleScalar(p.Y));
        }

        public PointF UnscalePoint(PointF p)
        {
            return new PointF(UnscaleScalar(p.X), UnscaleScalar(p.Y));
        }

        public PointF UnscalePointJustX(PointF p)
        {
            return new PointF(UnscaleScalar(p.X), p.Y);
        }

        public PointF UnscalePointJustY(PointF p)
        {
            return new PointF(p.X, UnscaleScalar(p.Y));
        }

        public Point ScalePointJustX(Point p)
        {
            return new Point(ScaleScalar(p.X), p.Y);
        }

        public Point ScalePointJustY(Point p)
        {
            return new Point(p.X, ScaleScalar(p.Y));
        }

        public Point UnscalePoint(Point p)
        {
            return new Point(UnscaleScalar(p.X), UnscaleScalar(p.Y));
        }

        public Point UnscalePointJustX(Point p)
        {
            return new Point(UnscaleScalar(p.X), p.Y);
        }

        public Point UnscalePointJustY(Point p)
        {
            return new Point(p.X, UnscaleScalar(p.Y));
        }

        public SizeF ScaleSize(SizeF s)
        {
            return new SizeF(ScaleScalar(s.Width), ScaleScalar(s.Height));
        }

        public SizeF UnscaleSize(SizeF s)
        {
            return new SizeF(UnscaleScalar(s.Width), UnscaleScalar(s.Height));
        }

        public Size ScaleSize(Size s)
        {
            return new Size(ScaleScalar(s.Width), ScaleScalar(s.Height));
        }

        public Size UnscaleSize(Size s)
        {
            return new Size(UnscaleScalar(s.Width), UnscaleScalar(s.Height));
        }

        public RectangleF ScaleRectangle(RectangleF rectF)
        {
            return new RectangleF(ScalePoint(rectF.Location), ScaleSize(rectF.Size));
        }

        public RectangleF UnscaleRectangle(RectangleF rectF)
        {
            return new RectangleF(UnscalePoint(rectF.Location), UnscaleSize(rectF.Size));
        }

        public Rectangle ScaleRectangle(Rectangle rect)
        {
            return new Rectangle(ScalePoint(rect.Location), ScaleSize(rect.Size));
        }

        public Rectangle UnscaleRectangle(Rectangle rect)
        {
            return new Rectangle(UnscalePoint(rect.Location), UnscaleSize(rect.Size));
        }

        private static readonly double[] scales = 
            { 
                0.01, 0.02, 0.03, 0.04, 0.05, 0.06, 0.08, 0.12, 0.16, 0.25, 0.33, 0.50, 0.66, 1,
                2, 3, 4, 5, 6, 7, 8, 12, 16, 24, 32
            };

        /// <summary>
        /// Gets a list of values that GetNextLarger() and GetNextSmaller() will cycle through.
        /// </summary>
        /// <remarks>
        /// 1.0 is guaranteed to be in the array returned by this property. This list is also
        /// sorted in ascending order.
        /// </remarks>
        public static double[] PresetValues
        {
            get
            {
                double[] returnValue = new double[scales.Length];
                scales.CopyTo(returnValue, 0);
                return returnValue;
            }
        }

        /// <summary>
        /// Rounds the current scaling factor up to the next power of two.
        /// </summary>
        /// <returns>The new ScaleFactor value.</returns>
        public ScaleFactor GetNextLarger()
        {
            double ratio = Ratio + 0.005;

            int index = Array.FindIndex(
                scales,
                delegate(double scale)
                {
                    return ratio <= scale;
                });

            if (index == -1)
            {
                index = scales.Length;
            }

            index = Math.Min(index, scales.Length - 1);

            return ScaleFactor.FromDouble(scales[index]);
        }

        public ScaleFactor GetNextSmaller()
        {
            double ratio = Ratio - 0.005;

            int index = Array.FindIndex(
                scales,
                delegate(double scale)
                {
                    return ratio <= scale;
                });

            --index;

            if (index == -1)
            {
                index = 0;
            }

            index = Math.Max(index, 0);

            return ScaleFactor.FromDouble(scales[index]);
        }

        private static ScaleFactor Reduce(int numerator, int denominator)
        {
            int factor = 2;

            while (factor < denominator && factor < numerator)
            {
                if ((numerator % factor) == 0 && (denominator % factor) == 0)
                {
                    numerator /= factor;
                    denominator /= factor;
                }
                else
                {
                    ++factor;
                }
            }

            return new ScaleFactor(numerator, denominator);
        }

        public static ScaleFactor FromDouble(double scalar)
        {
            int numerator = (int)(Math.Floor(scalar * 1000.0));
            int denominator = 1000;
            return Reduce(numerator, denominator);
        }

        public ScaleFactor(int numerator, int denominator)
        {
            if (denominator <= 0)
            {
                throw new ArgumentOutOfRangeException("denominator", "must be greater than 0");
            }

            if (numerator < 0)
            {
                throw new ArgumentOutOfRangeException("numerator", "must be greater than 0");
            }

            this.Numerator = numerator;
            this.Denominator = denominator;
            this.Clamp();
        }
    }
}
