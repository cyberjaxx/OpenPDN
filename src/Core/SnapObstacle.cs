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
using System.Drawing.Drawing2D;

namespace PaintDotNet
{
    public abstract class SnapObstacle
    {
        public const int DefaultSnapProximity = 15;
        public const int DefaultSnapDistance = 3;
        protected Rectangle previousBounds; // for BoundsChanged event
        protected Rectangle bounds;

        public string Name { get; }

        /// <summary>
        /// Gets the bounds of this snap obstacle, defined in coordinates relative to its container.
        /// </summary>
        public Rectangle Bounds
        {
            get
            {
                return this.bounds;
            }
        }

        protected virtual void OnBoundsChangeRequested(Rectangle newBounds, ref bool handled)
        {
        }

        public bool RequestBoundsChange(Rectangle newBounds)
        {
            bool handled = false;
            OnBoundsChangeRequested(newBounds, ref handled);
            return handled;
        }

        public SnapRegion SnapRegion { get; }

        /// <summary>
        /// Gets whether or not this obstacle has "sticky" edges.
        /// </summary>
        /// <remarks>
        /// If an obstacle has sticky edges, than any obstacle that is snapped on 
        /// to it will move with this obstacle.
        /// </remarks>
        public bool StickyEdges { get; }

        /// <summary>
        /// Gets how close another obstacle must be to snap to this one, in pixels
        /// </summary>
        public int SnapProximity { get; }

        /// <summary>
        /// Gets how close another obstacle will be parked when it snaps to this one, in pixels.
        /// </summary>
        public int SnapDistance { get; }

        public bool Enabled { get; set; }

        public bool EnableSave { get; set; }

        /// <summary>
        /// Raised before the Bounds is changed.
        /// </summary>
        /// <remarks>
        /// The Data property of the event args is the value that Bounds is being set to.
        /// </remarks>
        public event EventHandler<EventArgs<Rectangle>> BoundsChanging;
        protected virtual void OnBoundsChanging()
        {
            BoundsChanging?.Invoke(this, new EventArgs<Rectangle>(this.Bounds));
        }

        /// <summary>
        /// Raised after the Bounds is changed.
        /// </summary>
        /// <remarks>
        /// The Data property of the event args is the value that Bounds was just changed from.
        /// </remarks>
        public event EventHandler<EventArgs<Rectangle>> BoundsChanged;
        protected virtual void OnBoundsChanged()
        {
            BoundsChanged?.Invoke(this, new EventArgs<Rectangle>(this.previousBounds));
        }

        internal SnapObstacle(string name, Rectangle bounds, SnapRegion snapRegion, bool stickyEdges)
            : this(name, bounds, snapRegion, stickyEdges, DefaultSnapProximity, DefaultSnapDistance)
        {
        }

        internal SnapObstacle(string name, Rectangle bounds, SnapRegion snapRegion, bool stickyEdges, int snapProximity, int snapDistance)
        {
            this.Name = name;
            this.bounds = bounds;
            this.previousBounds = bounds;
            this.SnapRegion = snapRegion;
            this.StickyEdges = stickyEdges;
            this.SnapProximity = snapProximity;
            this.SnapDistance = snapDistance;
            this.Enabled = true;
            this.EnableSave = true;
        }
    }
}
