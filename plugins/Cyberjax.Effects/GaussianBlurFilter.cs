using PaintDotNet;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cyberjax
{
    public class GaussianBlur
    {
        private readonly int[] _alpha;
        private readonly int[] _red;
        private readonly int[] _green;
        private readonly int[] _blue;

        private readonly int _width;
        private readonly int _height;
        private readonly int _length;

        private readonly ParallelOptions _pOptions;

        public unsafe GaussianBlur(Surface source, CancellationToken token)
        {
            _pOptions = new ParallelOptions { MaxDegreeOfParallelism = 16, CancellationToken = token };

            _width = source.Width;
            _height = source.Height;
            _length = _width * _height;

            _alpha = new int[_length];
            _red = new int[_length];
            _green = new int[_length];
            _blue = new int[_length];

            ColorBgra* srcPtr = source.GetRowAddress(0);

            Parallel.For(0, _length, _pOptions, i =>
            {
                ColorBgra src = srcPtr[i];
                _alpha[i] = src.A;
                _red[i] = src.R;
                _green[i] = src.G;
                _blue[i] = src.B;
            });
        }

        public unsafe void Process(Surface dest, int radial)
        {
            var newAlpha = new int[_length];
            var newRed = new int[_length];
            var newGreen = new int[_length];
            var newBlue = new int[_length];
            ColorBgra* destPtr = dest.GetRowAddress(0);

            Parallel.Invoke(
                () => gaussBlur_4(_alpha, newAlpha, radial),
                () => gaussBlur_4(_red, newRed, radial),
                () => gaussBlur_4(_green, newGreen, radial),
                () => gaussBlur_4(_blue, newBlue, radial));

            Parallel.For(0, _length, _pOptions, i =>
            {
                if (newAlpha[i] > 255) newAlpha[i] = 255;
                else if (newAlpha[i] < 0) newAlpha[i] = 0;
                if (newRed[i] > 255) newRed[i] = 255;
                else if (newRed[i] < 0) newRed[i] = 0;
                if (newGreen[i] > 255) newGreen[i] = 255;
                else if (newGreen[i] < 0) newGreen[i] = 0;
                if (newBlue[i] > 255) newBlue[i] = 255;
                else if (newBlue[i] < 0) newBlue[i] = 0;

                destPtr[i].Bgra = ((uint)(newAlpha[i] << 24) | (uint)(newRed[i] << 16) | (uint)(newGreen[i] << 8) | (uint)newBlue[i]);
            });
        }

        private void gaussBlur_4(int[] source, int[] dest, int r)
        {
            var bxs = boxesForGauss(r, 3);
            boxBlur_4(source, dest, _width, _height, (bxs[0] - 1) / 2);
            boxBlur_4(dest, source, _width, _height, (bxs[1] - 1) / 2);
            boxBlur_4(source, dest, _width, _height, (bxs[2] - 1) / 2);
        }

        private int[] boxesForGauss(int sigma, int n)
        {
            // sqrt((12·sigma²/n) + 1)
            var wIdeal = Math.Sqrt((12 * sigma * sigma / n) + 1);
            var wl = (int)Math.Floor(wIdeal);
            if (wl % 2 == 0) wl--;
            var wu = wl + 2;
            // (12·sigma² - n·wl² - 4n·wl - 3n)/(-4·wl - 4)
            var mIdeal = (double)(12 * sigma * sigma - n * wl * wl - 4 * n * wl - 3 * n) / (-4 * wl - 4);
            var m = (int)Math.Round(mIdeal);

            int[] sizes = new int[n];
            for (var i = 0; i < n; i++) { sizes[i] = (i < m ? wl : wu); }
            return sizes;
        }

        private void boxBlur_4(int[] source, int[] dest, int w, int h, int r)
        {
            Array.Copy(source, dest, source.Length);
            boxBlurH_4(dest, source, w, h, r);
            boxBlurT_4(source, dest, w, h, r);
        }

        private void boxBlurH_4(int[] source, int[] dest, int w, int h, int r)
        {
            //float iar = 1.0F / (r + r + 1);
            IntegerDivider divider = IntegerDivider.GetDivider(r + r + 1);
            Parallel.For(0, h, _pOptions, i =>
            {
                var ti = i * w;
                var li = ti;
                var ri = ti;
                var fv = source[ti];
                var lv = source[ti + w - 1];
                var val = (r + 1) * fv;
                for (var j = 0; j < r; j++)
                {
                    val += source[ri++];
                }
                for (var j = 0; j <= r; j++)
                {
                    val += source[ri++] - fv;
                    dest[ti++] = divider.Divide(val);
                }
                for (var j = r + 1; j < w - r; j++)
                {
                    val += source[ri++] - dest[li++];
                    dest[ti++] = divider.Divide(val);
                }
                for (var j = w - r; j < w; j++)
                {
                    val += lv - source[li++];
                    dest[ti++] = divider.Divide(val);
                }
            });
        }

        private void boxBlurT_4(int[] source, int[] dest, int w, int h, int r)
        {
            //float iar = 1.0F / (r + r + 1);
            IntegerDivider divider = IntegerDivider.GetDivider(r + r + 1);
            Parallel.For(0, w, _pOptions, i =>
            {
                var ti = i;
                var li = ti;
                var ri = ti;
                var fv = source[ti];
                var lv = source[ti + w * (h - 1)];
                var val = (r + 1) * fv; ;
                for (int j = 0; j < r; j++)
                {
                    val += source[ri];
                    ri += w;
                }
                for (var j = 0; j <= r; j++)
                {
                    val += source[ri] - fv;
                    dest[ti] = divider.Divide(val);
                    ri += w;
                    ti += w;
                }
                for (var j = r + 1; j < h - r; j++)
                {
                    val += source[ri] - source[li];
                    dest[ti] = divider.Divide(val);
                    li += w;
                    ri += w;
                    ti += w;
                }
                for (var j = h - r; j < h; j++)
                {
                    val += lv - source[li];
                    dest[ti] = divider.Divide(val);
                    li += w;
                    ti += w;
                }
            });
        }
    }
}
