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
    public struct Scanline
    {
        public int X { get; }

        public int Y { get; }

        public int Length { get; }

        public override int GetHashCode()
        {
            unchecked
            {
                return Length.GetHashCode() + X.GetHashCode() + Y.GetHashCode();
            }
        }
        
        public override bool Equals(object obj)
        {
            return obj is Scanline rhs ? X == rhs.X && Y == rhs.Y && Length == rhs.Length : false;
        }

        public static bool operator== (Scanline lhs, Scanline rhs)
        {
            return lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.Length == rhs.Length;
        }

        public static bool operator!= (Scanline lhs, Scanline rhs)
        {
            return !(lhs == rhs);
        }

        public override string ToString()
        {
            return "(" + X + "," + Y + "):[" + Length.ToString() + "]";
        }

        public Scanline(int x, int y, int length)
        {
            this.X = x;
            this.Y = y;
            this.Length = length;
        }
    }
}
