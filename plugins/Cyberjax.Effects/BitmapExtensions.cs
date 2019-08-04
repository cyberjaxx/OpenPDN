using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace CJ.Extensions
{
    public static class BitmapExtensions
    {
        public static Bitmap ExtractSource(this Bitmap outputBitmap, Color dest, Color source)
        {
            int cols = outputBitmap.Width;
            int rows = outputBitmap.Height;
            Bitmap sourceBitmap = new Bitmap(cols, rows);

            for (int col = 0; col < cols; ++col)
            {
                for (int row = 0; row < rows; ++row)
                {
                    Color output = outputBitmap.GetPixel(col, row);
                    if (output == source)
                    {
                        sourceBitmap.SetPixel(col, row, output);
                    }
                    else if (output != dest)
                    {
                        int alpha = ColorExt.SimpleCalculateAlpha(source, dest, output);
                        if (alpha > 0)
                        {
                            sourceBitmap.SetPixel(col, row, Color.FromArgb(alpha, output));
                        }
                    }
                }
            }
            return sourceBitmap;
        }

        public static Bitmap ExtractSource(this Bitmap bitmap, Color dest, Color source, Color color)
        {
            Bitmap newBitmap = ExtractSource(bitmap, dest, source);
            newBitmap.ReplaceColor(color);
            return newBitmap;
        }

        public static Rectangle GetBounds(this Bitmap bitmap)
        {
            return new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        }

        public delegate Color PixelTransform(Color srcPixel, Color destPixel);

        public static void Clear(this Bitmap bitmap, Color color)
        {
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(color);
            }
        }

        public static void ScaleAlpha(this Bitmap bitmap, int x, int y, int scale)
        {
            Color color = bitmap.GetPixel(x, y);
            int alpha = color.A * scale / 255;
            color = Color.FromArgb(alpha, color);
            bitmap.SetPixel(x, y, color);
        }

        public static void ScaleAlpha(this Bitmap bitmap, int[,] scale, Point location, Rectangle srcRect)
        {
            //Rectangle rect = bitmap.GetBounds();
            Rectangle rect = new Rectangle(location, srcRect.Size);
            byte[] argbValues = bitmap.GetBytes(rect, out BitmapData bmpData);
            int index = 3;
            int scaleX = srcRect.X;
            for (int x = 0; x < srcRect.Width; ++x)
            {
                int scaleY = srcRect.Y;
                for (int y = 0; y < srcRect.Height; ++y)
                {
                    // for speed over perfection, we'll divide by 256 instead of 255
                    argbValues[index] = (byte)(argbValues[index] * scale[scaleX, scaleY++] / 256);
                    index += 4;
                }

                scaleX++;
            }
            bitmap.SetBytes(argbValues, rect, bmpData);
        }

        public static void LimitAlpha(this Bitmap bitmap, Rectangle bmpRect, int maxAlpha)
        {
            byte[] argbValues = bitmap.GetBytes(bmpRect, out BitmapData bmpData);
            for (int index = 3; index < argbValues.Length; index += 4)
            {
                if (argbValues[index] > maxAlpha)
                {
                    argbValues[index] = (byte)maxAlpha;
                }
            }
            bitmap.SetBytes(argbValues, bmpRect, bmpData);
        }

        public static void ReplaceColor(this Bitmap bitmap, Color color)
        {
            Rectangle bmpRect = bitmap.GetBounds();
            byte[] argbValues = bitmap.GetBytes(bmpRect, out BitmapData bmpData);
            for (int index = 0; index < argbValues.Length; index += 4)
            {
                argbValues[index] = color.B;
                argbValues[index + 1] = color.G;
                argbValues[index + 2] = color.R;
            }
            bitmap.SetBytes(argbValues, bmpRect, bmpData);
        }

        public static void ApplyTransparencyMask(this Bitmap bitmap, Bitmap mask, Rectangle bmpRect, Rectangle maskRect)
        {
            byte[] argbValues = bitmap.GetBytes(bmpRect, out BitmapData bmpData);
            byte[] maskValues = mask.GetBytes(maskRect, out BitmapData maskData, ImageLockMode.ReadOnly);
            for (int index = 3; index < argbValues.Length; index += 4)
            {
                // for speed over perfection, we'll divide by 256 instead of 255
                argbValues[index] = (byte)(argbValues[index] * (255 - maskValues[index]) / 256);
            }
            mask.UnlockBits(maskData);
            bitmap.SetBytes(argbValues, bmpRect, bmpData);
        }


        public static Bitmap CreateMask(this Bitmap bitmap, Color filterColor, int tolerance)
        {
            Bitmap maskBitmap = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
            Rectangle bitmapRect = bitmap.GetBounds();

            byte[] maskValues = maskBitmap.GetBytes(bitmapRect, out BitmapData maskData);
            byte[] bmpValues = bitmap.GetBytes(bitmapRect, out BitmapData bmpData, ImageLockMode.ReadOnly);


            int[] filterValues = new int[] { filterColor.B, filterColor.G, filterColor.R };

            for (int index = 0; index < bmpValues.Length; index += 4)
            {
                maskValues[index + 3] = 255;
                for (int i = 0; i < 3; ++i)
                {
                    int delta = bmpValues[index + i] - filterValues[i];
                    if (delta < 0) { delta = -delta; }
                    if (delta > tolerance)
                    {
                        maskValues[index + 3] = 0;
                        break;
                    }
                }
            }

            bitmap.UnlockBits(bmpData);
            maskBitmap.SetBytes(maskValues, bitmapRect, maskData);

            return maskBitmap;
        }

        public static void Recolor(this Bitmap outBitmap, Bitmap origBitmap, Bitmap maskBitmap, Rectangle drawRect, Rectangle maskRect, Color drawColor, Color filterColor, int tolerance)
        {

            byte[] filterValues = new byte[] { filterColor.B, filterColor.G, filterColor.R };
            byte[] drawValues = new byte[] { drawColor.B, drawColor.G, drawColor.R };

            byte[] outValues = outBitmap.GetBytes(drawRect, out BitmapData outData);
            byte[] origValues = origBitmap.GetBytes(drawRect, out BitmapData origData, ImageLockMode.ReadOnly);
            byte[] maskValues = maskBitmap.GetBytes(maskRect, out BitmapData maskData, ImageLockMode.ReadOnly);

            for (int index = 0; index < outValues.Length; index += 4)
            {
                byte maskAlpha = maskValues[index + 3];
                if (maskAlpha > 0)
                {
                    bool recolorPixel = true;
                    for (int i = 0; i < 3; ++i)
                    {
                        int delta = origValues[index + i] - filterValues[i];
                        if (delta < 0) { delta = -delta; }
                        if (delta > tolerance)
                        {
                            recolorPixel = false;
                            break;
                        }
                    }
                    if (recolorPixel)
                    {
                        byte origAlpha = origValues[index + 3];
                        if (maskAlpha == 255 || origAlpha == 0)
                        {
                            // replace out pixel with the draw color pixel
                            outValues[index] = drawValues[0];
                            outValues[index + 1] = drawValues[1];
                            outValues[index + 2] = drawValues[2];
                            outValues[index + 3] = drawColor.A;
                        }
                        else
                        {
                            // replace out pixel with blended out and draw color pixels
                            int drawScale = maskAlpha * drawColor.A / 255;
                            int outScale = origValues[index + 3] * (255 - drawScale) / 255;
                            int outAlpha = drawScale + outScale;
                            for (int i = 0; i < 3; ++i)
                            {
                                outValues[index + i] = (byte)((drawValues[i] * drawScale + outValues[index + i] * outScale) / outAlpha);
                            }

                            outValues[index + 3] = (byte)Math.Min(outAlpha, drawColor.A);
                        }

                    }
                }
            }

            maskBitmap.UnlockBits(maskData);
            origBitmap.UnlockBits(origData);
            outBitmap.SetBytes(outValues, drawRect, outData);
        }

        public static void FillWithHSLGradient(this Bitmap bitmap)
        {
            Rectangle bmpRect = bitmap.GetBounds();
            byte[] argbValues = bitmap.GetBytes(bmpRect, out BitmapData bmpData);

            for (int row = 0, index = 0; row < bitmap.Height; ++row)
            {
                float saturation = ((bitmap.Height - 1.0f) - row) / (bitmap.Height - 1.0f);

                for (int col = 0; col < bitmap.Width; ++col, index += 4)
                {
                    float hue = col * 360.0f / (bitmap.Width - 1.0f);

                    Color color = ColorHSL.FromAhsl(1.0f, hue, saturation, 0.5f);
                    argbValues[index] = color.B;
                    argbValues[index + 1] = color.G;
                    argbValues[index + 2] = color.R;
                    argbValues[index + 3] = color.A;
                }
            }
            bitmap.SetBytes(argbValues, bmpRect, bmpData);
        }

        public static byte[] GetBytes(this Bitmap bitmap, Rectangle rect, out BitmapData bmpData, ImageLockMode mode = ImageLockMode.ReadWrite)
        {
            // Lock the bitmap's bits.
            bmpData = bitmap.LockBits(rect, mode,
                bitmap.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = rect.Width * rect.Height * 4;
            byte[] byteArray = new byte[bytes];

            // Copy the RGB values into the array.
            if (bitmap.Size == rect.Size)
            {
                System.Runtime.InteropServices.Marshal.Copy(ptr, byteArray, 0, bytes);
            }
            else
            {
                int length = rect.Width * 4;
                for (int row = rect.Top, index = 0; row < rect.Bottom; ++row, index += length)
                {
                    System.Runtime.InteropServices.Marshal.Copy(ptr, byteArray, index, length);
                    ptr = IntPtr.Add(ptr, bmpData.Stride);
                }
            }

            return byteArray;
        }

        public static void SetBytes(this Bitmap bitmap, byte[] byteArray, Rectangle rect, BitmapData bmpData)
        {
            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Copy the ARGB values back to the bitmap
            if (bitmap.Size == rect.Size)
            {
                System.Runtime.InteropServices.Marshal.Copy(byteArray, 0, ptr, byteArray.Length);
            }
            else
            {
                int length = rect.Width * 4;
                for (int row = rect.Top, index = 0; row < rect.Bottom; ++row, index += length)
                {
                    System.Runtime.InteropServices.Marshal.Copy(byteArray, index, ptr, length);
                    ptr = IntPtr.Add(ptr, bmpData.Stride);
                }
            }
            // Unlock the bits.
            bitmap.UnlockBits(bmpData);
        }
    }
}
