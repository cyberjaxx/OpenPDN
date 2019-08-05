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
using System.Text;
using System.Windows.Forms;

namespace PaintDotNet.Updates
{
    internal abstract class UpdatesState
        : State
    {
        public new UpdatesStateMachine StateMachine
        {
            get => (UpdatesStateMachine)base.StateMachine;
        }

        public virtual string InfoText
        {
            get
            {
                string infoTextStringName = "UpdatesDialog.InfoText.Text." + this.GetType().Name;
                string infoText = PdnResources.GetString(infoTextStringName);
                return infoText;
            }
        }

        public string ContinueButtonText
        {
            get
            {
                string continueButtonTextStringName = "UpdatesDialog.ContinueButton.Text." + this.GetType().Name;
                string continueButtonText = PdnResources.GetString(continueButtonTextStringName);
                return continueButtonText;
            }
        }

        public bool ContinueButtonVisible { get; }

        public MarqueeStyle MarqueeStyle { get; }

        public UpdatesState(bool isFinalState, bool continueButtonVisible, MarqueeStyle marqueeStyle)
            : base(isFinalState)
        {
            ContinueButtonVisible = continueButtonVisible;
            MarqueeStyle = marqueeStyle;
            SystemLayer.Tracing.LogFeature("UpdatesState(" + GetType().Name + ")");
        }
    }
}
