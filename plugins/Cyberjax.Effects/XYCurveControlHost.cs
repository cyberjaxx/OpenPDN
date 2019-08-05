using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using PaintDotNet;
using PaintDotNet.Core;
using PaintDotNet.Effects;

namespace Cyberjax
{
    public partial class XYCurveControlHost : UserControl
    {
        public event EventHandler PropertyChanged;

        public XYCurveControl CurveControl { get => xyCurveControl; }

        public XYCurveControlHost()
        {
            InitializeComponent();
            xyCurveControl.ValueChanged += curveControl_ValueChanged;
            xyCurveControl.CoordinatesChanged += curveControl_CoordinatesChanged;
        }

        public void InitTokenFromControl(EffectConfigToken effectToken)
        {
            ((DistortionEffectConfigToken)effectToken).ControlPoints.List = (ControlPointList[])xyCurveControl.ControlPoints.Clone();
        }

        public void InitControlFromToken(EffectConfigToken effectToken)
        {
            DistortionEffectConfigToken token = (DistortionEffectConfigToken)effectToken;
            xyCurveControl.ControlPoints = (ControlPointList[])token.ControlPoints.List.Clone();
            xyCurveControl.Invalidate();
            xyCurveControl.Update();
        }

        private void curveControl_ValueChanged(object sender, EventArgs e)
        {
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }

        private void curveControl_CoordinatesChanged(object sender, EventArgs<Point> e)
        {
            Point pt = e.Data;
            string newText;

            if (pt.X >= 0)
            {
                float scale = 1.0f / (xyCurveControl.Entries - 1);
                newText = string.Format("({0:N3}, {1:N3})", pt.X * scale, pt.Y * scale);
            }
            else
            {
                newText = string.Empty;
            }

            if (newText != labelCoordinates.Text)
            {
                labelCoordinates.Text = newText;
                labelCoordinates.Update();
            }
        }

        private void checkBoxX_CheckedChanged(object sender, EventArgs e)
        {
            xyCurveControl.SetSelected(0, checkBoxX.Checked);
        }

        private void checkBoxY_CheckedChanged(object sender, EventArgs e)
        {
            xyCurveControl.SetSelected(1, checkBoxY.Checked);
        }

        private void resetButtonMap_Click(object sender, EventArgs e)
        {
            xyCurveControl.ResetControlPoints();
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }


    }
}
