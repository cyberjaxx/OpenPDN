using System;
using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.PropertySystem;
using System.Drawing;

namespace Cyberjax
{
    [EffectCategory(EffectCategory.Adjustment)]
    public sealed class ColorDeltaGraphEffect
        : PropertyBasedEffect
    {
        private ColorDeltaOp PixelOp { get; set; }

        public ColorBgra SelectedColor => EnvironmentParameters.SecondaryColor;

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            return PropertyCollection.CreateEmpty();
        }

        protected override void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            PixelOp.SelectedColor = SelectedColor;
            PixelOp.Apply(DstArgs.Surface, SrcArgs.Surface, rois, startIndex, length);
        }

        public ColorDeltaGraphEffect()
            : base(Properties.Resources.ColorDeltaGraphEffectName,
                   Properties.Resources.MaskEffect,
                   null,
                   EffectFlags.None)
        {
            PixelOp = new ColorDeltaOp();
        }

        [Serializable]
        public class ColorDeltaOp
            : UnaryPixelOp
        {
            public ColorBgra SelectedColor { get; set; }

            public override ColorBgra Apply(ColorBgra color)
            {
                byte delta = color.MaxDeviance(SelectedColor);
                return ColorBgra.FromBgr(delta, delta, delta);
            }
        }
    }
}

