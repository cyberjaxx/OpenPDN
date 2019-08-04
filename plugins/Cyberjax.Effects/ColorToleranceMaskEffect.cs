using System;
using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.PropertySystem;
using System.Drawing;

namespace Cyberjax
{
    [EffectCategory(EffectCategory.Adjustment)]
    public sealed class ColorToleranceMaskEffect
        : PropertyBasedEffect
    {
        private ColorToleranceMaskOp PixelOp { get; set; }

        public ColorBgra FilterColor => EnvironmentParameters.SecondaryColor;
        public byte Tolerance => 128; // Utility.ClampToByte(EnvironmentParameters.Tolerance * 256);
        public ColorBgra OpaqueColor { get; } = ColorBgra.Black;
        public ColorBgra TransparentColor { get; } = ColorBgra.White;

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            return PropertyCollection.CreateEmpty();
        }

        protected override void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            PixelOp.FilterColor = FilterColor;
            PixelOp.Tolerance = Tolerance;
            PixelOp.OpaqueColor = OpaqueColor;
            PixelOp.TransparentColor = TransparentColor;
            PixelOp.Apply(DstArgs.Surface, SrcArgs.Surface, rois, startIndex, length);
        }

        public ColorToleranceMaskEffect()
            : base(Properties.Resources.ColorToleranceEffectName,
                   Properties.Resources.MaskEffect,
                   null,
                   EffectFlags.None)
        {
            PixelOp = new ColorToleranceMaskOp();
        }

        [Serializable]
        public class ColorToleranceMaskOp
            : UnaryPixelOp
        {
            public ColorBgra FilterColor { get; set; }
            public int Tolerance { get; set; }
            public ColorBgra OpaqueColor { get; set; }
            public ColorBgra TransparentColor { get; set; }

            public override ColorBgra Apply(ColorBgra color)
            {
                return Utility.ColorDifference(color, FilterColor) < Tolerance ? TransparentColor : OpaqueColor;
            }
        }
    }
}
