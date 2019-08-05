/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.Menus;
using PaintDotNet.SystemLayer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace PaintDotNet
{
    internal class PdnToolBar 
        : Control,
          IPaintBackground
    {
        private const ToolStripGripStyle toolStripsGripStyle = ToolStripGripStyle.Hidden;
        private DateTime ignoreShowDocumentListUntil = DateTime.MinValue;

        private AppWorkspace appWorkspace;
        private OurDocumentStrip documentStrip;
        private ArrowButton documentListButton;
        private ImageListMenu imageListMenu;
        private OurToolStripRenderer otsr = new OurToolStripRenderer();

        private class OurToolStripRenderer :
            ToolStripProfessionalRenderer
        {
            public OurToolStripRenderer()
            {
                RoundedEdges = false;
            }

            private void PaintBackground(Graphics g, Control control, Rectangle clipRect)
            {
                Control parent = control;
                IPaintBackground asIpb = null;

                while (true)
                {
                    parent = parent.Parent;

                    if (parent == null)
                    {
                        break;
                    }

                    asIpb = parent as IPaintBackground;

                    if (asIpb != null)
                    {
                        break;
                    }
                }

                if (asIpb != null)
                {
                    Rectangle screenRect = control.RectangleToScreen(clipRect);
                    Rectangle parentRect = parent.RectangleToClient(screenRect);

                    int dx = parentRect.Left - clipRect.Left;
                    int dy = parentRect.Top - clipRect.Top;

                    g.TranslateTransform(-dx, -dy, MatrixOrder.Append);
                    asIpb.PaintBackground(g, parentRect);
                    g.TranslateTransform(dx, dy, MatrixOrder.Append);
                }
            }

            protected override void OnRenderToolStripPanelBackground(ToolStripPanelRenderEventArgs e)
            {
                PaintBackground(e.Graphics, e.ToolStripPanel, new Rectangle(new Point(0, 0), e.ToolStripPanel.Size));
                e.Handled = true;
            }

            protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
            {
                if (e.ToolStrip.GetType() != typeof(ToolStrip) &&
                    e.ToolStrip.GetType() != typeof(ToolStripEx) &&
                    e.ToolStrip.GetType() != typeof(PdnMainMenu))
                {
                    base.OnRenderToolStripBackground(e);
                }
                else
                {
                    PaintBackground(e.Graphics, e.ToolStrip, e.AffectedBounds);
                }
            }
        }

        private class OurDocumentStrip :
            DocumentStrip,
            IPaintBackground
        {
            protected override void DrawItemBackground(Graphics g, Item item, Rectangle itemRect)
            {
                PaintBackground(g, itemRect);
            }

            public void PaintBackground(Graphics g, Rectangle clipRect)
            {
                if (this.Parent is IPaintBackground asIpb)
                {
                    Rectangle newClipRect = new Rectangle(
                        clipRect.Left + Left, clipRect.Top + Top,
                        clipRect.Width, clipRect.Height);

                    g.TranslateTransform(-Left, -Top, MatrixOrder.Append);
                    asIpb.PaintBackground(g, newClipRect);
                    g.TranslateTransform(Left, Top, MatrixOrder.Append);
                }
            }
        }

        public void PaintBackground(Graphics g, Rectangle clipRect)
        {
            if (clipRect.Width > 0 && clipRect.Height > 0)
            {
                Color backColor = ProfessionalColors.MenuStripGradientEnd;

                using (SolidBrush brush = new SolidBrush(backColor))
                {
                    g.FillRectangle(brush, clipRect);
                }
            }
        }

        public AppWorkspace AppWorkspace
        {
            get
            {
                return this.appWorkspace;
            }

            set
            {
                this.appWorkspace = value;
                this.MainMenu.AppWorkspace = value;
            }
        }

        public PdnMainMenu MainMenu { get; private set; }

        public ToolStripPanel ToolStripContainer { get; private set; }

        public CommonActionsStrip CommonActionsStrip { get; private set; }

        public ViewConfigStrip ViewConfigStrip { get; private set; }

        public ToolChooserStrip ToolChooserStrip { get; private set; }

        public ToolConfigStrip ToolConfigStrip { get; private set; }

        public DocumentStrip DocumentStrip
        {
            get
            {
                return this.documentStrip;
            }
        }

        public PdnToolBar()
        {
            SuspendLayout();
            InitializeComponent();

            ToolInfo[] toolInfos = DocumentWorkspace.ToolInfos;

            this.ToolChooserStrip.SetTools(toolInfos);

            this.otsr = new OurToolStripRenderer();
            this.CommonActionsStrip.Renderer = otsr;
            this.ViewConfigStrip.Renderer = otsr;
            this.ToolStripContainer.Renderer = otsr;
            this.ToolChooserStrip.Renderer = otsr;
            this.ToolConfigStrip.Renderer = otsr;
            this.MainMenu.Renderer = otsr;

            ResumeLayout(true);
        }

        private bool computedMaxRowHeight = false;
        private int maxRowHeight = -1;

        protected override void OnLayout(LayoutEventArgs e)
        {
            bool plentyWidthBefore =
                (this.MainMenu.Width >= this.MainMenu.PreferredSize.Width) &&
                (this.CommonActionsStrip.Width >= this.CommonActionsStrip.PreferredSize.Width) &&
                (this.ViewConfigStrip.Width >= this.ViewConfigStrip.PreferredSize.Width) &&
                (this.ToolChooserStrip.Width >= this.ToolChooserStrip.PreferredSize.Width) &&
                (this.ToolConfigStrip.Width >= this.ToolConfigStrip.PreferredSize.Width);

            if (!plentyWidthBefore)
            {
                UI.SuspendControlPainting(this);
            }
            else
            {
                // if we don't do this then we get some terrible flickering of the right scroll arrow
                UI.SuspendControlPainting(this.documentStrip);
            }

            this.MainMenu.Location = new Point(0, 0);
            this.MainMenu.Height = this.MainMenu.PreferredSize.Height;
            this.ToolStripContainer.Location = new Point(0, this.MainMenu.Bottom);

            this.ToolStripContainer.RowMargin = new Padding(0);
            this.MainMenu.Padding = new Padding(0, this.MainMenu.Padding.Top, 0, this.MainMenu.Padding.Bottom);

            this.CommonActionsStrip.Width = this.CommonActionsStrip.PreferredSize.Width;
            this.ViewConfigStrip.Width = this.ViewConfigStrip.PreferredSize.Width;
            this.ToolChooserStrip.Width = this.ToolChooserStrip.PreferredSize.Width;
            this.ToolConfigStrip.Width = this.ToolConfigStrip.PreferredSize.Width;

            if (!this.computedMaxRowHeight)
            {
                ToolBarConfigItems oldTbci = this.ToolConfigStrip.ToolBarConfigItems;
                this.ToolConfigStrip.ToolBarConfigItems = ToolBarConfigItems.All;
                this.ToolConfigStrip.PerformLayout();

                this.maxRowHeight =
                    Math.Max(this.CommonActionsStrip.PreferredSize.Height,
                    Math.Max(this.ViewConfigStrip.PreferredSize.Height,
                    Math.Max(this.ToolChooserStrip.PreferredSize.Height, this.ToolConfigStrip.PreferredSize.Height)));

                this.ToolConfigStrip.ToolBarConfigItems = oldTbci;
                this.ToolConfigStrip.PerformLayout();

                this.computedMaxRowHeight = true;
            }

            this.CommonActionsStrip.Height = this.maxRowHeight;
            this.ViewConfigStrip.Height = this.maxRowHeight;
            this.ToolChooserStrip.Height = this.maxRowHeight;
            this.ToolConfigStrip.Height = this.maxRowHeight;

            this.CommonActionsStrip.Location = new Point(0, 0);
            this.ViewConfigStrip.Location = new Point(this.CommonActionsStrip.Right, this.CommonActionsStrip.Top);
            this.ToolChooserStrip.Location = new Point(0, this.ViewConfigStrip.Bottom);
            this.ToolConfigStrip.Location = new Point(this.ToolChooserStrip.Right, this.ToolChooserStrip.Top);

            this.ToolStripContainer.Height =
                Math.Max(this.CommonActionsStrip.Bottom,
                Math.Max(this.ViewConfigStrip.Bottom,
                Math.Max(this.ToolChooserStrip.Bottom,
                         this.ToolConfigStrip.Visible ? this.ToolConfigStrip.Bottom : this.ToolChooserStrip.Bottom)));

            // Compute how wide the toolStripContainer would like to be
            int widthRow1 =
                this.CommonActionsStrip.Left + this.CommonActionsStrip.PreferredSize.Width + this.CommonActionsStrip.Margin.Horizontal +
                this.ViewConfigStrip.PreferredSize.Width + this.ViewConfigStrip.Margin.Horizontal;

            int widthRow2 =
                this.ToolChooserStrip.Left + this.ToolChooserStrip.PreferredSize.Width + this.ToolChooserStrip.Margin.Horizontal +
                this.ToolConfigStrip.PreferredSize.Width + this.ToolConfigStrip.Margin.Horizontal;

            int preferredMinTscWidth = Math.Max(widthRow1, widthRow2);

            // Throw in the documentListButton if necessary
            bool showDlb = this.documentStrip.DocumentCount > 0;

            this.documentListButton.Visible = showDlb;
            this.documentListButton.Enabled = showDlb;

            if (showDlb)
            {
                int documentListButtonWidth = UI.ScaleWidth(15);
                this.documentListButton.Width = documentListButtonWidth;
            }
            else
            {
                this.documentListButton.Width = 0;
            }

            // Figure out the DocumentStrip's size -- we actually make two passes at setting its Width
            // so that we can toss in the documentListButton if necessary
            if (this.documentStrip.DocumentCount == 0)
            {
                this.documentStrip.Width = 0;
            }
            else
            {
                this.documentStrip.Width = Math.Max(
                    this.documentStrip.PreferredMinClientWidth,
                    Math.Min(this.documentStrip.PreferredSize.Width, 
                             ClientSize.Width - preferredMinTscWidth - this.documentListButton.Width));
            }

            this.documentStrip.Location = new Point(ClientSize.Width - this.documentStrip.Width, 0);
            this.documentListButton.Location = new Point(this.documentStrip.Left - this.documentListButton.Width, 0);

            this.imageListMenu.Location = new Point(this.documentListButton.Left, this.documentListButton.Bottom - 1);
            this.imageListMenu.Width = this.documentListButton.Width;
            this.imageListMenu.Height = 0;

            this.documentListButton.Visible = showDlb;
            this.documentListButton.Enabled = showDlb;

            // Finish setting up widths and heights
            int oldDsHeight = this.documentStrip.Height;
            this.documentStrip.Height = this.ToolStripContainer.Bottom;
            this.documentListButton.Height = this.documentStrip.Height;

            int tsWidth = ClientSize.Width - (this.documentStrip.Width + this.documentListButton.Width);
            this.MainMenu.Width = tsWidth;
            this.ToolStripContainer.Width = tsWidth;

            this.Height = this.ToolStripContainer.Bottom;

            // Now get stuff to paint right
            this.documentStrip.PerformLayout();

            if (!plentyWidthBefore)
            {
                UI.ResumeControlPainting(this);
                Invalidate(true);
            }
            else
            {
                UI.ResumeControlPainting(this.documentStrip);
                this.documentStrip.Invalidate(true);
            }

            if (this.documentStrip.Width == 0)
            {
                this.MainMenu.Invalidate();
            }

            if (oldDsHeight != this.documentStrip.Height)
            {
                this.documentStrip.RefreshAllThumbnails();
            }

            base.OnLayout(e);
        }

        protected override void OnResize(EventArgs e)
        {
            PerformLayout();
            base.OnResize(e);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.MainMenu = new PdnMainMenu();
            this.ToolStripContainer = new ToolStripPanel();
            this.CommonActionsStrip = new CommonActionsStrip();
            this.ViewConfigStrip = new ViewConfigStrip();
            this.ToolChooserStrip = new ToolChooserStrip();
            this.ToolConfigStrip = new ToolConfigStrip();
            this.documentStrip = new OurDocumentStrip();
            this.documentListButton = new ArrowButton();
            this.imageListMenu = new ImageListMenu();
            this.ToolStripContainer.BeginInit();
            this.ToolStripContainer.SuspendLayout();
            // 
            // mainMenu
            // 
            this.MainMenu.Name = "mainMenu";
            //
            // toolStripContainer
            //
            this.ToolStripContainer.AutoSize = true;
            this.ToolStripContainer.Name = "toolStripPanel";
            this.ToolStripContainer.TabIndex = 0;
            this.ToolStripContainer.TabStop = false;
            this.ToolStripContainer.Join(this.ViewConfigStrip);
            this.ToolStripContainer.Join(this.CommonActionsStrip);
            this.ToolStripContainer.Join(this.ToolConfigStrip);
            this.ToolStripContainer.Join(this.ToolChooserStrip);
            //
            // commonActionsStrip
            //
            this.CommonActionsStrip.Name = "commonActionsStrip";
            this.CommonActionsStrip.AutoSize = false;
            this.CommonActionsStrip.TabIndex = 0;
            this.CommonActionsStrip.Dock = DockStyle.None;
            this.CommonActionsStrip.GripStyle = toolStripsGripStyle;
            //
            // viewConfigStrip
            //
            this.ViewConfigStrip.Name = "viewConfigStrip";
            this.ViewConfigStrip.AutoSize = false;
            this.ViewConfigStrip.ZoomBasis = PaintDotNet.ZoomBasis.FitToWindow;
            this.ViewConfigStrip.TabStop = false;
            this.ViewConfigStrip.DrawGrid = false;
            this.ViewConfigStrip.TabIndex = 1;
            this.ViewConfigStrip.Dock = DockStyle.None;
            this.ViewConfigStrip.GripStyle = toolStripsGripStyle;
            //
            // toolChooserStrip
            //
            this.ToolChooserStrip.Name = "toolChooserStrip";
            this.ToolChooserStrip.AutoSize = false;
            this.ToolChooserStrip.TabIndex = 2;
            this.ToolChooserStrip.Dock = DockStyle.None;
            this.ToolChooserStrip.GripStyle = toolStripsGripStyle;
            this.ToolChooserStrip.ChooseDefaultsClicked += new EventHandler(ToolChooserStrip_ChooseDefaultsClicked);
            //
            // toolConfigStrip
            //
            this.ToolConfigStrip.Name = "drawConfigStrip";
            this.ToolConfigStrip.AutoSize = false;
            this.ToolConfigStrip.ShapeDrawType = PaintDotNet.ShapeDrawType.Outline;
            this.ToolConfigStrip.TabIndex = 3;
            this.ToolConfigStrip.Dock = DockStyle.None;
            this.ToolConfigStrip.GripStyle = toolStripsGripStyle;
            this.ToolConfigStrip.Layout +=
                delegate(object sender, LayoutEventArgs e)
                {
                    PerformLayout();
                };
            this.ToolConfigStrip.SelectionDrawModeInfoChanged +=
                delegate(object sender, EventArgs e)
                {
                    BeginInvoke(new Procedure(PerformLayout));
                };
            //
            // documentStrip
            //
            this.documentStrip.AutoSize = false;
            this.documentStrip.Name = "documentStrip";
            this.documentStrip.TabIndex = 5;
            this.documentStrip.ShowScrollButtons = true;
            this.documentStrip.DocumentListChanged += new EventHandler(DocumentStrip_DocumentListChanged);
            this.documentStrip.DocumentClicked += DocumentStrip_DocumentClicked;
            this.documentStrip.ManagedFocus = true;
            //
            // documentListButton
            //
            this.documentListButton.Name = "documentListButton";
            this.documentListButton.ArrowDirection = ArrowDirection.Down;
            this.documentListButton.ReverseArrowColors = true;
            this.documentListButton.Click += new EventHandler(DocumentListButton_Click);
            //
            // imageListMenu
            //
            this.imageListMenu.Name = "imageListMenu";
            this.imageListMenu.Closed += new EventHandler(ImageListMenu_Closed);
            this.imageListMenu.ItemClicked += ImageListMenu_ItemClicked;
            //
            // PdnToolBar
            //
            this.Controls.Add(this.documentListButton);
            this.Controls.Add(this.documentStrip);
            this.Controls.Add(this.ToolStripContainer);
            this.Controls.Add(this.MainMenu);
            this.Controls.Add(this.imageListMenu);
            this.ToolStripContainer.ResumeLayout(false);
            this.ToolStripContainer.EndInit();
            this.ResumeLayout(false);
        }

        private void ToolChooserStrip_ChooseDefaultsClicked(object sender, EventArgs e)
        {
            PdnBaseForm.UpdateAllForms();

            WaitCursorChanger wcc = new WaitCursorChanger(this);

            using (ChooseToolDefaultsDialog dialog = new ChooseToolDefaultsDialog())
            {
                EventHandler shownDelegate = null;

                shownDelegate =
                    delegate(object sender2, EventArgs e2)
                    {
                        wcc.Dispose();
                        wcc = null;
                        dialog.Shown -= shownDelegate;
                    };

                dialog.Shown += shownDelegate;
                dialog.SetToolBarSettings(this.appWorkspace.GlobalToolTypeChoice, this.appWorkspace.AppEnvironment);

                AppEnvironment defaultAppEnv = AppEnvironment.GetDefaultAppEnvironment();

                try
                {
                    dialog.LoadUIFromAppEnvironment(defaultAppEnv);
                }

                catch (Exception)
                {
                    defaultAppEnv = new AppEnvironment();
                    defaultAppEnv.SetToDefaults();
                    dialog.LoadUIFromAppEnvironment(defaultAppEnv);
                }

                dialog.ToolType = this.appWorkspace.DefaultToolType;

                DialogResult dr = dialog.ShowDialog(this);

                if (dr != DialogResult.Cancel)
                {
                    AppEnvironment newDefaultAppEnv = dialog.CreateAppEnvironmentFromUI();
                    newDefaultAppEnv.SaveAsDefaultAppEnvironment();
                    this.appWorkspace.AppEnvironment.LoadFrom(newDefaultAppEnv);
                    this.appWorkspace.DefaultToolType = dialog.ToolType;
                    this.appWorkspace.GlobalToolTypeChoice = dialog.ToolType;
                }
            }

            if (wcc != null)
            {
                wcc.Dispose();
                wcc = null;
            }
        }

        private void DocumentListButton_Click(object sender, EventArgs e)
        {
            if (this.imageListMenu.IsImageListVisible)
            {
                HideDocumentList();
            }
            else
            {
                ShowDocumentList();
            }
        }

        public void HideDocumentList()
        {
            this.imageListMenu.HideImageList();
        }

        private void ImageListMenu_Closed(object sender, EventArgs e)
        {
            this.documentListButton.ForcedPushedAppearance = false;

            // We set this up because otherwise if the user clicks on the documentListButton,
            // then first the documentListMenu closes, and then the documentClickButton's Click
            // event fires. The behavior we want is to hide the menu when this Click occurs,
            // but since the menu is already hidden we have no way of knowing that we should
            // not show the menu.
            this.ignoreShowDocumentListUntil = DateTime.Now + new TimeSpan(0, 0, 0, 0, 250);
        }

        private void ImageListMenu_ItemClicked(object sender, EventArgs<ImageListMenu.Item> e)
        {
            DocumentWorkspace dw = (DocumentWorkspace)e.Data.Tag;

            if (!dw.IsDisposed)
            {
                this.documentStrip.SelectedDocument = dw;
            }
        }

        public void ShowDocumentList()
        {
            if (this.documentStrip.DocumentCount < 1)
            {
                return;
            }

            if (DateTime.Now < this.ignoreShowDocumentListUntil)
            {
                return;
            }

            if (this.imageListMenu.IsImageListVisible)
            {
                return;
            }

            DocumentWorkspace[] documents = this.documentStrip.DocumentList;
            Image[] thumbnails = this.documentStrip.DocumentThumbnails;

            ImageListMenu.Item[] items = new ImageListMenu.Item[this.documentStrip.DocumentCount];

            for (int i = 0; i < items.Length; ++i)
            {
                bool selected = (documents[i] == this.documentStrip.SelectedDocument);

                items[i] = new ImageListMenu.Item(
                    thumbnails[i],
                    documents[i].GetFriendlyName(),
                    selected);

                items[i].Tag = documents[i];
            }

            Cursor.Current = Cursors.Default;

            this.documentListButton.ForcedPushedAppearance = true;
            this.imageListMenu.ShowImageList(items);
        }

        private void DocumentStrip_DocumentClicked(object sender, EventArgs<Pair<DocumentWorkspace, DocumentClickAction>> e)
        {
            if (e.Data.Second == DocumentClickAction.Select)
            {
                PerformLayout();
            }
        }

        private void DocumentStrip_DocumentListChanged(object sender, EventArgs e)
        {
            PerformLayout();

            if (this.documentStrip.DocumentCount == 0)
            {
                this.ViewConfigStrip.Enabled = false;
                this.ToolChooserStrip.Enabled = false;
                this.ToolConfigStrip.Enabled = false;
            }
            else
            {
                this.ViewConfigStrip.Enabled = true;
                this.ToolChooserStrip.Enabled = true;
                this.ToolConfigStrip.Enabled = true;
            }
        }
    }
}
