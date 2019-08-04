using System;
using System.Drawing;

namespace CJ.Extensions
{
    public class ColorHSL
    {
        public byte A { get => (byte)(A1 * 255); }
        public byte H { get => (byte)(H360 * 240 / 360); }
        public byte S { get => (byte)(S1 * 240); }
        public byte L { get => (byte)(L1 * 240); }

        public float A1 { get; set; }
        public float H360 { get; set; }
        public float S1 { get; set; }
        public float L1 { get; set; }

        #region ctor


        public static ColorHSL FromArgb(int alpha, int red, int green, int blue)
        {
            Color color = Color.FromArgb(alpha, red, green, blue);
            return new ColorHSL()
            {
                A1 = color.A / 255.0f,
                H360 = color.GetHue(),
                S1 = color.GetSaturation(),
                L1 = color.GetBrightness()
            };
        }

        public static ColorHSL FromAhsl(float alpha, float hue, float saturation, float lightness)
        {
            return new ColorHSL()
            {
                A1 = alpha,
                H360 = hue,
                S1 = saturation,
                L1 = lightness
            };
        }

        #endregion

        #region Object Overrides

        public override string ToString() { return string.Format("({0},{1},{2},{3})", A, H, S, L); }

        public override int GetHashCode()
        {
            // 269 and 47 are primes
            int hash = 269;
            hash = (hash * 47) + A;
            hash = (hash * 47) + H;
            hash = (hash * 47) + S;
            hash = (hash * 47) + L;
            return hash;
        }


        #endregion

        #region IEquatable Methods

        public override bool Equals(object obj)
        {
            if (obj is ColorHSL ahsl)
            {
                return Equals(ahsl);
            }
            return false;
        }

        public bool Equals(ColorHSL other)
        {
            if (other is null) return false;
            return (A1 == other.A1 && H360 == other.H360 && S1 == other.S1 && L1 == other.L1);
        }

        public static bool operator ==(ColorHSL ahsl1, ColorHSL ahsl2)
        {
            return ahsl1.Equals(ahsl2);
        }

        public static bool operator !=(ColorHSL ahsl1, ColorHSL ahsl2)
        {
            return !ahsl1.Equals(ahsl2);
        }

        #endregion

        #region Methods

        public static implicit operator Color(ColorHSL ahsl)
        {
            return Ahsl2Argb(ahsl.A1, ahsl.H360, ahsl.S1, ahsl.L1);
        }

        public static implicit operator ColorHSL(Color color)
        {
            return new ColorHSL()
            {
                A1 = color.A / 255.0f,
                H360 = color.GetHue(),
                S1 = color.GetSaturation(),
                L1 = color.GetBrightness()
            };
        }

        // input: alpha in [0,1] h in [0,360] and s,l in [0,1] - output: Color struct
        private static Color Ahsl2Argb(float alpha, float h, float s, float l)
        {
            float a = s * Math.Min(l, 1 - l);
            int f(int n)
            {
                float k = (n + h / 30) % 12;
                return (int)((l - a * Math.Max(Math.Min(Math.Min(k - 3.0f, 9.0f - k), 1.0f), -1.0f)) * 255);
            }
            return Color.FromArgb((int)(alpha * 255), f(0), f(8), f(4));
        }


        #endregion

    }
}
