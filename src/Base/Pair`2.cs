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
    [Serializable]
    public struct Pair<T, U>
    {
        public T First { get; }

        public U Second { get; }

        public override int GetHashCode()
        {
            int firstHash;
            int secondHash;

            if (object.ReferenceEquals(this.First, null))
            {
                firstHash = 0;
            }
            else
            {
                firstHash = this.First.GetHashCode();
            }

            if (object.ReferenceEquals(this.Second, null))
            {
                secondHash = 0;
            }
            else
            {
                secondHash = this.Second.GetHashCode();
            }

            return firstHash ^ secondHash;
        }

        public override bool Equals(object obj)
        {
            return ((obj != null) && (obj is Pair<T, U>) && (this == (Pair<T, U>)obj));
        }

        public static bool operator ==(Pair<T, U> lhs, Pair<T, U> rhs)
        {
            bool firstEqual;
            bool secondEqual;

            if (object.ReferenceEquals(lhs.First, null) && object.ReferenceEquals(rhs.First, null))
            {
                firstEqual = true;
            }
            else if (object.ReferenceEquals(lhs.First, null) || object.ReferenceEquals(rhs.First, null))
            {
                firstEqual = false;
            }
            else
            {
                firstEqual = lhs.First.Equals(rhs.First);
            }

            if (object.ReferenceEquals(lhs.Second, null) && object.ReferenceEquals(rhs.Second, null))
            {
                secondEqual = true;
            }
            else if (object.ReferenceEquals(lhs.Second, null) || object.ReferenceEquals(rhs.Second, null))
            {
                secondEqual = false;
            }
            else
            {
                secondEqual = lhs.Second.Equals(rhs.Second);
            }

            return firstEqual && secondEqual;
        }

        public static bool operator !=(Pair<T, U> lhs, Pair<T, U> rhs)
        {
            return !(lhs == rhs);
        }

        public Pair(T first, U second)
        {
            this.First = first;
            this.Second = second;
        }
    }
}
