namespace Cyberjax
{
    partial class XYCurveControlHost
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
            this.distortionHeader = new PaintDotNet.HeaderLabel();
            this.labelCoordinates = new System.Windows.Forms.Label();
            this.resetButtonMap = new System.Windows.Forms.Button();
            this.checkBoxY = new System.Windows.Forms.CheckBox();
            this.checkBoxX = new System.Windows.Forms.CheckBox();
            this.labelHelpText = new System.Windows.Forms.Label();
            this.xyCurveControl = new Cyberjax.XYCurveControl();
            this.SuspendLayout();
            // 
            // distortionHeader
            // 
            this.distortionHeader.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.distortionHeader.ForeColor = System.Drawing.SystemColors.Highlight;
            this.distortionHeader.Location = new System.Drawing.Point(1, 3);
            this.distortionHeader.Margin = new System.Windows.Forms.Padding(1, 3, 1, 1);
            this.distortionHeader.Name = "distortionHeader";
            this.distortionHeader.RightMargin = 0;
            this.distortionHeader.Size = new System.Drawing.Size(253, 17);
            this.distortionHeader.TabIndex = 21;
            this.distortionHeader.TabStop = false;
            this.distortionHeader.Text = "Distortion Map";
            // 
            // labelCoordinates
            // 
            this.labelCoordinates.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelCoordinates.Location = new System.Drawing.Point(172, 21);
            this.labelCoordinates.Name = "labelCoordinates";
            this.labelCoordinates.Size = new System.Drawing.Size(78, 13);
            this.labelCoordinates.TabIndex = 26;
            this.labelCoordinates.Text = "(0.5, 0.5)";
            this.labelCoordinates.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // resetButtonMap
            // 
            this.resetButtonMap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.resetButtonMap.Image = global::Cyberjax.Properties.Resources.ResetIcon;
            this.resetButtonMap.Location = new System.Drawing.Point(91, 298);
            this.resetButtonMap.Name = "resetButtonMap";
            this.resetButtonMap.Size = new System.Drawing.Size(20, 24);
            this.resetButtonMap.TabIndex = 34;
            this.resetButtonMap.UseVisualStyleBackColor = true;
            this.resetButtonMap.Click += new System.EventHandler(this.resetButtonMap_Click);
            // 
            // checkBoxY
            // 
            this.checkBoxY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxY.AutoSize = true;
            this.checkBoxY.Checked = true;
            this.checkBoxY.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxY.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxY.Location = new System.Drawing.Point(46, 302);
            this.checkBoxY.Name = "checkBoxY";
            this.checkBoxY.Size = new System.Drawing.Size(39, 18);
            this.checkBoxY.TabIndex = 33;
            this.checkBoxY.Text = "Y";
            this.checkBoxY.UseVisualStyleBackColor = true;
            this.checkBoxY.CheckedChanged += new System.EventHandler(this.checkBoxY_CheckedChanged);
            // 
            // checkBoxX
            // 
            this.checkBoxX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxX.AutoSize = true;
            this.checkBoxX.Checked = true;
            this.checkBoxX.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxX.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxX.Location = new System.Drawing.Point(1, 302);
            this.checkBoxX.Name = "checkBoxX";
            this.checkBoxX.Size = new System.Drawing.Size(39, 18);
            this.checkBoxX.TabIndex = 32;
            this.checkBoxX.Text = "X";
            this.checkBoxX.UseVisualStyleBackColor = true;
            this.checkBoxX.CheckedChanged += new System.EventHandler(this.checkBoxX_CheckedChanged);
            // 
            // labelHelpText
            // 
            this.labelHelpText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelHelpText.AutoSize = true;
            this.labelHelpText.Location = new System.Drawing.Point(3, 321);
            this.labelHelpText.Name = "labelHelpText";
            this.labelHelpText.Size = new System.Drawing.Size(203, 13);
            this.labelHelpText.TabIndex = 31;
            this.labelHelpText.Text = "Tip: Right - click to remove control points.";
            // 
            // xyCurveControl
            // 
            this.xyCurveControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xyCurveControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.xyCurveControl.Location = new System.Drawing.Point(0, 42);
            this.xyCurveControl.Name = "xyCurveControl";
            this.xyCurveControl.Size = new System.Drawing.Size(253, 253);
            this.xyCurveControl.TabIndex = 0;
            this.xyCurveControl.TabStop = false;
            // 
            // XYCurveControlHost
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.resetButtonMap);
            this.Controls.Add(this.checkBoxY);
            this.Controls.Add(this.checkBoxX);
            this.Controls.Add(this.labelHelpText);
            this.Controls.Add(this.labelCoordinates);
            this.Controls.Add(this.distortionHeader);
            this.Controls.Add(this.xyCurveControl);
            this.Name = "XYCurveControlHost";
            this.Size = new System.Drawing.Size(253, 335);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Cyberjax.XYCurveControl xyCurveControl;
        private PaintDotNet.HeaderLabel distortionHeader;
        private System.Windows.Forms.Label labelCoordinates;
        private System.Windows.Forms.Button resetButtonMap;
        private System.Windows.Forms.CheckBox checkBoxY;
        private System.Windows.Forms.CheckBox checkBoxX;
        private System.Windows.Forms.Label labelHelpText;
    }
}
