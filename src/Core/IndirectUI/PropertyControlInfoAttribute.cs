/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.Core;
using PaintDotNet.PropertySystem;
using System;

namespace PaintDotNet.IndirectUI
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal class PropertyControlInfoAttribute
        : Attribute
    {
        public Type PropertyType { get; }

        public PropertyControlType ControlType { get; }

        public bool IsDefault { get; set; }

        public PropertyControlInfoAttribute(Type propertyType, PropertyControlType controlType)
        {
            if (!typeof(Property).IsAssignableFrom(propertyType))
            {
                throw new ArgumentException("propertyType must be a type that derives from Property");
            }

            this.PropertyType = propertyType;
            this.ControlType = controlType;
        }
    }
}
