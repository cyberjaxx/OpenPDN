/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet
{
    public class PanelEx : 
        PaintDotNet.SystemLayer.ScrollPanel
    {
        public bool HideHScroll { get; set; } = false;

        protected override void OnSizeChanged(EventArgs e)
        {
            if (this.HideHScroll)
            {
                SystemLayer.UI.SuspendControlPainting(this);
            }

            base.OnSizeChanged(e);

            if (this.HideHScroll)
            {
                SystemLayer.UI.HideHorizontalScrollBar(this);
                SystemLayer.UI.ResumeControlPainting(this);
                Invalidate(true);
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            //base.OnMouseWheel(e);
        }
    }
}
