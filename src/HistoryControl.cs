/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.SystemLayer;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet
{
    internal sealed class HistoryControl 
        : Control
    {
        private enum ItemType
        {
            Undo,
            Redo
        }

        private VScrollBar VScrollBar { get; set; }
        private int ItemHeight { get; set; }
        private int UndoItemHighlight { get; set; } = -1;
        private int RedoItemHighlight { get; set; } = -1;
        private Point LastMouseClientPt { get; set; } = new Point(-1, -1);
        private int IgnoreScrollOffsetSet { get; set; } = 0;

        private void SuspendScrollOffsetSet()
        {
            ++IgnoreScrollOffsetSet;
        }

        private void ResumeScrollOffsetSet()
        {
            --IgnoreScrollOffsetSet;
        }

        public bool ManagedFocus { get; set; } = false;

        public event EventHandler RelinquishFocus;
        private void OnRelinquishFocus()
        {
            RelinquishFocus?.Invoke(this, EventArgs.Empty);
        }

        private int ItemCount
        {
            get => historyStack == null ? 0 :
                    historyStack.UndoStack.Count + historyStack.RedoStack.Count;
        }

        public event EventHandler ScrollOffsetChanged;
        private void OnScrollOffsetChanged()
        {
            VScrollBar.Value = Utility.Clamp(ScrollOffset, VScrollBar.Minimum, VScrollBar.Maximum);

            ScrollOffsetChanged?.Invoke(this, EventArgs.Empty);
        }

        public int MinScrollOffset
        {
            get => 0;
        }

        public int MaxScrollOffset
        {
            get => Math.Max(0, ViewHeight - ClientSize.Height);
        }

        private int scrollOffset = 0;
        public int ScrollOffset
        {
            get => scrollOffset;

            set
            {
                if (IgnoreScrollOffsetSet <= 0)
                {
                    int clampedOffset = Utility.Clamp(value, MinScrollOffset, MaxScrollOffset);

                    if (scrollOffset != clampedOffset)
                    {
                        scrollOffset = clampedOffset;
                        OnScrollOffsetChanged();
                        Invalidate(false);
                    }
                }
            }
        }

        public Rectangle ViewRectangle
        {
            get => new Rectangle(0, 0, ViewWidth, ViewHeight);
        }

        public Rectangle ClientRectangleToViewRectangle(Rectangle clientRect)
        {
            Point clientPt = ClientPointToViewPoint(clientRect.Location);
            return new Rectangle(clientPt, clientRect.Size);
        }

        public int ViewWidth
        {
            get => VScrollBar.Visible ? ClientSize.Width - VScrollBar.Width : ClientSize.Width;
        }

        private int ViewHeight
        {
            get => ItemCount * ItemHeight;
        }

        private void EnsureItemIsFullyVisible(ItemType itemType, int itemIndex)
        {
            Point itemPt = StackIndexToViewPoint(itemType, itemIndex);
            Rectangle itemRect = new Rectangle(itemPt, new Size(ViewWidth, ItemHeight));

            int minOffset = itemRect.Bottom - ClientSize.Height;
            int maxOffset = itemRect.Top;

            ScrollOffset = Utility.Clamp(ScrollOffset, minOffset, maxOffset);
        }

        private Point ClientPointToViewPoint(Point pt)
        {
            return new Point(pt.X, pt.Y + ScrollOffset);
        }

        private void ViewPointToStackIndex(Point viewPt, out ItemType itemType, out int itemIndex)
        {
            Rectangle undoRect = UndoViewRectangle;

            if (viewPt.Y >= undoRect.Top && viewPt.Y < undoRect.Bottom)
            {
                itemType = ItemType.Undo;
                itemIndex = (viewPt.Y - undoRect.Top) / ItemHeight;
            }
            else
            {
                Rectangle redoRect = RedoViewRectangle;

                itemType = ItemType.Redo;
                itemIndex = (viewPt.Y - redoRect.Top) / ItemHeight;
            }
        }

        private Point StackIndexToViewPoint(ItemType itemType, int itemIndex)
        {
            int y;
            Rectangle typeRect;

            if (itemType == ItemType.Undo)
            {
                typeRect = UndoViewRectangle;
            }
            else // if (itemTYpe == ItemType.Redo)
            {
                typeRect = RedoViewRectangle;
            }

            y = (itemIndex * ItemHeight) + typeRect.Top;
            return new Point(0, y);
        }

        public event EventHandler HistoryChanged;
        private void OnHistoryChanged()
        {
            VScrollBar.Maximum = ViewHeight;

            HistoryChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int totalItems = 0;

            if (HistoryStack != null)
            {
                totalItems = HistoryStack.UndoStack.Count + HistoryStack.RedoStack.Count;
            }

            int totalHeight = totalItems * ItemHeight;

            if (totalHeight > ClientSize.Height)
            {
                VScrollBar.Visible = true;
                VScrollBar.Location = new Point(ClientSize.Width - VScrollBar.Width, 0);
                VScrollBar.Height = ClientSize.Height;
                VScrollBar.Minimum = 0;
                VScrollBar.Maximum = totalHeight;
                VScrollBar.LargeChange = ClientSize.Height;
                VScrollBar.SmallChange = ItemHeight;
            }
            else
            {
                VScrollBar.Visible = false;
            }

            if (HistoryStack != null)
            {
                ScrollOffset = Utility.Clamp(ScrollOffset, MinScrollOffset, MaxScrollOffset);
            }

            base.OnLayout(levent);
        }

        private Rectangle UndoViewRectangle
        {
            get
            {
                return new Rectangle(0, 0, ViewWidth, ItemHeight * HistoryStack.UndoStack.Count);
            }
        }

        private Rectangle RedoViewRectangle
        {
            get
            {
                int undoRectBottom = ItemHeight * HistoryStack.UndoStack.Count;
                return new Rectangle(0, undoRectBottom, ViewWidth, ItemHeight * HistoryStack.RedoStack.Count);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (HistoryStack != null)
            {
                using (SolidBrush backBrush = new SolidBrush(BackColor))
                {
                    e.Graphics.FillRectangle(backBrush, e.ClipRectangle);
                }

                e.Graphics.TranslateTransform(0, -ScrollOffset);

                int afterImageHMargin = UI.ScaleWidth(1);

                StringFormat stringFormat = (StringFormat)StringFormat.GenericTypographic.Clone();
                stringFormat.LineAlignment = StringAlignment.Center;
                stringFormat.Trimming = StringTrimming.EllipsisCharacter;

                Rectangle visibleViewRectangle = ClientRectangleToViewRectangle(ClientRectangle);

                // Fill in the background for the undo items
                Rectangle undoRect = UndoViewRectangle;
                e.Graphics.FillRectangle(SystemBrushes.Window, undoRect);

                // We only want to draw what's visible, so figure out the first and last
                // undo items that are actually visible and only draw them.
                Rectangle visibleUndoRect = Rectangle.Intersect(visibleViewRectangle, undoRect);

                int beginUndoIndex;
                int endUndoIndex;
                if (visibleUndoRect.Width > 0 && visibleUndoRect.Height > 0)
                {
                    ViewPointToStackIndex(visibleUndoRect.Location, out ItemType itemType, out beginUndoIndex);
                    ViewPointToStackIndex(new Point(visibleUndoRect.Left, visibleUndoRect.Bottom - 1), out itemType, out endUndoIndex);
                }
                else
                {
                    beginUndoIndex = 0;
                    endUndoIndex = -1;
                }

                // Draw undo items
                for (int i = beginUndoIndex; i <= endUndoIndex; ++i)
                {
                    Image image;
                    ImageResource imageResource = HistoryStack.UndoStack[i].Image;

                    if (imageResource != null)
                    {
                        image = imageResource.Reference;
                    }
                    else
                    {
                        image = null;
                    }

                    int drawWidth;
                    if (image != null)
                    {
                        drawWidth = (image.Width * ItemHeight) / image.Height;
                    }
                    else
                    {
                        drawWidth = ItemHeight;
                    }

                    Brush textBrush;

                    if (i == UndoItemHighlight)
                    {
                        Rectangle itemRect = new Rectangle(
                            0,
                            i * ItemHeight,
                            ViewWidth,
                            ItemHeight);

                        e.Graphics.FillRectangle(SystemBrushes.Highlight, itemRect);
                        textBrush = SystemBrushes.HighlightText;
                    }
                    else
                    {
                        textBrush = SystemBrushes.WindowText;
                    }

                    if (image != null)
                    {
                        e.Graphics.DrawImage(
                            image,
                            new Rectangle(0, i * ItemHeight, drawWidth, ItemHeight),
                            new Rectangle(0, 0, image.Width, image.Height),
                            GraphicsUnit.Pixel);
                    }

                    int textX = drawWidth + afterImageHMargin;

                    Rectangle textRect = new Rectangle(
                        textX,
                        i * ItemHeight,
                        ViewWidth - textX,
                        ItemHeight);

                    e.Graphics.DrawString(
                        HistoryStack.UndoStack[i].Name, 
                        Font,
                        textBrush, 
                        textRect, 
                        stringFormat);
                }

                // Fill in the background for the redo items
                Rectangle redoRect = RedoViewRectangle;
                e.Graphics.FillRectangle(Brushes.SlateGray, redoRect);

                Font redoFont = new Font(Font, Font.Style | FontStyle.Italic);

                // We only want to draw what's visible, so figure out the first and last
                // redo items that are actually visible and only draw them.
                Rectangle visibleRedoRect = Rectangle.Intersect(visibleViewRectangle, redoRect);

                int beginRedoIndex;
                int endRedoIndex;
                if (visibleRedoRect.Width > 0 && visibleRedoRect.Height > 0)
                {
                    ViewPointToStackIndex(visibleRedoRect.Location, out ItemType itemType, out beginRedoIndex);
                    ViewPointToStackIndex(new Point(visibleRedoRect.Left, visibleRedoRect.Bottom - 1), out itemType, out endRedoIndex);
                }
                else
                {
                    beginRedoIndex = 0;
                    endRedoIndex = -1;
                } 

                // Draw redo items
                for (int i = beginRedoIndex; i <= endRedoIndex; ++i)
                {
                    Image image;
                    ImageResource imageResource = HistoryStack.RedoStack[i].Image;

                    if (imageResource != null)
                    {
                        image = imageResource.Reference;
                    }
                    else
                    {
                        image = null;
                    }

                    int drawWidth;

                    if (image != null)
                    {
                        drawWidth = (image.Width * ItemHeight) / image.Height;
                    }
                    else
                    {
                        drawWidth = ItemHeight;
                    }

                    int y = redoRect.Top + i * ItemHeight;

                    Brush textBrush;
                    if (i == RedoItemHighlight)
                    {
                        Rectangle itemRect = new Rectangle(
                            0,
                            y,
                            ViewWidth,
                            ItemHeight);

                        e.Graphics.FillRectangle(SystemBrushes.Highlight, itemRect);
                        textBrush = SystemBrushes.HighlightText;
                    }
                    else
                    {
                        textBrush = SystemBrushes.InactiveCaptionText;
                    }

                    if (image != null)
                    {
                        e.Graphics.DrawImage(
                            image,
                            new Rectangle(0, y, drawWidth, ItemHeight),
                            new Rectangle(0, 0, image.Width, image.Height),
                            GraphicsUnit.Pixel);
                    }

                    int textX = drawWidth + afterImageHMargin;

                    Rectangle textRect = new Rectangle(
                        textX,
                        y,
                        ViewWidth - textX,
                        ItemHeight);

                    e.Graphics.DrawString(
                        HistoryStack.RedoStack[i].Name,
                        redoFont,
                        textBrush,
                        textRect,
                        stringFormat);
                }

                redoFont.Dispose();
                redoFont = null;

                stringFormat.Dispose();
                stringFormat = null;

                e.Graphics.TranslateTransform(0, ScrollOffset);
            }

            base.OnPaint(e);
        }
        private HistoryStack historyStack = null;
        public HistoryStack HistoryStack
        {
            get => historyStack;

            set
            {
                if (historyStack != null)
                {
                    historyStack.Changed -= History_Changed;
                    historyStack.SteppedForward -= History_SteppedForward;
                    historyStack.SteppedBackward -= History_SteppedBackward;
                    historyStack.HistoryFlushed -= History_HistoryFlushed;
                    historyStack.NewHistoryMemento -= History_NewHistoryMemento;
                }

                historyStack = value;
                PerformLayout();

                if (HistoryStack != null)
                {
                    HistoryStack.Changed += History_Changed;
                    HistoryStack.SteppedForward += History_SteppedForward;
                    HistoryStack.SteppedBackward += History_SteppedBackward;
                    HistoryStack.HistoryFlushed += History_HistoryFlushed;
                    HistoryStack.NewHistoryMemento += History_NewHistoryMemento;
                    EnsureLastUndoItemIsFullyVisible();
                }

                Refresh();
                OnHistoryChanged();
            }
        }

        private void EnsureLastUndoItemIsFullyVisible()
        {
            int index = HistoryStack.UndoStack.Count - 1;
            EnsureItemIsFullyVisible(ItemType.Undo, index);
        }

        private void History_HistoryFlushed(object sender, EventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            EnsureLastUndoItemIsFullyVisible();
            PerformMouseMove();
            PerformLayout();
            Refresh();
        }

        private void History_SteppedForward(object sender, EventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            UndoItemHighlight = -1;
            RedoItemHighlight = -1;
            EnsureLastUndoItemIsFullyVisible();
            PerformMouseMove();
            PerformLayout();
            Refresh();
        }

        private void History_SteppedBackward(object sender, EventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            UndoItemHighlight = -1;
            RedoItemHighlight = -1;
            EnsureLastUndoItemIsFullyVisible();
            PerformMouseMove();
            PerformLayout();
            Refresh();
        }

        private void History_NewHistoryMemento(object sender, EventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            EnsureLastUndoItemIsFullyVisible();
            PerformMouseMove();
            PerformLayout();
            Invalidate();
        }

        private void History_Changed(object sender, EventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            PerformMouseMove();
            PerformLayout();
            Refresh();
            OnHistoryChanged();
        }

        public HistoryControl()
        {
            UI.InitScaling(this);
            ItemHeight = UI.ScaleHeight(16);

            SetStyle(ControlStyles.StandardDoubleClick, false);

            InitializeComponent();
        }

        private void KeyUpHandler(object sender, KeyEventArgs e)
        {
            OnKeyUp(e);
        }

        private void OnItemClicked(ItemType itemType, int itemIndex)
        {
            HistoryMemento hm;

            if (itemType == ItemType.Undo)
            {
                if (itemIndex >= 0 && itemIndex < HistoryStack.UndoStack.Count)
                {
                    hm = HistoryStack.UndoStack[itemIndex];
                }
                else
                {
                    hm = null;
                }
            }
            else
            {
                if (itemIndex >= 0 && itemIndex < HistoryStack.RedoStack.Count)
                {
                    hm = HistoryStack.RedoStack[itemIndex];
                }
                else
                {
                    hm = null;
                }
            }

            if (hm != null)
            {
                EnsureItemIsFullyVisible(itemType, itemIndex);
                OnItemClicked(itemType, hm);
            }
        }

        private void OnItemClicked(ItemType itemType, HistoryMemento hm)
        {
            int hmID = hm.ID;

            if (itemType == ItemType.Undo)
            {
                if (hmID == HistoryStack.UndoStack[HistoryStack.UndoStack.Count - 1].ID)
                {
                    if (HistoryStack.UndoStack.Count > 1)
                    {
                        HistoryStack.StepBackward();
                    }
                }
                else
                {
                    SuspendScrollOffsetSet();

                    HistoryStack.BeginStepGroup();

                    using (new WaitCursorChanger(this))
                    {
                        while (HistoryStack.UndoStack[HistoryStack.UndoStack.Count - 1].ID != hmID)
                        {
                            HistoryStack.StepBackward();
                        }
                    }

                    HistoryStack.EndStepGroup();

                    ResumeScrollOffsetSet();
                }
            }
            else // if (itemType == ItemType.Redo)
            {
                SuspendScrollOffsetSet();

                // Step forward to redo
                HistoryStack.BeginStepGroup();

                using (new WaitCursorChanger(this))
                {
                    while (HistoryStack.UndoStack[HistoryStack.UndoStack.Count - 1].ID != hmID)
                    {
                        HistoryStack.StepForward();
                    }
                }

                HistoryStack.EndStepGroup();

                ResumeScrollOffsetSet();
            }

            Focus();
        }

        protected override void OnResize(EventArgs e)
        {
            PerformLayout();
            base.OnResize(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            PerformLayout();
            base.OnSizeChanged(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            if (HistoryStack != null)
            {
                if (ManagedFocus && !MenuStripEx.IsAnyMenuActive && UI.IsOurAppActive)
                {
                    Focus();
                }
            }

            base.OnMouseEnter(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (HistoryStack != null)
            {
                Point clientPt = new Point(e.X, e.Y);
                Point viewPt = ClientPointToViewPoint(clientPt);

                ViewPointToStackIndex(viewPt, out ItemType itemType, out int itemIndex);

                switch (itemType)
                {
                    case ItemType.Undo:
                        if (itemIndex >= 0 && itemIndex < HistoryStack.UndoStack.Count)
                        {
                            UndoItemHighlight = itemIndex;
                        }
                        else
                        {
                            UndoItemHighlight = -1;
                        }

                        RedoItemHighlight = -1;
                        break;

                    case ItemType.Redo:
                        UndoItemHighlight = -1;

                        if (itemIndex >= 0 && itemIndex < HistoryStack.RedoStack.Count)
                        {
                            RedoItemHighlight = itemIndex;
                        }
                        else
                        {
                            RedoItemHighlight = -1;
                        } 
                        break;

                    default:
                        throw new InvalidEnumArgumentException();
                }

                Refresh();
                LastMouseClientPt = clientPt;
            }

            base.OnMouseMove(e);
        }

        protected override void OnClick(EventArgs e)
        {
            if (HistoryStack != null)
            {
                Point viewPt = ClientPointToViewPoint(LastMouseClientPt);

                ViewPointToStackIndex(viewPt, out ItemType itemType, out int itemIndex);

                OnItemClicked(itemType, itemIndex);
            }

            base.OnClick(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (HistoryStack != null)
            {
                int items = (e.Delta * SystemInformation.MouseWheelScrollLines) / SystemInformation.MouseWheelScrollDelta;
                int pixels = items * ItemHeight;
                ScrollOffset -= pixels;

                PerformMouseMove();
            }

            base.OnMouseWheel(e);
        }

        private void PerformMouseMove()
        {
            Point clientPt = PointToClient(Control.MousePosition);

            if (ClientRectangle.Contains(clientPt))
            {
                MouseEventArgs me = new MouseEventArgs(MouseButtons.None, 0, clientPt.X, clientPt.Y, 0);
                OnMouseMove(me);
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (HistoryStack != null)
            {
                UndoItemHighlight = -1;
                RedoItemHighlight = -1;
                Refresh();

                if (Focused && ManagedFocus)
                {
                    OnRelinquishFocus();
                }
            }

            base.OnMouseLeave(e);
        }

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            VScrollBar = new VScrollBar();
            SuspendLayout();
            //
            // vScrollBar
            //
            VScrollBar.Name = "vScrollBar";
            VScrollBar.ValueChanged += new EventHandler(VScrollBar_ValueChanged);
            //
            // HistoryControl
            //
            Name = "HistoryControl";
            TabStop = false;
            Controls.Add(VScrollBar);
            ResizeRedraw = true;
            DoubleBuffered = true;
            ResumeLayout();
            PerformLayout();
        }
        #endregion

        private void VScrollBar_ValueChanged(object sender, EventArgs e)
        {
            ScrollOffset = VScrollBar.Value;
        }
    }
}
