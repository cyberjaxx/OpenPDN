namespace Cyberjax
{
    partial class PanControlHost
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
            this.resetButtonX = new System.Windows.Forms.Button();
            this.resetButtonY = new System.Windows.Forms.Button();
            this.resetButtonR = new System.Windows.Forms.Button();
            this.pdnNumericUpDownY = new PaintDotNet.PdnNumericUpDown();
            this.pdnNumericUpDownX = new PaintDotNet.PdnNumericUpDown();
            this.panControl = new PaintDotNet.Core.PanControl();
            this.offsetHeader = new PaintDotNet.HeaderLabel();
            this.pdnNumericUpDownR = new PaintDotNet.PdnNumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.pdnNumericUpDownY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pdnNumericUpDownX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pdnNumericUpDownR)).BeginInit();
            this.SuspendLayout();
            // 
            // resetButtonX
            // 
            this.resetButtonX.Image = global::Cyberjax.Properties.Resources.ResetIcon;
            this.resetButtonX.Location = new System.Drawing.Point(232, 21);
            this.resetButtonX.Name = "resetButtonX";
            this.resetButtonX.Size = new System.Drawing.Size(20, 20);
            this.resetButtonX.TabIndex = 41;
            this.resetButtonX.UseVisualStyleBackColor = true;
            this.resetButtonX.Click += new System.EventHandler(this.resetButtonX_Click);
            // 
            // resetButtonY
            // 
            this.resetButtonY.Image = global::Cyberjax.Properties.Resources.ResetIcon;
            this.resetButtonY.Location = new System.Drawing.Point(232, 47);
            this.resetButtonY.Name = "resetButtonY";
            this.resetButtonY.Size = new System.Drawing.Size(20, 20);
            this.resetButtonY.TabIndex = 42;
            this.resetButtonY.UseVisualStyleBackColor = true;
            this.resetButtonY.Click += new System.EventHandler(this.resetButtonY_Click);
            // 
            // resetButtonR
            // 
            this.resetButtonR.Image = global::Cyberjax.Properties.Resources.ResetIcon;
            this.resetButtonR.Location = new System.Drawing.Point(232, 73);
            this.resetButtonR.Name = "resetButtonR";
            this.resetButtonR.Size = new System.Drawing.Size(20, 20);
            this.resetButtonR.TabIndex = 44;
            this.resetButtonR.UseVisualStyleBackColor = true;
            this.resetButtonR.Click += new System.EventHandler(this.resetButtonR_Click);
            // 
            // pdnNumericUpDownY
            // 
            this.pdnNumericUpDownY.DecimalPlaces = 3;
            this.pdnNumericUpDownY.Increment = new decimal(new int[] {
            5,
            0,
            0,
            196608});
            this.pdnNumericUpDownY.Location = new System.Drawing.Point(166, 47);
            this.pdnNumericUpDownY.Name = "pdnNumericUpDownY";
            this.pdnNumericUpDownY.Size = new System.Drawing.Size(60, 20);
            this.pdnNumericUpDownY.TabIndex = 40;
            this.pdnNumericUpDownY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.pdnNumericUpDownY.ValueChanged += new System.EventHandler(this.pdnNumericUpDownY_ValueChanged);
            // 
            // pdnNumericUpDownX
            // 
            this.pdnNumericUpDownX.DecimalPlaces = 3;
            this.pdnNumericUpDownX.Increment = new decimal(new int[] {
            5,
            0,
            0,
            196608});
            this.pdnNumericUpDownX.Location = new System.Drawing.Point(166, 21);
            this.pdnNumericUpDownX.Name = "pdnNumericUpDownX";
            this.pdnNumericUpDownX.Size = new System.Drawing.Size(60, 20);
            this.pdnNumericUpDownX.TabIndex = 39;
            this.pdnNumericUpDownX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.pdnNumericUpDownX.ValueChanged += new System.EventHandler(this.pdnNumericUpDownX_ValueChanged);
            // 
            // panControl
            // 
            this.panControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panControl.Location = new System.Drawing.Point(0, 22);
            this.panControl.Name = "panControl";
            this.panControl.Size = new System.Drawing.Size(160, 160);
            this.panControl.TabIndex = 38;
            this.panControl.TabStop = false;
            this.panControl.PositionChanged += new System.EventHandler(this.panControl_PositionChanged);
            // 
            // offsetHeader
            // 
            this.offsetHeader.ForeColor = System.Drawing.SystemColors.Highlight;
            this.offsetHeader.Location = new System.Drawing.Point(0, 0);
            this.offsetHeader.Margin = new System.Windows.Forms.Padding(1, 3, 1, 1);
            this.offsetHeader.Name = "offsetHeader";
            this.offsetHeader.RightMargin = 0;
            this.offsetHeader.Size = new System.Drawing.Size(257, 17);
            this.offsetHeader.TabIndex = 37;
            this.offsetHeader.TabStop = false;
            this.offsetHeader.Text = "Offset";
            // 
            // pdnNumericUpDownR
            // 
            this.pdnNumericUpDownR.DecimalPlaces = 3;
            this.pdnNumericUpDownR.Increment = new decimal(new int[] {
            5,
            0,
            0,
            196608});
            this.pdnNumericUpDownR.Location = new System.Drawing.Point(166, 73);
            this.pdnNumericUpDownR.Name = "pdnNumericUpDownR";
            this.pdnNumericUpDownR.Size = new System.Drawing.Size(60, 20);
            this.pdnNumericUpDownR.TabIndex = 43;
            this.pdnNumericUpDownR.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.pdnNumericUpDownR.ValueChanged += new System.EventHandler(this.pdnNumericUpDownR_ValueChanged);
            // 
            // PanControlHost
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.resetButtonR);
            this.Controls.Add(this.pdnNumericUpDownR);
            this.Controls.Add(this.resetButtonY);
            this.Controls.Add(this.resetButtonX);
            this.Controls.Add(this.pdnNumericUpDownY);
            this.Controls.Add(this.pdnNumericUpDownX);
            this.Controls.Add(this.panControl);
            this.Controls.Add(this.offsetHeader);
            this.Name = "PanControlHost";
            this.Size = new System.Drawing.Size(253, 182);
            ((System.ComponentModel.ISupportInitialize)(this.pdnNumericUpDownY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pdnNumericUpDownX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pdnNumericUpDownR)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button resetButtonX;
        private System.Windows.Forms.Button resetButtonY;
        private System.Windows.Forms.Button resetButtonR;
        private PaintDotNet.PdnNumericUpDown pdnNumericUpDownX;
        private PaintDotNet.PdnNumericUpDown pdnNumericUpDownY;
        private PaintDotNet.PdnNumericUpDown pdnNumericUpDownR;
        private PaintDotNet.Core.PanControl panControl;
        private PaintDotNet.HeaderLabel offsetHeader;

    }
}
