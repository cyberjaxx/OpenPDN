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

namespace PaintDotNet
{
    public sealed class TaskButton
    {
        private static TaskButton cancel = null;
        public static TaskButton Cancel
        {
            get
            {
                if (cancel == null)
                {
                    cancel = new TaskButton(
                        PdnResources.GetImageResource("Icons.CancelIcon.png").Reference,
                        PdnResources.GetString("TaskButton.Cancel.ActionText"),
                        PdnResources.GetString("TaskButton.Cancel.ExplanationText"));
                }

                return cancel;
            }
        }

        public Image Image { get; }

        public string ActionText { get; }

        public string ExplanationText { get; }

        public TaskButton(Image image, string actionText, string explanationText)
        {
            this.Image = image;
            this.ActionText = actionText;
            this.ExplanationText = explanationText;
        }
    }
}
