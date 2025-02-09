/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet.Tools
{
    internal class PanTool
        : Tool
    {
        private bool tracking = false;
        private Point lastMouseXY;
        private Cursor cursorMouseDown;
        private Cursor cursorMouseUp;
        private Cursor cursorMouseInvalid;
        private int ignoreMouseMove = 0;

        private bool CanPan()
        {
            return (DocumentWorkspace.VisibleDocumentRectangleF.Size != Document.Size);
        }

        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseDown(e);

            lastMouseXY = new Point(e.X, e.Y);
            tracking = true;

            Cursor = CanPan() ? cursorMouseUp : cursorMouseInvalid;
        }

        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseUp(e);

            Cursor = CanPan() ? cursorMouseUp : cursorMouseInvalid;

            tracking = false;
        }

        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (this.ignoreMouseMove > 0)
            {
                --this.ignoreMouseMove;
            }
            else if (tracking)
            {
                Point mouseXY = new Point(e.X, e.Y);
                Size delta = new Size(mouseXY.X - lastMouseXY.X, mouseXY.Y - lastMouseXY.Y);

                if (delta.Width != 0 || delta.Height != 0)
                {
                    PointF scrollPos = DocumentWorkspace.DocumentScrollPositionF;
                    PointF newScrollPos = new PointF(scrollPos.X - delta.Width, scrollPos.Y - delta.Height);
                    ++this.ignoreMouseMove; // setting DocumentScrollPosition incurs a MouseMove event
                    DocumentWorkspace.DocumentScrollPositionF = newScrollPos;

                    lastMouseXY = mouseXY;
                    lastMouseXY.X -= delta.Width;
                    lastMouseXY.Y -= delta.Height;
                }
            }
            else
            {
                Cursor = CanPan() ? cursorMouseUp : cursorMouseInvalid;
            }
        }

        protected override void OnActivate()
        {
            // cursor-action assignments
            this.cursorMouseDown = new Cursor(PdnResources.GetResourceStream("Cursors.PanToolCursorMouseDown.cur"));
            this.cursorMouseUp = new Cursor(PdnResources.GetResourceStream("Cursors.PanToolCursor.cur"));
            this.cursorMouseInvalid = new Cursor(PdnResources.GetResourceStream("Cursors.PanToolCursorInvalid.cur"));
            this.Cursor = cursorMouseUp;
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            cursorMouseDown?.Dispose();
            cursorMouseDown = null;

            cursorMouseUp?.Dispose();
            cursorMouseUp = null;

            cursorMouseInvalid?.Dispose();
            cursorMouseInvalid = null;
            
            base.OnDeactivate();
        }

        public PanTool(DocumentWorkspace documentWorkspace)
            : base(documentWorkspace,
                   PdnResources.GetImageResource("Icons.PanToolIcon.png"),
                   PdnResources.GetString("PanTool.Name"),
                   PdnResources.GetString("PanTool.HelpText"), 
                   'h',
                   false,
                   ToolBarConfigItems.None)
        {
            autoScroll = false;
            tracking = false;
        }
    }
}
