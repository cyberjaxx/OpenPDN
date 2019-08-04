using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.PropertySystem;
using System;
using System.Drawing;

namespace Cyberjax
{
    [EffectCategory(EffectCategory.Adjustment)]
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
                   null,
                   EffectFlags.None)
        {
        }

        protected unsafe override void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            Surface dst = DstArgs.Surface;
            int w = dst.Width - 1;
            int h = dst.Height - 1;
            int bands = 8;

            for (int r = startIndex; r < startIndex + length; ++r)
            {
                Rectangle rect = rois[r];

                for (int y = rect.Top; y < rect.Bottom; ++y)
                {
                    ColorBgra* dstPtr = dst.GetPointAddressUnchecked(rect.Left, y);
                    int band = Math.DivRem(y * bands + h - 1, h, out int rem);
                    float saturation = (float)rem / h;

                    for (int x = rect.Left; x < rect.Right; ++x)
                    {
                        float hue = x * 360.0f / w;

                        *dstPtr++ = ColorBgraExt.FromAhsl(1.0f, hue, 1.0f - saturation, (float)band / bands);
                    }
                }
            }
        }
    }
}
