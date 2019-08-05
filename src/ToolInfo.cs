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
    internal class ToolInfo
    {
        public string Name { get; }

        public string HelpText { get; }

        public ImageResource Image { get; }

        public bool SkipIfActiveOnHotKey { get; }

        public char HotKey { get; }

        public Type ToolType { get; }

        public ToolBarConfigItems ToolBarConfigItems { get; }

        public override bool Equals(object obj)
        {
            return !(obj is ToolInfo rhs) ? false
                : (Name == rhs.Name) &&
                   (HelpText == rhs.HelpText) &&
                   (HotKey == rhs.HotKey) &&
                   (SkipIfActiveOnHotKey == rhs.SkipIfActiveOnHotKey) &&
                   (ToolType == rhs.ToolType);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public ToolInfo(
            string name, 
            string helpText, 
            ImageResource image, 
            char hotKey, 
            bool skipIfActiveOnHotKey, 
            ToolBarConfigItems toolBarConfigItems, 
            Type toolType)
        {
            Name = name;
            HelpText = helpText;
            Image = image;
            HotKey = hotKey;
            SkipIfActiveOnHotKey = skipIfActiveOnHotKey;
            ToolBarConfigItems = toolBarConfigItems;
            ToolType = toolType;
        }
    }
}
