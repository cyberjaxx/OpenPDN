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
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet
{
    // TODO: move
    internal delegate bool CmdKeysEventHandler(object sender, ref Message msg, Keys keyData);

    internal class FloatingToolForm 
        : PdnBaseForm,
          ISnapObstacleHost
    {
        private System.ComponentModel.IContainer components = null;

        private ControlEventHandler ControlAddedDelegate;
        private ControlEventHandler ControlRemovedDelegate;
        private KeyEventHandler KeyUpDelegate;
        private SnapObstacleController SnapObstacleController;

        public SnapObstacle SnapObstacle
        {
            get
            {
                if (SnapObstacleController == null)
                {
                    int distancePadding = UI.GetExtendedFrameBounds(this);
                    int distance = SnapObstacle.DefaultSnapDistance + distancePadding;

                    SnapObstacleController = new SnapObstacleController(Name, Bounds, SnapRegion.Exterior, false, SnapObstacle.DefaultSnapProximity, distance);
                    SnapObstacleController.BoundsChangeRequested += SnapObstacle_BoundsChangeRequested;
                }

                return SnapObstacleController;
            }
        }

        private void SnapObstacle_BoundsChangeRequested(object sender, HandledEventArgs<Rectangle> e)
        {
            Bounds = e.Data;
        }

        /// <summary>
        /// Occurs when it is appropriate for the parent to steal focus.
        /// </summary>
        public event EventHandler RelinquishFocus;
        protected virtual void OnRelinquishFocus()
        {
            // Only relinquish focus if we have it in the first place
            if (MenuStripEx.IsAnyMenuActive)
            {
                return;
            }

            RelinquishFocus?.Invoke(this, EventArgs.Empty);
        }

        public FloatingToolForm()
        {
            KeyPreview = true;
            ControlAddedDelegate = new ControlEventHandler(ControlAddedHandler);
            ControlRemovedDelegate = new ControlEventHandler(ControlRemovedHandler);
            KeyUpDelegate = new KeyEventHandler(KeyUpHandler);

            ControlAdded += ControlAddedDelegate; // we don't override OnControlAdded so we can re-use the method (see code below for ControlAdded)
            ControlRemoved += ControlRemovedDelegate;

            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            try
            {
                SystemLayer.UserSessions.SessionChanged += new EventHandler(UserSessions_SessionChanged);
                Microsoft.Win32.SystemEvents.DisplaySettingsChanged += new EventHandler(SystemEvents_DisplaySettingsChanged);
            }

            catch (Exception ex)
            {
                Tracing.Ping("Exception while signing up for some system events: " + ex.ToString());
            }
        }

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            if (Visible && IsShown)
            {
                EnsureFormIsOnScreen();
            }
        }

        private void UserSessions_SessionChanged(object sender, EventArgs e)
        {
            if (Visible && IsShown)
            {
                EnsureFormIsOnScreen();
            }
        }

        protected override void OnClick(EventArgs e)
        {
            OnRelinquishFocus();
            base.OnClick(e);
        }

        public event CmdKeysEventHandler ProcessCmdKeyEvent;

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            bool result = false;

            if (Utility.IsArrowKey(keyData))
            {
                KeyEventArgs kea = new KeyEventArgs(keyData);

                switch (msg.Msg)
                {
                    case 0x100: // WM_KEYDOWN:
                        OnKeyDown(kea);
                        return kea.Handled;

                /*
                case NativeMethods.WmConstants.WM_KEYUP:
                    this.OnKeyUp(kea);
                    return kea.Handled;
                */
                }
            }
            else
            {
                if (ProcessCmdKeyEvent != null)
                {
                    result = ProcessCmdKeyEvent(this, ref msg, keyData);
                }
            }

            if (!result)
            {
                result = base.ProcessCmdKey(ref msg, keyData);
            }

            return result;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                components = null;

                try
                {
                    SystemLayer.UserSessions.SessionChanged -= new EventHandler(UserSessions_SessionChanged);
                    Microsoft.Win32.SystemEvents.DisplaySettingsChanged -= new EventHandler(SystemEvents_DisplaySettingsChanged);
                }

                catch (Exception)
                {
                    // Ignore any errors
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
            // 
            // FloatingToolForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            ClientSize = new System.Drawing.Size(292, 271);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FloatingToolForm";
            ShowInTaskbar = false;
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            ForceActiveTitleBar = true;
        }
        #endregion

        private void ControlAddedHandler(object sender, ControlEventArgs e)
        {
            e.Control.ControlAdded += ControlAddedDelegate;
            e.Control.ControlRemoved += ControlRemovedDelegate;
            e.Control.KeyUp += KeyUpDelegate;
        }

        private void ControlRemovedHandler(object sender, ControlEventArgs e)
        {
            e.Control.ControlAdded -= ControlAddedDelegate;
            e.Control.ControlRemoved -= ControlRemovedDelegate;
            e.Control.KeyUp -= KeyUpDelegate;
        }

        private void KeyUpHandler(object sender, KeyEventArgs e)
        {
            if (!e.Handled)
            {
                OnKeyUp(e);
            }
        }

        private void UpdateSnapObstacleBounds()
        {
            if (SnapObstacleController != null)
            {
                SnapObstacleController.SetBounds(Bounds);
            }
        }

        private void UpdateParking()
        {
            if (FormBorderStyle == FormBorderStyle.Fixed3D ||
                FormBorderStyle == FormBorderStyle.FixedDialog ||
                FormBorderStyle == FormBorderStyle.FixedSingle ||
                FormBorderStyle == FormBorderStyle.FixedToolWindow)
            {
                if (Owner is ISnapManagerHost ismh)
                {
                    SnapManager mySM = ismh.SnapManager;
                    mySM.ReparkObstacle(this);
                }
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            if (Visible)
            {
                EnsureFormIsOnScreen();
            }

            base.OnVisibleChanged(e);
        }

        protected override void OnResizeBegin(EventArgs e)
        {
            UpdateSnapObstacleBounds();
            UpdateParking();
            base.OnResizeBegin(e);
        }

        protected override void OnResize(EventArgs e)
        {
            UpdateSnapObstacleBounds();
            base.OnResize(e);
            UpdateParking();
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            FormMoving = false;
            UpdateSnapObstacleBounds();
            UpdateParking();
            base.OnResizeEnd(e);
            OnRelinquishFocus();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            UpdateSnapObstacleBounds();
            UpdateParking();
            base.OnSizeChanged(e);
        }

        private Size MovingCursorDelta = Size.Empty; // dx,dy from  mousex,y to bounds.Location
        private bool FormMoving = false;

        protected override void OnMoving(MovingEventArgs mea)
        {
            if (Owner is ISnapManagerHost snapHost)
            {
                SnapManager sm = snapHost.SnapManager;

                // Make sure the window titlebar always follows a constant distance from the mouse cursor
                // Otherwise the window may "slip" as it snaps and unsnaps
                if (!FormMoving)
                {
                    MovingCursorDelta = new Size(
                        Cursor.Position.X - mea.Rectangle.X,
                        Cursor.Position.Y - mea.Rectangle.Y);

                    FormMoving = true;
                }

                mea.Rectangle = new Rectangle(
                    Cursor.Position.X - MovingCursorDelta.Width,
                    Cursor.Position.Y - MovingCursorDelta.Height,
                    mea.Rectangle.Width,
                    mea.Rectangle.Height);

                SnapObstacleController.SetBounds(mea.Rectangle);

                Point pt = mea.Rectangle.Location;
                Point newPt = sm.AdjustObstacleDestination(SnapObstacle, pt);
                Rectangle newRect = new Rectangle(newPt, mea.Rectangle.Size);

                SnapObstacleController.SetBounds(newRect);

                mea.Rectangle = newRect;
            }

            base.OnMoving(mea);
        }

        protected override void OnMove(EventArgs e)
        {
            UpdateSnapObstacleBounds();
            base.OnMove(e);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            if (SnapObstacleController != null)
            {
                SnapObstacleController.Enabled = Enabled;
            }

            base.OnEnabledChanged(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            if (Owner is ISnapManagerHost smh)
            {
                smh.SnapManager.AddSnapObstacle(this);
            }

            base.OnLoad(e);
        }
    }
}
