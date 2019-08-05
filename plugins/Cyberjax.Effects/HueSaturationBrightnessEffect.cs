using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.PropertySystem;
using System;
using System.Drawing;

namespace Cyberjax
{
    [EffectCategory(EffectCategory.Effect)]
    public sealed class HueSaturationBrightnessEffect
        : PropertyBasedEffect
    {
        protected override PropertyCollection OnCreatePropertyCollection()
        {
            return PropertyCollection.CreateEmpty();
        }

        public HueSaturationBrightnessEffect()
            : base(Properties.Resources.HueSaturationBrightnessEffectName,
                   Properties.Resources.HueSaturation,
                   "Cyberjax",
                   EffectFlags.None)
        {
        }

        protected unsafe override void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            Surface dst = DstArgs.Surface;
            int maxX = dst.Width - 1;
            int maxY = dst.Height - 1;
            int bands = 8;

            for (int r = startIndex; r < startIndex + length; ++r)
            {
                Rectangle rect = rois[r];

                for (int y = rect.Top; y < rect.Bottom; ++y)
                {
                    ColorBgra* dstPtr = dst.GetPointAddressUnchecked(rect.Left, y);
                    int band = Math.DivRem(y * bands + maxY - 1, maxY, out int rem);
                    float saturation = (float)rem / maxY;

                    for (int x = rect.Left; x < rect.Right; ++x)
                    {
                        float hue = x * 360.0f / maxX;

                        *dstPtr++ = ColorBgraExt.FromAhsl(1.0f, hue, 1.0f - saturation, (float)band / bands);
                    }
                }
            }
        }
    }
}
