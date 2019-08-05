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

namespace PaintDotNet
{
    /// <summary>
    /// This class is used to hold references to many of the UI elements
    /// that are privately encapsulated in various places.
    /// This allows other program elements to access these objects while
    /// allowing these items to move around, and without breaking OO best 
    /// practices.
    /// </summary>
    internal class WorkspaceWidgets
    {
        private AppWorkspace Workspace { get; }
        public DocumentStrip DocumentStrip { get; set; }
        public ViewConfigStrip ViewConfigStrip { get; set; }
        public ToolConfigStrip ToolConfigStrip { get; set; }
        public CommonActionsStrip CommonActionsStrip { get; set; }
        public ToolsForm ToolsForm { get; set; }

        public ToolsControl ToolsControl
        {
            get => ToolsForm.ToolsControl;
        }

        public LayerForm LayerForm { get; set; }

        public LayerControl LayerControl
        {
            get => LayerForm.LayerControl;
        }

        public HistoryForm HistoryForm { get; set; }

        public HistoryControl HistoryControl
        {
            get => HistoryForm.HistoryControl;
        }

        public ColorsForm ColorsForm { get; set; }

        public IStatusBarProgress StatusBarProgress { get; set; }

        public WorkspaceWidgets(AppWorkspace workspace)
        {
            Workspace = workspace;
        }
    }
}
