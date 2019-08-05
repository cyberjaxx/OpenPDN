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
    internal class ToleranceSliderControl 
        : ToolbarSliderControl
    {
        public float Tolerance
        {
            get => base.Value;
            set => base.Value = value;
        }

        public EventHandler ToleranceChanged;
        protected override void OnValueChanged() 
        {
            base.OnValueChanged();
            ToleranceChanged?.Invoke(this, EventArgs.Empty);
        }

        public void PerformToleranceChanged() 
        {
            OnValueChanged();
        }

        public ToleranceSliderControl()
        {
            base.fValue = 0.5f;
            base.controlText = PdnResources.GetString("ToleranceSliderControl.Tolerance");
            base.percentageFormat = PdnResources.GetString("ToleranceSliderControl.Percentage.Format");
        }

        protected override void InitializeComponent()
        {
            base.InitializeComponent();
            this.Name = "ToleranceSliderControl";
        }
    }
}