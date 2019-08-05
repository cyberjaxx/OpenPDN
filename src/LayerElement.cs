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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet
{
    internal class LayerElement 
        : UserControl
    {
        public const int ThumbSizePreScaling = 40;

        private Layer layer;

        private PropertyEventHandler LayerPropertyChangedDelegate;

        private int suspendPreviewUpdates = 0;

        public ThumbnailManager ThumbnailManager { get; set; }

        private int thumbnailSize = 16;
        public int ThumbnailSize
        {
            get => thumbnailSize;

            set
            {
                if (thumbnailSize != value)
                {
                    thumbnailSize = value;
                    RefreshPreview();
                }
            }
        }


        private bool isSelected;
        public bool IsSelected
        {
            get
            {
                return this.isSelected;
            }
            set
            {
                this.isSelected = value;

                if (this.isSelected)
                {
                    LayerDescription.BackColor = SystemColors.Highlight;
                    LayerDescription.ForeColor = SystemColors.HighlightText;
                    LayerVisible.BackColor = this.LayerDescription.BackColor;
                    Icon.BackColor = SystemColors.Highlight;
                }
                else // !selected
                {               
                    LayerDescription.ForeColor = SystemColors.WindowText;
                    LayerDescription.BackColor = SystemColors.Window;
                    LayerVisible.BackColor = this.LayerDescription.BackColor;
                    Icon.BackColor = SystemColors.Window;
                }

                Update();
            }
        }

        public Image Image
        {
            get => Icon.Image;

            set
            {
                Icon.Image?.Dispose();
                Icon.Image = value;
                Invalidate(true);
                Update();
            }
        }

        public Layer Layer 
        {
            get => this.layer;

            set
            {
                if (ReferenceEquals(this.layer, value))
                {
                    return;
                }

                if (this.layer != null)
                {
                    this.layer.PropertyChanged -= LayerPropertyChangedDelegate;
                    this.layer.Invalidated -= Layer_Invalidated;
                }
                
                this.layer = value;

                if (this.layer != null)
                {
                    this.layer.PropertyChanged += LayerPropertyChangedDelegate;
                    this.layer.Invalidated += Layer_Invalidated;
                    this.LayerPropertyChangedDelegate(layer, new PropertyEventArgs("")); // sync up

                    // Add italics if it's the background layer
                    if (this.layer.IsBackground)
                    {
                        LayerDescription.Font = new Font(LayerDescription.Font.FontFamily, LayerDescription.Font.Size, 
                            LayerDescription.Font.Style | FontStyle.Italic);
                    }

                    RefreshPreview();
                }

                Update();
            }
        }
        
        public LayerElement()
        {
            // This call is required by the Windows.Forms Form Designer.
            this.SuspendLayout();
            InitializeComponent();
            InitializeComponent2();
            this.ResumeLayout(false);
            this.IsSelected = false;
            this.TabStop = false;
            LayerPropertyChangedDelegate = new PropertyEventHandler(LayerPropertyChangedHandler);
        }

        private void LayerPropertyChangedHandler(object sender, PropertyEventArgs e)
        {
            LayerDescription.Text = layer.Name;
            LayerVisible.Checked = layer.Visible;
        }

        private void InitializeComponent2()
        {
            this.Size = new System.Drawing.Size(200, SystemLayer.UI.ScaleWidth(LayerElement.ThumbSizePreScaling));
            this.Icon.Size = new System.Drawing.Size(6 + this.Height, this.Height);
            this.LayerDescription.Location = new System.Drawing.Point(this.Icon.Right, 0);
            this.LayerVisible.Size = new System.Drawing.Size(16, this.Height);
        }


        public void SuspendPreviewUpdates()
        {
            ++suspendPreviewUpdates;
        }

        public void ResumePreviewUpdates()
        {
            --suspendPreviewUpdates;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            RefreshPreview();
            base.OnHandleCreated(e);
        }

        public void RefreshPreview()
        {
            if (suspendPreviewUpdates > 0)
            {
                return;
            }

            if (!IsHandleCreated)
            {
                return;
            }

            ThumbnailManager.QueueThumbnailUpdate(Layer, ThumbnailSize, OnThumbnailRendered);
        }

        private void OnThumbnailRendered(object sender, EventArgs<Pair<IThumbnailProvider, Surface>> e)
        {
            if (!IsDisposed)
            {
                Bitmap thumbBitmap = e.Data.Second.CreateAliasedBitmap();
                Image = CreateIconImage(thumbBitmap);
            }
        }

        private Bitmap CreateIconImage(Bitmap thumbBitmap)
        {
            Bitmap bitmap = new Bitmap(Icon.Width, Icon.Height, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CompositingMode = CompositingMode.SourceCopy;
                g.Clear(Color.Transparent);

                Rectangle thumbRect = new Rectangle(
                    (bitmap.Width - thumbBitmap.Width) / 2,
                    (bitmap.Height - thumbBitmap.Height) / 2,
                    thumbBitmap.Width,
                    thumbBitmap.Height);

                g.DrawImage(
                    thumbBitmap,
                    thumbRect,
                    new Rectangle(new Point(0, 0), thumbBitmap.Size),
                    GraphicsUnit.Pixel);

                Rectangle outlineRect = thumbRect;
                --outlineRect.X;
                --outlineRect.Y;
                ++outlineRect.Width;
                ++outlineRect.Height;
                g.DrawRectangle(Pens.Black, outlineRect);

                g.CompositingMode = CompositingMode.SourceOver;

                Rectangle dropShadowRect = outlineRect;
                dropShadowRect.Inflate(1, 1);
                ++dropShadowRect.Width;
                ++dropShadowRect.Height;
                Utility.DrawDropShadow1px(g, dropShadowRect);
            }

            thumbBitmap.Dispose();
            return bitmap;
        }

        private void Control_Click(object sender, EventArgs e)
        {
            OnClick(e);
        }

        private void Control_DoubleClick(object sender, EventArgs e)
        {
            OnDoubleClick(e);
        }

        private void LayerVisible_CheckStateChanged(object sender, EventArgs e)
        {
            Layer.Visible = LayerVisible.Checked;
            Update();
        }

        private void LayerVisible_KeyPress(object sender, KeyPressEventArgs e)
        {
            OnKeyPress(e);
        }

        private void LayerVisible_KeyUp(object sender, KeyEventArgs e)
        {
            OnKeyUp(e);
        }

        private void Layer_Invalidated(object sender, InvalidateEventArgs e)
        {
            RefreshPreview();
        }

        #region LayerElement.Designer.cs

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Layer = null;

                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }
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
            this.LayerDescription = new System.Windows.Forms.Label();
            this.Icon = new System.Windows.Forms.PictureBox();
            this.LayerVisible = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.Icon)).BeginInit();
            this.SuspendLayout();
            // 
            // LayerDescription
            // 
            this.LayerDescription.BackColor = System.Drawing.SystemColors.Window;
            this.LayerDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LayerDescription.Location = new System.Drawing.Point(100, 0);
            this.LayerDescription.Name = "LayerDescription";
            this.LayerDescription.Size = new System.Drawing.Size(0, 150);
            this.LayerDescription.TabIndex = 9;
            this.LayerDescription.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.LayerDescription.Click += new System.EventHandler(this.Control_Click);
            this.LayerDescription.DoubleClick += new System.EventHandler(this.Control_DoubleClick);
            // 
            // Icon
            // 
            this.Icon.BackColor = System.Drawing.SystemColors.Control;
            this.Icon.Dock = System.Windows.Forms.DockStyle.Left;
            this.Icon.Location = new System.Drawing.Point(0, 0);
            this.Icon.Name = "Icon";
            this.Icon.Size = new System.Drawing.Size(100, 150);
            this.Icon.TabIndex = 10;
            this.Icon.TabStop = false;
            this.Icon.Click += new System.EventHandler(this.Control_Click);
            this.Icon.DoubleClick += new System.EventHandler(this.Control_DoubleClick);
            // 
            // LayerVisible
            // 
            this.LayerVisible.BackColor = System.Drawing.SystemColors.Window;
            this.LayerVisible.Checked = true;
            this.LayerVisible.CheckState = System.Windows.Forms.CheckState.Checked;
            this.LayerVisible.Dock = System.Windows.Forms.DockStyle.Right;
            this.LayerVisible.Location = new System.Drawing.Point(46, 0);
            this.LayerVisible.Name = "LayerVisible";
            this.LayerVisible.Size = new System.Drawing.Size(104, 150);
            this.LayerVisible.TabIndex = 7;
            this.LayerVisible.UseVisualStyleBackColor = false;
            this.LayerVisible.CheckStateChanged += new System.EventHandler(this.LayerVisible_CheckStateChanged);
            this.LayerVisible.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.LayerVisible_KeyPress);
            this.LayerVisible.KeyUp += new System.Windows.Forms.KeyEventHandler(this.LayerVisible_KeyUp);
            // 
            // LayerElement
            // 
            this.Controls.Add(this.LayerDescription);
            this.Controls.Add(this.Icon);
            this.Controls.Add(this.LayerVisible);
            this.Name = "LayerElement";
            ((System.ComponentModel.ISupportInitialize)(this.Icon)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.Label LayerDescription;
        private System.Windows.Forms.PictureBox Icon;
        private System.Windows.Forms.CheckBox LayerVisible;

        #endregion
    }
}
