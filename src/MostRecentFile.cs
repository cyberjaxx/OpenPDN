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
    /// <summary>
    /// Encapsulates a filename and a thumbnail.
    /// </summary>
    internal class MostRecentFile
    {
        public string FileName { get; }

        public Image Thumb { get; }

        public MostRecentFile(string fileName, Image thumb)
        {
            FileName = fileName;
            Thumb = thumb;
        }
    }
}
