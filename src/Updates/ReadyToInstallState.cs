/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows.Forms;

namespace PaintDotNet.Updates
{
    internal class ReadyToInstallState
        : UpdatesState,
          INewVersionInfo
    {
        private string InstallerPath { get; }

        public PdnVersionInfo NewVersionInfo { get; }

        public override void OnEnteredState()
        {
        }

        public override void ProcessInput(object input, out State newState)
        {
            if (input.Equals(UpdatesAction.Continue))
            {
                newState = new InstallingState(InstallerPath);
            }
            else if (input.Equals(UpdatesAction.Cancel))
            {
                newState = new DoneState();
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public ReadyToInstallState(string installerPath, PdnVersionInfo newVersionInfo)
            : base(false, true, MarqueeStyle.None)
        {
            InstallerPath = installerPath;
            NewVersionInfo = newVersionInfo;
        }
    }
}
