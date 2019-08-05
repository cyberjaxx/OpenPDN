using System;
using System.Windows.Forms;
using PaintDotNet.Effects;

namespace Cyberjax
{
    public partial class DualDistortionEffectConfigDialog 
        : EffectConfigDialog
    {
        private DistortionConfigPanel[] DistortionConfigPanels { get; }

        private int SuppressEvents { get; set; } = 0;

        public PanControlSettings PanControlSettings
        {
            set
            {
                foreach (DistortionConfigPanel panel in DistortionConfigPanels)
                {
                    panel.PanControlSettings = value;
                }
            }
        }

        public DualDistortionEffectConfigDialog()
        {
            InitializeComponent();

            DistortionConfigPanels = new DistortionConfigPanel[]
            {
                DistortionConfigPanel1,
                DistortionConfigPanel2
            };

            foreach (DistortionConfigPanel panel in DistortionConfigPanels)
            {
                panel.PropertyChanged += OnPropertyChanged;
            }
        }

        protected override void InitialInitToken()
        {
            DistortionEffectConfigTokens token = new DistortionEffectConfigTokens(2);
            foreach (DistortionEffectConfigToken subToken in token.SubTokens)
            {
                subToken.ValueX = -0.5;
                subToken.ValueY = -0.5;
            }
            theEffectToken = token;
        }

        protected override void InitTokenFromDialog()
        {
            for (int i = 0; i < DistortionConfigPanels.Length; ++i)
            {
                DistortionConfigPanels[i].InitTokenFromControl(((DistortionEffectConfigTokens)EffectToken).SubTokens[i]);
            }
        }

        protected override void InitDialogFromToken(EffectConfigToken effectToken)
        {
            ++SuppressEvents;

            for (int i = 0; i < DistortionConfigPanels.Length; ++i)
            {
                DistortionConfigPanels[i].InitControlFromToken(((DistortionEffectConfigTokens)EffectToken).SubTokens[i]);
            }

            UpdateRadius();

            --SuppressEvents;
        }

        private void UpdateRadius()
        {
            double u = DistortionConfigPanels[1].ValueX - DistortionConfigPanels[0].ValueX;
            double v = DistortionConfigPanels[1].ValueY - DistortionConfigPanels[0].ValueY;
            double r = Math.Sqrt(u * u + v * v);
            double r0 = DistortionConfigPanels[0].ValueR;
            double r1 = DistortionConfigPanels[1].ValueR;
            if (r0 + r1 > r)
            {
                double rdiv2 = r / 2;
                if (r0 > rdiv2)
                {
                    if (r1 > rdiv2)
                    {
                        DistortionConfigPanels[0].ValueR = rdiv2;
                        DistortionConfigPanels[1].ValueR = rdiv2;
                    }
                    else
                    {
                        DistortionConfigPanels[0].ValueR = r - r1;
                    }
                }
                else if (r1 > rdiv2)
                {
                    DistortionConfigPanels[1].ValueR = r - r0;

                }
                DistortionConfigPanels[0].UpdatePanControls();
                DistortionConfigPanels[1].UpdatePanControls();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            okButton.Select();
            base.OnLoad(e);
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void OnPropertyChanged(object sender, EventArgs e)
        {
            if (SuppressEvents == 0)
            {
                if (sender is PanControlHost)
                {
                    UpdateRadius();
                }

                FinishTokenUpdate();
            }
        }
    }
}
