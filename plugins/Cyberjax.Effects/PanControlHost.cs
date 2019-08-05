using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using PaintDotNet;
using PaintDotNet.Effects;

namespace Cyberjax
{
    public struct PanControlSettings
    {
        public double MinValueX;
        public double MaxValueX;
        public double MinValueY;
        public double MaxValueY;
        public double MinValueR;
        public double MaxValueR;
        public ImageResource StaticImageUnderlay;
    }

    public partial class PanControlHost : UserControl
    {
        public event EventHandler PropertyChanged;

        public double ValueX { get; set; }
        public double ValueY { get; set; }
        public double ValueR { get; set; }

        [Browsable(false)]
        public double MinValueX
        {
            get
            {
                return (double)pdnNumericUpDownX.Minimum;
            }
            set
            {
                pdnNumericUpDownX.Minimum = (decimal)value;
            }
        }

        [Browsable(false)]
        public double MaxValueX
        {
            get
            {
                return (double)pdnNumericUpDownX.Maximum;
            }
            set
            {
                pdnNumericUpDownX.Maximum = (decimal)value;
            }
        }

        [Browsable(false)]
        public double MinValueY
        {
            get
            {
                return (double)pdnNumericUpDownY.Minimum;
            }
            set
            {
                pdnNumericUpDownY.Minimum = (decimal)value;
            }
        }

        [Browsable(false)]
        public double MaxValueY
        {
            get
            {
                return (double)pdnNumericUpDownY.Maximum;
            }
            set
            {
                pdnNumericUpDownY.Maximum = (decimal)value;
            }
        }

        [Browsable(false)]
        public double MinValueR
        {
            get
            {
                return (double)pdnNumericUpDownR.Minimum;
            }
            set
            {
                pdnNumericUpDownR.Minimum = (decimal)value;
            }
        }

        [Browsable(false)]
        public double MaxValueR
        {
            get
            {
                return (double)pdnNumericUpDownR.Maximum;
            }
            set
            {
                pdnNumericUpDownR.Maximum = (decimal)value;
            }
        }

        [Browsable(false)]
        public ImageResource StaticImageUnderlay
        {
            get
            {
                return panControl.StaticImageUnderlay;
            }

            set
            {
                panControl.StaticImageUnderlay = value;

                panControl.BorderStyle = value == null ? BorderStyle.FixedSingle : BorderStyle.None;
            }
        }

        public PanControlHost()
        {
            InitializeComponent();
        }

        public void ApplySettings(PanControlSettings settings)
        {
            MinValueX = settings.MinValueX;
            MaxValueX = settings.MaxValueX;
            MinValueY = settings.MinValueY;
            MaxValueY = settings.MaxValueY;
            MinValueR = settings.MinValueR;
            MaxValueR = settings.MaxValueR;
            StaticImageUnderlay = settings.StaticImageUnderlay;
        }

        public void InitTokenFromControl(EffectConfigToken effectToken)
        {
            ((DistortionEffectConfigToken)effectToken).ValueX = ValueX;
            ((DistortionEffectConfigToken)effectToken).ValueY = ValueY;
            ((DistortionEffectConfigToken)effectToken).ValueR = ValueR;
        }

        public void InitControlFromToken(EffectConfigToken effectToken)
        {
            DistortionEffectConfigToken token = (DistortionEffectConfigToken)effectToken;
            ValueX = token.ValueX;
            ValueY = token.ValueY;
            ValueR = token.ValueR;
            UpdatePanControls();
        }

        public void UpdatePanControls()
        {
            PointF newPos = new PointF((float)ValueX, (float)ValueY);
            panControl.Position = newPos;

            pdnNumericUpDownX.Value = (decimal)ValueX;
            pdnNumericUpDownY.Value = (decimal)ValueY;
            pdnNumericUpDownR.Value = (decimal)ValueR;
        }

        public void OnValueChanged()
        {
            UpdatePanControls();
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }

        private void panControl_PositionChanged(object sender, EventArgs e)
        {
            PointF pos = panControl.Position;

            PointF clampedPos = new PointF(
                Utility.Clamp(pos.X, (float)MinValueX, (float)MaxValueX),
                Utility.Clamp(pos.Y, (float)MinValueY, (float)MaxValueY));

            panControl.Position = clampedPos;

            ValueX = clampedPos.X;
            ValueY = clampedPos.Y;

            OnValueChanged();
        }

        private void pdnNumericUpDownX_ValueChanged(object sender, EventArgs e)
        {
            ValueX = (double)pdnNumericUpDownX.Value;

            OnValueChanged();
        }

        private void pdnNumericUpDownY_ValueChanged(object sender, EventArgs e)
        {
            ValueY = (double)pdnNumericUpDownY.Value;

            OnValueChanged();
        }

        private void pdnNumericUpDownR_ValueChanged(object sender, EventArgs e)
        {
            ValueR = (double)pdnNumericUpDownR.Value;

            OnValueChanged();
        }

        private void resetButtonX_Click(object sender, EventArgs e)
        {
            ValueX = 0.0;
            OnValueChanged();
        }

        private void resetButtonY_Click(object sender, EventArgs e)
        {
            ValueY = 0.0;
            OnValueChanged();
        }

        private void resetButtonR_Click(object sender, EventArgs e)
        {
            ValueR = 1.0;
            OnValueChanged();
        }
    }
}
