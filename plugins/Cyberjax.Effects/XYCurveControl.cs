using PaintDotNet;

namespace Cyberjax
{
    /// <summary>
    /// Curve control specialized for XY curves
    /// </summary>
    public sealed class XYCurveControl
        : BezierCurveControl
    {
        public XYCurveControl()
            : base(2, 501)
        {
            Mask = new bool[2] { true, true };
            VisualColors = new ColorBgra[]
            {
                ColorBgra.Red,
                ColorBgra.Green,
            };
            ChannelNames = new string[]
            {
                "X",
                "Y"
            };
            ResetControlPoints();
        }
    }
}

