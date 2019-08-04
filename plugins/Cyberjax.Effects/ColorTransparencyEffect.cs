using System;
using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.PropertySystem;
using System.Drawing;

namespace Cyberjax
{
    [EffectCategory(EffectCategory.Adjustment)]
    public sealed class ColorTransparencyEffect
        : PropertyBasedEffect
    {
        private Color2AlphaOp PixelOp { get; set; }

        public ColorBgra ForeColor => EnvironmentParameters.PrimaryColor;
        public ColorBgra BackColor => EnvironmentParameters.SecondaryColor;

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            return PropertyCollection.CreateEmpty();
        }

        protected override void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            PixelOp.ForeAverage = ForeColor.GetAverageColorChannelValueF();
            PixelOp.BackAverage = BackColor.GetAverageColorChannelValueF();
            PixelOp.Apply(DstArgs.Surface, SrcArgs.Surface, rois, startIndex, length);
        }

        public ColorTransparencyEffect()
            : base(Properties.Resources.ColorTransparencyEffectName,
                   Properties.Resources.MaskEffect3,
                   null,
                   EffectFlags.None)
        {
            PixelOp = new Color2AlphaOp();
        }

        [Serializable]
        public class Color2AlphaOp
            : UnaryPixelOp
        {
            public float ForeAverage { get; set; }
            public float BackAverage { get; set; }

            public override ColorBgra Apply(ColorBgra color)
            {
                float colorAverage = color.GetAverageColorChannelValueF();
                float scale = (colorAverage - BackAverage) / (ForeAverage - BackAverage);
                byte alpha = Utility.ClampToByte(scale * 255);
                return color.NewAlpha(alpha);
            }
        }
    }
}
