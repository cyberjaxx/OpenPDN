/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace PaintDotNet
{
    internal class ToolsForm 
        : FloatingToolForm
    {
        public ToolsControl ToolsControl { get; private set; } = null;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.ClientSize = new Size(ToolsControl.Width, ToolsControl.Height);
        }

        public ToolsForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
        
            this.Text = PdnResources.GetString("MainToolBarForm.Text");
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ToolsControl = new PaintDotNet.ToolsControl();
            this.SuspendLayout();
            // 
            // toolsControl
            // 
            this.ToolsControl.Location = new System.Drawing.Point(0, 0);
            this.ToolsControl.Name = "toolsControl";
            this.ToolsControl.Size = new System.Drawing.Size(50, 88);
            this.ToolsControl.TabIndex = 0;
            this.ToolsControl.RelinquishFocus += new EventHandler(ToolsControl_RelinquishFocus);
            // 
            // MainToolBarForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(50, 273);
            this.Controls.Add(this.ToolsControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ToolsForm";
            this.Controls.SetChildIndex(this.ToolsControl, 0);
            this.ResumeLayout(false);
        }
        #endregion

        private void ToolsControl_RelinquishFocus(object sender, EventArgs e)
        {
            OnRelinquishFocus();
        }
    }
}
