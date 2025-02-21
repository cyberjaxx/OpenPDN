/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Carries information about the Font details that we support.
    /// Does not carry text alignment information.
    /// </summary>
    [Serializable]
    internal class FontInfo
        : IDisposable,
          ISerializable,
          ICloneable
    {
        private const string fontFamilyNameTag = "family.Name";
        private const string sizeTag = "size";
        private const string styleTag = "style";

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(fontFamilyNameTag, this.FontFamily.Name);
            info.AddValue(sizeTag, this.Size);
            info.AddValue(styleTag, (int)this.FontStyle);
        }

        protected FontInfo(SerializationInfo info, StreamingContext context)
        {
            string familyName = info.GetString(fontFamilyNameTag);

            try
            {
                this.FontFamily = new FontFamily(familyName);
            }

            catch (ArgumentException)
            {
                this.FontFamily = FontFamily.GenericSansSerif;
            }

            this.Size = info.GetSingle(sizeTag);

            int styleInt = info.GetInt32(styleTag);
            this.FontStyle = (FontStyle)styleInt;
        }

        public static bool operator ==(FontInfo lhs, FontInfo rhs)
        {
            if ((lhs.FontFamily == rhs.FontFamily) && (lhs.Size == rhs.Size) && (lhs.FontStyle == rhs. FontStyle))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool operator!= (FontInfo lhs, FontInfo rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object obj)
        {
            return this == (FontInfo)obj;
        }

        public override int GetHashCode()
        {
            return unchecked(FontFamily.GetHashCode() + Size.GetHashCode() + FontStyle.GetHashCode());
        }
        
        /// <summary>
        /// Constructs an instance of the FontInfo class.
        /// </summary>
        /// <param name="fontFamily">The FontFamily to associate with this class. The FontInfo instance takes ownership of this FontFamily instance: do not call Dispose() on it.</param>
        /// <param name="size">The Size of the font.</param>
        /// <param name="fontStyle">The FontStyle of the font.</param>
        public FontInfo(FontFamily fontFamily, float size, FontStyle fontStyle)
        {
            this.FontFamily = fontFamily;
            this.Size = size;
            this.FontStyle = fontStyle;
        }

        ~FontInfo()
        {
            Dispose(false);
        }

        /// <summary>
        /// The FontFamily property gets and sets the font family.
        /// </summary>
        public FontFamily FontFamily { get; set; }

        /// <summary>
        /// The Size property gets and sets the size of the text.
        /// </summary>
        public float Size { get; set; }

        /// <summary>
        /// The FontStyle property gets and sets the font style to bold and or italic and or underline.
        /// </summary>
        public FontStyle FontStyle { get; set; }

        public bool CanCreateFont()
        {
            if (this.FontFamily.IsStyleAvailable(this.FontStyle))   // check to see if style is available in family
            {
                return true;
            }
            else    // find the style it is available in (coersion)
            {
                if (this.FontFamily.IsStyleAvailable(FontStyle.Regular))
                {
                    return true;
                }
                else if (this.FontFamily.IsStyleAvailable(FontStyle.Italic))
                {
                    return true;
                }
                else if (this.FontFamily.IsStyleAvailable(FontStyle.Bold))
                {
                    return true;
                }
                else if (this.FontFamily.IsStyleAvailable(FontStyle.Underline))
                {
                    return true;
                }
            }

            return false;
        }

        public Font CreateFont()
        {   
            // it may be possible that the font style may not be available in the font returned.  But I am not sure.
            // I am checking for this possibility in the textconfig widget getFontInfo fucntion
            if (this.FontFamily.IsStyleAvailable(this.FontStyle))   // check to see if style is availble in family
            {
                return new Font(this.FontFamily, (float)Math.Max(1.0, (double)this.Size), this.FontStyle);
            }
            else    // find the style it is available in (coersion)
            {
                FontStyle fs = new FontStyle();

                if (this.FontFamily.IsStyleAvailable(FontStyle.Regular))
                {
                    fs = FontStyle.Regular;
                }
                else if (this.FontFamily.IsStyleAvailable(FontStyle.Italic))
                {
                    fs = FontStyle.Italic;
                }
                else if (this.FontFamily.IsStyleAvailable(FontStyle.Bold))
                {
                    fs = FontStyle.Bold;    
                }
                else if (this.FontFamily.IsStyleAvailable(FontStyle.Underline))
                {
                    fs = FontStyle.Underline;
                }

                try
                {
                    return new Font(this.FontFamily, (float)Math.Max(1.0, (double)this.Size), fs);
                }

                catch
                {
                    return null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.FontFamily != null)
                {
                    this.FontFamily.Dispose();
                    this.FontFamily = null;
                }
            }
        }

        public FontInfo Clone()
        {
            return new FontInfo(this.FontFamily, this.Size, this.FontStyle);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
