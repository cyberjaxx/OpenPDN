using System;
using System.Windows.Forms;
using PaintDotNet.Effects;

namespace Cyberjax
{
    public partial class DistortionEffectConfigDialog
        : EffectConfigDialog
    {
        private bool SuppressEvents { get; set; } = false;

        public PanControlSettings PanControlSettings
        {
            set
            {
                DistortionConfigPanel.PanControlSettings = value;
            }
        }

        public DistortionEffectConfigDialog()
        {
            InitializeComponent();

            DistortionConfigPanel.PropertyChanged += OnPropertyChanged;
        }

        protected override void InitialInitToken()
        {
            DistortionEffectConfigToken token = new DistortionEffectConfigToken();
            theEffectToken = token;
        }

        protected override void InitTokenFromDialog()
        {
            DistortionConfigPanel.InitTokenFromControl((DistortionEffectConfigToken)EffectToken);
        }

        protected override void InitDialogFromToken(EffectConfigToken effectToken)
        {
            SuppressEvents = true;

            DistortionConfigPanel.InitControlFromToken((DistortionEffectConfigToken)EffectToken);

            SuppressEvents = false;
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
            if (!SuppressEvents)
            {
                FinishTokenUpdate();
            }
        }
    }
}
