/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.SystemLayer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;

namespace PaintDotNet
{
    /// <summary>
    /// Manages a selection for Paint.NET.
    /// There are five major components of a selection:
    /// * Base path
    /// * Continuation path
    /// * Continuation combination mode
    /// * Cumulative transform
    /// * Interim transform
    /// The selection itself, as returned by e.g. CreatePath(), is (base COMBINE continuation) x interimTransform.
    /// Whenever the interim transform is set, the continuation path is first committed, and vice versa.
    /// Because of this, you may think of the selection as operating in two editing 'modes': editing
    /// the path, and editing the transformation.
    /// When the continuation path is committed, it is appended to the base path using the given combination mode. 
    /// The continuation is then reset to empty.
    /// When the interim transform is committed, both the base path and the cumulative transform
    /// are multiplied by it. The interim transform is then reset to the identity matrix.
    /// If the selection is empty, then its "clip region" is the entire canvas as set by the ClipRectangle
    /// property.
    /// </summary>
    public sealed class Selection
    {
        public object SyncRoot { get; } = new object();

        private class Data
            : ICloneable,
              IDisposable
        {
            public PdnGraphicsPath BasePath { get; set; }
            public PdnGraphicsPath ContinuationPath { get; set; }
            public CombineMode ContinuationCombineMode { get; set; }
            public Matrix CumulativeTransform { get; set; } // resets whenever SetContinuation is called
            public Matrix InterimTransform { get; set; }

            public Data()
            {
                BasePath = new PdnGraphicsPath();
                ContinuationPath = new PdnGraphicsPath();
                ContinuationCombineMode = CombineMode.Xor;
                CumulativeTransform = new Matrix();
                CumulativeTransform.Reset();
                InterimTransform = new Matrix();
                InterimTransform.Reset();            
            }

            ~Data()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            public void Dispose(bool disposing)
            {
                if (disposing)
                {
                    BasePath?.Dispose();
                    BasePath = null;

                    ContinuationPath?.Dispose();
                    ContinuationPath = null;

                    CumulativeTransform?.Dispose();
                    CumulativeTransform = null;

                    InterimTransform?.Dispose();
                    InterimTransform = null;
                }
            }

            public Data Clone()
            {
                return new Data
                {
                    BasePath = BasePath.Clone(),
                    ContinuationPath = ContinuationPath.Clone(),
                    ContinuationCombineMode = ContinuationCombineMode,
                    CumulativeTransform = CumulativeTransform.Clone(),
                    InterimTransform = InterimTransform.Clone()
                };
            }

            object ICloneable.Clone()
            {
                return Clone();
            }
        }

        private Data SelectionData { get; set; }
        private int AlreadyChanging { get; set; } // we don't want to nest Changing events -- consolidate them with this

        public Selection()
        {
            SelectionData = new Data();
            AlreadyChanging = 0;
            ClipRectangle = new Rectangle(0, 0, 65535, 65535);
        }

        public Rectangle ClipRectangle { get; set; }

        public bool IsEmpty
        {
            get => SelectionData.BasePath.IsEmpty && SelectionData.ContinuationPath.IsEmpty;
        }

        public bool IsVisible(Point pt)
        {
            using (PdnGraphicsPath path = CreatePath())
            {
                return path.IsVisible(pt);
            }
        }

        public object Save()
        {
            lock (SyncRoot)
            {
                return SelectionData.Clone();
            }
        }

        public void Restore(object state)
        {
            lock (SyncRoot)
            {
                OnChanging();
                SelectionData.Dispose();
                SelectionData = ((Data)state).Clone();
                OnChanged();
            }
        }

        public PdnGraphicsPath CreatePixelatedPath()
        {
            using (PdnGraphicsPath path = CreatePath())
            {
                using (PdnRegion region = new PdnRegion(path))
                {
                    PdnGraphicsPath pixellatedPath = PdnGraphicsPath.FromRegion(region);
                    return pixellatedPath;
                }
            }
        }

        public PdnGraphicsPath CreatePath()
        {
            return CreatePath(true);
        }

