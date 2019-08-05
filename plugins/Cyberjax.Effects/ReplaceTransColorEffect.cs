using System;
using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.PropertySystem;
using System.Drawing;

namespace Cyberjax
{
    [EffectCategory(EffectCategory.Effect)]
    public sealed class ReplaceTransColorEffect
        : PropertyBasedEffect
    {
        private ReplaceTransColorOp PixelOp { get; set; }

        public ColorBgra ForeColor => EnvironmentParameters.PrimaryColor;
        public ColorBgra BackColor => EnvironmentParameters.SecondaryColor;

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            return PropertyCollection.CreateEmpty();
        }

        protected override void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            PixelOp.Color = ForeColor;
            PixelOp.Apply(DstArgs.Surface, SrcArgs.Surface, rois, startIndex, length);
        }

        public ReplaceTransColorEffect()
            : base(Properties.Resources.ReplaceTransColorEffectName,
                   Properties.Resources.ReplaceTransColorEffect,
                   "Cyberjax",
                   EffectFlags.None)
        {
            PixelOp = new ReplaceTransColorOp();
        }

        [Serializable]
        public class ReplaceTransColorOp
            : UnaryPixelOp
        {
            public ColorBgra Color { get; set; }

            public override ColorBgra Apply(ColorBgra color)
            {
                ColorBgra newColor = Color;
                return newColor.NewAlpha(color.A);
            }
        }
    }
}
