/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PaintDotNet.SystemLayer
{
    internal abstract class VistaFileDialog
        : IFileDialog
    {
        private DialogResult dialogResult = DialogResult.None;
        private NativeInterfaces.IFileDialogEvents fileDialogEvents;
        private string filter = null;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        protected NativeInterfaces.IFileDialogEvents FileDialogEvents
        {
            get
            {
                return this.fileDialogEvents;
            }

            set
            {
                if (this.fileDialogEvents == null)
                {
                    this.fileDialogEvents = value;
                }
                else
                {
                    throw new InvalidOperationException("May only set this property once");
                }
            }
        }

        protected IFileDialogUICallbacks FileDialogUICallbacks { get; private set; } = null;

        protected NativeInterfaces.IFileDialog FileDialog { get; }

        protected void SetOptions(NativeConstants.FOS flags, bool enable)
        {
            if (enable)
            {
                EnableOption(flags);
            }
            else
            {
                DisableOption(flags);
            }
        }

        protected void EnableOption(NativeConstants.FOS flags)
        {
            this.FileDialog.GetOptions(out NativeConstants.FOS oldOptions);
            NativeConstants.FOS newOptions = oldOptions | flags;
            this.FileDialog.SetOptions(newOptions);
        }

        protected void DisableOption(NativeConstants.FOS flags)
        {
            this.FileDialog.GetOptions(out NativeConstants.FOS oldOptions);
            NativeConstants.FOS newOptions = oldOptions & ~flags;
            this.FileDialog.SetOptions(newOptions);
        }

        protected bool GetOptions(NativeConstants.FOS flags)
        {
            this.FileDialog.GetOptions(out NativeConstants.FOS options);
            NativeConstants.FOS masked = options & flags;
            return masked == flags;
        }

        private NativeInterfaces.IShellItem GetShellItem(string path)
        {
            Guid iid_IShellItem = new Guid(NativeConstants.IID_IShellItem);
            IntPtr ppv = IntPtr.Zero;
            NativeMethods.SHCreateItemFromParsingName(path, IntPtr.Zero, ref iid_IShellItem, out ppv);
            object iUnknown = Marshal.GetObjectForIUnknown(ppv);
            NativeInterfaces.IShellItem shellItem = (NativeInterfaces.IShellItem)iUnknown;
            return shellItem;
        }

        public bool CheckPathExists
        {
            get
            {
                return GetOptions(NativeConstants.FOS.FOS_PATHMUSTEXIST);
            }

            set
            {
                SetOptions(NativeConstants.FOS.FOS_PATHMUSTEXIST, value);
            }
        }

        public bool DereferenceLinks
        {
            get
            {
                return !this.GetOptions(NativeConstants.FOS.FOS_NODEREFERENCELINKS);
            }

            set
            {
                SetOptions(NativeConstants.FOS.FOS_NODEREFERENCELINKS, !value);
            }
        }

        public string Filter
        {
            get
            {
                return this.filter;
            }

            set
            {
                string[] split = value.Split('|');

                if ((split.Length % 2) != 0)
                {
                    throw new ArgumentException();
                }

                NativeStructs.COMDLG_FILTERSPEC[] filterSpecs = new NativeStructs.COMDLG_FILTERSPEC[split.Length / 2];

                for (int i = 0; i < filterSpecs.Length; ++i)
                {
                    NativeStructs.COMDLG_FILTERSPEC filterSpec = new NativeStructs.COMDLG_FILTERSPEC();
                    filterSpec.pszName = split[i * 2];
                    filterSpec.pszSpec = split[(i * 2) + 1];
                    filterSpecs[i] = filterSpec;
                }

                this.FileDialog.SetFileTypes((uint)filterSpecs.Length, filterSpecs);
                this.filter = value;
            }
        }

        public int FilterIndex
        {
            get
            {
                this.FileDialog.GetFileTypeIndex(out uint index);
                return (int)index;
            }

            set
            {
                this.FileDialog.SetFileTypeIndex((uint)value);
            }
        }

        public string InitialDirectory { get; set; } = null;

        public string Title
        {
            set
            {
                this.FileDialog.SetTitle(value);
            }
        }

        protected virtual void OnBeforeShow()
        {
            NativeInterfaces.IShellItem shellItem = null;

            try
            {
                shellItem = GetShellItem(this.InitialDirectory);
                this.FileDialog.SetDefaultFolder(shellItem);
            }

            catch (Exception)
            {
                // Do nothing.
            }

            finally
            {
                if (shellItem != null)
                {
                    try
                    {
                        Marshal.ReleaseComObject(shellItem);
                    }

                    catch (ArgumentException)
                    {
                    }

                    shellItem = null;
                }
            }
        }

        protected virtual void OnAfterShow()
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "uiCallbacks")]
        public DialogResult ShowDialog(IWin32Window owner, IFileDialogUICallbacks uiCallbacks)
        {
            if (uiCallbacks == null)
            {
                throw new ArgumentNullException("uiCallbacks");
            }

            this.FileDialogUICallbacks = uiCallbacks;

            int hrCCD = FileDialog.ClearClientData();

            uint dwCookie = 0xdeadbeef;

            if (this.fileDialogEvents != null)
            {
                FileDialog.Advise(this.fileDialogEvents, out dwCookie);
            }

            OnBeforeShow();

            int hr = 0;

            UI.InvokeThroughModalTrampoline(
                owner,
                delegate(IWin32Window modalOwner)
                {
                    hr = this.FileDialog.Show(modalOwner.Handle);
                    GC.KeepAlive(modalOwner);
                });

            DialogResult result;

            if (hr >= 0)
            {
                result = DialogResult.OK;
            }
            else
            {
                result = DialogResult.Cancel;
            }

            this.dialogResult = result;

            if (this.fileDialogEvents != null)
            {
                FileDialog.Unadvise(dwCookie);
            }

            OnAfterShow();
            this.FileDialogUICallbacks = null;

            GC.KeepAlive(owner);
            return result;
        }

        protected VistaFileDialog(NativeInterfaces.IFileDialog fileDialog)
        {
            this.FileDialog = fileDialog;
        }

        ~VistaFileDialog()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
