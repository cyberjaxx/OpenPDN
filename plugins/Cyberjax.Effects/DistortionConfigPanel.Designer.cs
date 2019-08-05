namespace Cyberjax
{
    partial class DistortionConfigPanel
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.XyCurveControlHost = new Cyberjax.XYCurveControlHost();
            this.PanControlHost = new Cyberjax.PanControlHost();
            this.SuspendLayout();
            // 
            // XyCurveControlHost
            // 
            this.XyCurveControlHost.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.XyCurveControlHost.Location = new System.Drawing.Point(9, 1);
            this.XyCurveControlHost.Margin = new System.Windows.Forms.Padding(9, 0, 9, 0);
            this.XyCurveControlHost.Name = "XyCurveControlHost";
            this.XyCurveControlHost.Size = new System.Drawing.Size(253, 335);
            this.XyCurveControlHost.TabIndex = 3;
            // 
            // PanControlHost
            // 
            this.PanControlHost.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.PanControlHost.Location = new System.Drawing.Point(9, 342);
            this.PanControlHost.Margin = new System.Windows.Forms.Padding(9, 0, 9, 0);
            this.PanControlHost.MaxValueR = 100D;
            this.PanControlHost.MaxValueX = 100D;
            this.PanControlHost.MaxValueY = 100D;
            this.PanControlHost.MinValueR = 0D;
            this.PanControlHost.MinValueX = 0D;
            this.PanControlHost.MinValueY = 0D;
            this.PanControlHost.Name = "PanControlHost";
            this.PanControlHost.Size = new System.Drawing.Size(253, 182);
            this.PanControlHost.StaticImageUnderlay = null;
            this.PanControlHost.TabIndex = 2;
            this.PanControlHost.ValueR = 0D;
            this.PanControlHost.ValueX = 0D;
            this.PanControlHost.ValueY = 0D;
            // 
            // DistortionEffectPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.XyCurveControlHost);
            this.Controls.Add(this.PanControlHost);
            this.Name = "DistortionEffectPanel";
            this.Size = new System.Drawing.Size(271, 525);
            this.ResumeLayout(false);

        }

        #endregion

        private XYCurveControlHost XyCurveControlHost;
        private PanControlHost PanControlHost;
    }
}
