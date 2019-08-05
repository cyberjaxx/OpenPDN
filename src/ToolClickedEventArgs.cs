/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet
{
    internal class ToolClickedEventArgs
        : System.EventArgs
    {
        public Type ToolType { get; }

        public ToolClickedEventArgs(Tool tool)
        {
            this.ToolType = tool.GetType();
        }

        public ToolClickedEventArgs(Type toolType)
        {
            this.ToolType = toolType;
        }
    }
}
