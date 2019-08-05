using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cyberjax
{
    public partial class DistortionConfigPanel : UserControl
    {
        public event EventHandler PropertyChanged;

        public double ValueX
        {
            get => PanControlHost.ValueX;
            set => PanControlHost.ValueX = value;
        }
        public double ValueY
        {
            get => PanControlHost.ValueY;
            set => PanControlHost.ValueY = value;
        }
        public double ValueR
        {
            get => PanControlHost.ValueR;
            set => PanControlHost.ValueR = value;
        }

        public PanControlSettings PanControlSettings
        {
            set
            {
                PanControlHost.ApplySettings(value);
            }
        }

        public DistortionConfigPanel()
        {
            InitializeComponent();

            XyCurveControlHost.PropertyChanged += XyCurveControlHost_PropertyChanged; ;
            PanControlHost.PropertyChanged += PanControlHost_PropertyChanged;
        }

        public void InitTokenFromControl(DistortionEffectConfigToken token)
        {
            XyCurveControlHost.InitTokenFromControl(token);
            PanControlHost.InitTokenFromControl(token);
        }

        public void InitControlFromToken(DistortionEffectConfigToken token)
        {
            XyCurveControlHost.InitControlFromToken(token);
            PanControlHost.InitControlFromToken(token);
        }

        public void UpdatePanControls()
        {
            PanControlHost.UpdatePanControls();
        }


        private void PanControlHost_PropertyChanged(object sender, EventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }

        private void XyCurveControlHost_PropertyChanged(object sender, EventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }

    }
}
