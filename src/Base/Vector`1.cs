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
using System.Collections.Generic;
using System.Drawing;

namespace PaintDotNet
{
    public sealed class Vector<T>
    {
        public Vector()
            : this(10)
        {
        }

        public Vector(int capacity)
        {
            this.UnsafeArray = new T[capacity];
        }

        public Vector(IEnumerable<T> copyMe)
        {
            foreach (T t in copyMe)
            {
                Add(t);
            }
        }

        public void Add(T pt)
        {
            if (this.Count >= this.UnsafeArray.Length)
            {
                Grow(this.Count + 1);
            }

            this.UnsafeArray[this.Count] = pt;
            ++this.Count;
        }

        public void Insert(int index, T item)
        {
            if (this.Count >= this.UnsafeArray.Length)
            {
                Grow(this.Count + 1);
            }

            ++this.Count;

            for (int i = this.Count - 1; i >= index + 1; --i)
            {
                this.UnsafeArray[i] = this.UnsafeArray[i - 1];
            }

            this.UnsafeArray[index] = item;
        }

        public void Clear()
        {
            this.Count = 0;
        }

        public T this[int index]
        {
            get
            {
                return Get(index);
            }

            set
            {
                Set(index, value);
            }
        }

        public T Get(int index)
        {
            if (index < 0 || index >= this.Count)
            {
                throw new ArgumentOutOfRangeException("index", index, "0 <= index < count");
            }

            return this.UnsafeArray[index];
        }

        public unsafe T GetUnchecked(int index)
        {
            return this.UnsafeArray[index];
        }

        public void Set(int index, T pt)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", index, "0 <= index");
            }

            if (index >= this.UnsafeArray.Length)
            {
                Grow(index + 1);
            }

            this.UnsafeArray[index] = pt;
        }

        public int Count { get; private set; } = 0;

        private void Grow(int min)
        {
            int newSize = this.UnsafeArray.Length;

            if (newSize <= 0)
            {
                newSize = 1;
            }

            while (newSize < min)
            {
                newSize = 1 + ((newSize * 10) / 8);
            }

            T[] replacement = new T[newSize];

            for (int i = 0; i < this.Count; i++)
            {
                replacement[i] = this.UnsafeArray[i];
            }

            this.UnsafeArray = replacement;
        }

        public T[] ToArray()
        {
            T[] ret = new T[this.Count];

            for (int i = 0; i < this.Count; i++)
            {
                ret[i] = this.UnsafeArray[i];
            }

            return ret;
        }

        public unsafe T[] UnsafeArray { get; private set; }

        /// <summary>
        /// Gets direct access to the array held by the Vector.
        /// The caller must not modify the array.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="length">The actual number of items stored in the array. This number will be less than or equal to array.Length.</param>
        /// <remarks>This method is supplied strictly for performance-critical purposes.</remarks>
        public unsafe void GetArrayReadOnly(out T[] arrayResult, out int lengthResult)
        {
            arrayResult = this.UnsafeArray;
            lengthResult = this.Count;
        }
    }
}
