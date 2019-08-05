using PaintDotNet;
using System;
using System.Drawing;
using Cyberjax.Geometry;

namespace Cyberjax
{
    public static class ColorBgraExt
    {
        public static float GetSaturation(this ColorBgra colorBgra)
        {
            Color colorArgb = colorBgra.ToColor();
            return colorArgb.GetSaturation();
        }

        public static float GetHue(this ColorBgra colorBgra)
        {
            Color colorArgb = colorBgra.ToColor();
            return colorArgb.GetHue();
        }

        public static float GetBrightness(this ColorBgra colorBgra)
        {
            Color colorArgb = colorBgra.ToColor();
            return colorArgb.GetBrightness();
        }

        // input: alpha in [0,1] h in [0,360] and s,l in [0,1] - output: Color struct
        public static ColorBgra FromAhsl(float alpha, float h, float s, float l)
        {
            if (s == 0)
            {
                // If s is 0, all colors are the same.
                // This is some flavor of gray.
                byte l255 = (byte)(l * 255);
                return ColorBgra.FromBgra(l255, l255, l255, (byte)(alpha * 255));
            }
            else
            {
                float a = s * Math.Min(l, 1.0f - l);
                float h1 = h / 30F;
                byte f(float n) // using a float (rather than int) to avoid overhead of conversion
                {
                    float k = (n + h1) % 12F;
                    return (byte)((l - a * Math.Max(Math.Min(Math.Min(k - 3.0f, 9.0f - k), 1.0f), -1.0f)) * 255);
                }
                return ColorBgra.FromBgra(f(4F), f(8F), f(0F), (byte)(alpha * 255));
            }
        }

        /// <summary>
        /// Returns the xor of the B, G, and R values. Alpha is unchanged.
        /// </summary>
        /// <returns></returns>
        /// </remarks>
        public static ColorBgra Xor (this ColorBgra thisColor, ColorBgra otherColor)
        {
            return ColorBgra.FromBgra(
                (byte)(thisColor.B ^ otherColor.B),
                (byte)(thisColor.G ^ otherColor.G),
                (byte)(thisColor.R ^ otherColor.R),
                thisColor.A);
        }

        /// <summary>
        /// Returns the maximum deviance out of the B, G, and R values. Alpha is ignored.
        /// </summary>
        /// <returns></returns>
        /// </remarks>
        public static byte MaxDeviance(this ColorBgra colorBgra, ColorBgra other)
        {
            return Math.Max((byte)(colorBgra.B ^ other.B),
                Math.Max((byte)(colorBgra.G ^ other.G),
                 (byte)(colorBgra.R ^ other.R)));
        }

        /// <summary>
        /// Returns the float average of the B, G, and R values. Alpha is ignored.
        /// </summary>
        /// <returns></returns>
        public static float GetAverageColorChannelValueF(this ColorBgra colorBgra)
        {
            return (colorBgra.B + colorBgra.G + colorBgra.R) / 3.0f;
        }

        /// <summary>
        /// Returns the color after scaling the sums. Alpha is ignored.
        /// </summary>
        /// <returns></returns>
        public static ColorBgra FromBgrSumScaled(float sumB, float sumG, float sumR, float scale)
        {
            byte red = Utility.ClampToByte(Math.Round(sumR * scale));
            byte green = Utility.ClampToByte(Math.Round(sumG * scale));
            byte blue = Utility.ClampToByte(Math.Round(sumB * scale));
            return ColorBgra.FromBgr(blue, green, red);
        }

        /// <summary>
        /// Returns a Point3D object where X = R, Y = G, & Z = B. Alpha is ignored.
        /// </summary>
        /// <returns></returns>
        public static Point3D ToPoint3D(this ColorBgra colorBgra)
        {
            return new Point3D(colorBgra.R, colorBgra.G, colorBgra.B);
        }

        /// <summary>
        /// Returns a Point3D object where X = R, Y = G, & Z = B. Alpha is ignored.
        /// </summary>
        /// <returns></returns>
        public static ColorBgra FromPoint3D(Point3D point)
        {
            return ColorBgra.FromBgr(Utility.ClampToByte(point.Z),
                Utility.ClampToByte(point.Y), Utility.ClampToByte(point.X));
        }

        /// <summary>
        /// Returns a Vector3D object where X = R, Y = G, & Z = B. Alpha is ignored.
        /// </summary>
        /// <returns></returns>
        public static Vector3D ToVector3D(this ColorBgra colorBgra)
        {
            return new Vector3D(colorBgra.R, colorBgra.G, colorBgra.B);
        }

        /// <summary>
        /// Returns a Vector3D object where X = other.R - colorBgra.R,
        /// Y = other.G - colorBgra.G, & Z = other.B - colorBgra.B. Alpha is ignored.
        /// </summary>
        /// <returns></returns>
        public static Vector3D ToVector3D(this ColorBgra colorBgra, ColorBgra other)
        {
            return other.ToPoint3D() - colorBgra.ToPoint3D();
        }
    }
}
