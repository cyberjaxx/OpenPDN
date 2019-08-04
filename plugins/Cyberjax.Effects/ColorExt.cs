using System;
using System.Drawing;

namespace CJ.Extensions
{
    public static class ColorExt
    {
        public static Color IntToColor(int rgb)
        {
            return Color.FromArgb(255, (byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
        }

        public static Color UIntToColor(uint argb)
        {
            return Color.FromArgb((byte)(argb >> 24), (byte)(argb >> 16), (byte)(argb >> 8), (byte)argb);
        }

        public static Color Overlay(this Color src, Color dest)
        {
            if (src.A == 0) { return dest; }
            if (src.A == 255 || dest.A == 0) { return src; }

            int srcA = src.A;
            int destScale= dest.A * (255 - src.A) / 255;
            int A = srcA + destScale;
            int R = (src.R * srcA + dest.R * destScale) / A;
            int G = (src.G * srcA + dest.G * destScale) / A;
            int B = (src.B * srcA + dest.B * destScale) / A;
            return Color.FromArgb(A, R, G, B);
        }

        public static Color OverlayOpaque(this Color src, Color dest)
        {
            if (src.A == 0) { return dest; }
            if (src.A == 255) { return src; }

            int srcA = src.A;
            int destA = (255 - src.A);
            int A = 255;
            int R = (src.R * srcA + dest.R * destA) / A;
            int G = (src.G * srcA + dest.G * destA) / A;
            int B = (src.B * srcA + dest.B * destA) / A;
            return Color.FromArgb(A, R, G, B);
        }

        public static int CalculateAlpha(Color src, Color dest, Color output)
        {
            float alphaR = float.MaxValue;
            if (src.R != dest.R)
            {
                alphaR = ((float)(output.R - dest.R)) / (src.R - dest.R);
            }
            float alphaG = float.MaxValue;
            if (src.G != dest.G)
            {
                alphaG = ((float)(output.G - dest.G)) / (src.G - dest.G);
            }
            float alphaB = float.MaxValue;
            if (src.G != dest.G)
            {
                alphaB = ((float)(output.B - dest.B)) / (src.B - dest.B);
            }

            // average the valid alphas
            float sum = 0.0f;
            int terms = 0;
            if (0.0 < alphaR && alphaR < 1.0)
            {
                sum += alphaR;
                ++terms;
            }
            if (0.0 < alphaG && alphaG < 1.0)
            {
                sum += alphaG;
                ++terms;
            }
            if (0.0 < alphaB && alphaB < 1.0)
            {
                sum += alphaB;
                ++terms;
            }
            if (terms > 0)
            {
                return Convert.ToInt32((sum / terms) * 255);
            }
            return 0;
        }

        public static int SimpleCalculateAlpha(Color source, Color dest, Color output)
        {
            float sourceAverage = (source.R + source.G + source.B) / 3.0f;
            float destAverage = (dest.R + dest.G + dest.B) / 3.0f;
            float outputAverage = (output.R + output.G + output.B) / 3.0f;
            float alpha = (outputAverage - destAverage) / (sourceAverage - destAverage);
            if (alpha > 1.0) { return 255; }
            return Convert.ToInt32(alpha * 255);
        }

        public static int GrayScale(this Color color)
        {
            return Convert.ToInt32(0.3 * color.R + 0.59 * color.G + 0.11 * color.B);
        }

        public static float GrayScaleF(this Color color)
        {
            return 0.0017647F * color.R + 0.0023137F * color.G + 0.0043137F * color.B;
        }

        public static Color Gray2Rgb(this Color color)
        {
            int gray = GrayScale(color);
            return Color.FromArgb(255, gray, gray, gray);
        }

        public static int GrayScale2(this Color color)
        {
            return (19661 * color.R + 38666 * color.G + 7209 * color.B) >> 16;
        }

        public static int GrayScale3(this Color color)
        {
            return (19737 * color.R + 38817 * color.G + 7237 * color.B) >> 16;
        }
    }
}