        public PdnGraphicsPath CreatePath(bool applyInterimTransform)
        {
            lock (SyncRoot)
            {
                PdnGraphicsPath returnPath = PdnGraphicsPath.Combine(SelectionData.BasePath, SelectionData.ContinuationCombineMode, SelectionData.ContinuationPath);

                if (applyInterimTransform)
                {
                    returnPath.Transform(SelectionData.InterimTransform);
                }

                return returnPath;
            }
        }

        public void Reset()
        {
            lock (SyncRoot)
            {
                OnChanging();
                SelectionData.BasePath.Dispose();
                SelectionData.BasePath = new PdnGraphicsPath();
                SelectionData.ContinuationPath.Dispose();
                SelectionData.ContinuationPath = new PdnGraphicsPath();
                SelectionData.CumulativeTransform.Reset();
                SelectionData.InterimTransform.Reset();
                OnChanged();
            }
        }

        public void ResetContinuation()
        {
            lock (SyncRoot)
            {
                OnChanging();
                CommitInterimTransform();
                ResetCumulativeTransform();
                SelectionData.ContinuationPath.Reset();
                OnChanged();
            }
        }

        public Rectangle GetBounds()
        {
            return GetBounds(true);
        }

        public Rectangle GetBounds(bool applyInterimTransformation)
        {
            return Utility.RoundRectangle(GetBoundsF(applyInterimTransformation));
        }

        public RectangleF GetBoundsF()
        {
            return GetBoundsF(true);
        }

        public RectangleF GetBoundsF(bool applyInterimTransformation)
        {
            using (PdnGraphicsPath path = CreatePath(applyInterimTransformation))
            //PdnGraphicsPath path = GetPathReadOnly(applyInterimTransformation);
            {
                RectangleF bounds2 = path.GetBounds2();
                return bounds2;
            }
        }

        public void SetContinuation(Rectangle rect, CombineMode combineMode)
        {
            lock (SyncRoot)
            {
                OnChanging();
                CommitInterimTransform();
                ResetCumulativeTransform();
                SelectionData.ContinuationCombineMode = combineMode;
                SelectionData.ContinuationPath.Reset();
                SelectionData.ContinuationPath.AddRectangle(rect);
                OnChanged();
            }
        }

        public void SetContinuation(Point[] linePoints, CombineMode combineMode)
        {
            lock (SyncRoot)
            {
                OnChanging();
                CommitInterimTransform();
                ResetCumulativeTransform();
                SelectionData.ContinuationCombineMode = combineMode;
                SelectionData.ContinuationPath.Reset();
                SelectionData.ContinuationPath.AddLines(linePoints);
                OnChanged();
            }
        }

        public void SetContinuation(PointF[] linePointsF, CombineMode combineMode)
        {
            lock (SyncRoot)
            {
                OnChanging();
                CommitInterimTransform();
                ResetCumulativeTransform();
                SelectionData.ContinuationCombineMode = combineMode;
                SelectionData.ContinuationPath.Reset();
                SelectionData.ContinuationPath.AddLines(linePointsF);
                OnChanged();
            }
        }

        public void SetContinuation(PointF[][] polygonSet, CombineMode combineMode)
        {
            lock (SyncRoot)
            {
                OnChanging();
                CommitInterimTransform();
                ResetCumulativeTransform();
                SelectionData.ContinuationCombineMode = combineMode;
                SelectionData.ContinuationPath.Reset();
                SelectionData.ContinuationPath.AddPolygons(polygonSet);
                OnChanged();
            }
        }

        public void SetContinuation(Point[][] polygonSet, CombineMode combineMode)
        {
            lock (SyncRoot)
            {
                OnChanging();
                CommitInterimTransform();
                ResetCumulativeTransform();
                SelectionData.ContinuationCombineMode = combineMode;
                SelectionData.ContinuationPath.Reset();
                SelectionData.ContinuationPath.AddPolygons(polygonSet);
                OnChanged();
            }
        }

        // only works if base is empty 
        public void SetContinuation(PdnGraphicsPath path, CombineMode combineMode, bool takeOwnership)
        {
            lock (SyncRoot)
            {
                if (!SelectionData.BasePath.IsEmpty)
                {
                    throw new InvalidOperationException("base path must be empty to use this overload of SetContinuation");
                }

                OnChanging();

                CommitInterimTransform();
                ResetCumulativeTransform();

                SelectionData.ContinuationCombineMode = combineMode;

                if (takeOwnership)
                {
                    SelectionData.ContinuationPath.Dispose();
                    SelectionData.ContinuationPath = path;
                }
                else
                {
                    SelectionData.ContinuationPath.Reset();
                    SelectionData.ContinuationPath.AddPath(path, false);
                }

                OnChanged();
            }
        }

        public void CommitContinuation()
        {
            lock (SyncRoot)
            {
                OnChanging();
                SelectionData.ContinuationPath.CloseAllFigures();
                PdnGraphicsPath newBasePath = CreatePath();
                SelectionData.BasePath.Dispose();
                SelectionData.BasePath = newBasePath;
                SelectionData.ContinuationPath.Reset();
                SelectionData.ContinuationCombineMode = CombineMode.Xor;
                OnChanged();
            }
        }

        public Matrix GetCumulativeTransformCopy()
        {
            lock (SyncRoot)
            {
                if (SelectionData.CumulativeTransform == null)
                {
                    Matrix m = new Matrix();
                    m.Reset();
                    return m;
                }
                else
                {
                    return SelectionData.CumulativeTransform.Clone();
                }
            }
        }

        public Matrix GetCumulativeTransformReadOnly()
        {
            lock (SyncRoot)
            {
                return SelectionData.CumulativeTransform;
            }
        }

        private void ResetCumulativeTransform()
        {
            lock (SyncRoot)
            {
                if (SelectionData.CumulativeTransform == null)
                {
                    SelectionData.CumulativeTransform = new Matrix();
                }

                SelectionData.CumulativeTransform.Reset();
            }
        }

        public Matrix GetInterimTransformCopy()
        {
            lock (SyncRoot)
            {
                if (SelectionData.InterimTransform == null)
                {
                    Matrix m = new Matrix();
                    m.Reset();
                    return m;
                }
                else
                {
                    return SelectionData.InterimTransform.Clone();
                }
            }
        }

        public Matrix GetInterimTransformReadOnly()
        {
            lock (SyncRoot)
            {
                return SelectionData.InterimTransform;
            }
        }

        public void SetInterimTransform(Matrix m)
        {
            lock (SyncRoot)
            {
                OnChanging();
                SelectionData.InterimTransform.Dispose();
                SelectionData.InterimTransform = m.Clone();
                OnChanged();
            }
        }

        public void CommitInterimTransform()
        {
            lock (SyncRoot)
            {
                if (!SelectionData.InterimTransform.IsIdentity)
                {
                    OnChanging();
                    SelectionData.BasePath.Transform(SelectionData.InterimTransform);
                    SelectionData.ContinuationPath.Transform(SelectionData.InterimTransform);
                    SelectionData.CumulativeTransform.Multiply(SelectionData.InterimTransform, MatrixOrder.Append);
                    SelectionData.InterimTransform.Reset();
                    OnChanged();
                }
            }
        }

        public void ResetInterimTransform()
        {
            lock (SyncRoot)
            {
                OnChanging();
                SelectionData.InterimTransform.Reset();
                OnChanged();
            }
        }

        public PdnRegion CreateRegionRaw()
        {
            using (PdnGraphicsPath path = CreatePath())
            {
                return new PdnRegion(path);
            }
        }

        public PdnRegion CreateRegion()
        {
            lock (SyncRoot)
            {
                if (IsEmpty)
                {
                    return new PdnRegion(ClipRectangle);
                }
                else
                {
                    PdnRegion region = CreateRegionRaw();
                    region.Intersect(ClipRectangle);
                    return region;
                }
            }
        }

        public event EventHandler Changing;
        private void OnChanging()
        {
            lock (SyncRoot)
            {
                if (AlreadyChanging == 0)
                {
                    Changing?.Invoke(this, EventArgs.Empty);
                }
            }

            ++AlreadyChanging;
        }

        public void PerformChanging()
        {
            OnChanging();
        }

        public event EventHandler Changed;
        private void OnChanged()
        {
            lock (SyncRoot)
            {
                if (AlreadyChanging <= 0)
                {
                    throw new InvalidOperationException("Changed event was raised without corresponding Changing event beforehand");
                }

                --AlreadyChanging;
            }

            if (AlreadyChanging == 0)
            {
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        public void PerformChanged()
        {
            OnChanged();
        }
    }
}
