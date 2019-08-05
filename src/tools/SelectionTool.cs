/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.HistoryMementos;
using PaintDotNet.HistoryFunctions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PaintDotNet.Tools
{
    internal class SelectionTool
        : Tool
    {
        private bool Tracking { get; set; } = false;
        private bool MoveOriginMode { get; set; } = false;
        private Point LastPoint { get; set; }
        private SelectionHistoryMemento UndoAction { get; set; }
        private List<Point> TracePoints = null;
        private DateTime StartTime { get; set; }
        private bool HasMoved { get; set; } = false;
        private bool Append { get; set; } = false;
        private bool WasNotEmpty { get; set; } = false;

        private Selection NewSelection { get; set; }
        private SelectionRenderer NewSelectionRenderer { get; set; }

        private Cursor CursorMouseUp { get; set; }
        private Cursor CursorMouseUpMinus { get; set; }
        private Cursor CursorMouseUpPlus { get; set; }
        private Cursor CursorMouseDown { get; set; }

        protected CombineMode SelectionMode { get; private set; }

        protected void SetCursors(
            string cursorMouseUpResName,
            string cursorMouseUpMinusResName,
            string cursorMouseUpPlusResName,
            string cursorMouseDownResName)
        {
            CursorMouseUp?.Dispose();
            CursorMouseUp = null;

            if (cursorMouseUpResName != null)
            {
                CursorMouseUp = new Cursor(PdnResources.GetResourceStream(cursorMouseUpResName));
            }

            CursorMouseUpMinus?.Dispose();
            CursorMouseUpMinus = null;

            if (cursorMouseUpMinusResName != null)
            {
                CursorMouseUpMinus = new Cursor(PdnResources.GetResourceStream(cursorMouseUpMinusResName));
            }

            CursorMouseUpPlus?.Dispose();
            CursorMouseUpPlus = null;

            if (cursorMouseUpPlusResName != null)
            {
                CursorMouseUpPlus = new Cursor(PdnResources.GetResourceStream(cursorMouseUpPlusResName));
            }

            CursorMouseDown?.Dispose();
            CursorMouseDown = null;

            if (cursorMouseDownResName != null)
            {
                CursorMouseDown = new Cursor(PdnResources.GetResourceStream(cursorMouseDownResName));
            }
        }

        private Cursor GetCursor(bool mouseDown, bool ctrlDown, bool altDown)
        {
            Cursor cursor;

            if (mouseDown)
            {
                cursor = CursorMouseDown;
            }
            else if (ctrlDown)
            {
                cursor = CursorMouseUpPlus;
            }
            else if (altDown)
            {
                cursor = CursorMouseUpMinus;
            }
            else
            {
                cursor = CursorMouseUp;
            }

            return cursor;
        }

        private Cursor GetCursor()
        {
            return GetCursor(IsMouseDown, (ModifierKeys & Keys.Control) != 0, (ModifierKeys & Keys.Alt) != 0);
        }

        protected override void OnActivate()
        {
            // Assume that SetCursors() has been called by now

            Cursor = GetCursor();
            DocumentWorkspace.EnableSelectionTinting = true;

            NewSelection = new Selection();
            NewSelectionRenderer = new SelectionRenderer(RendererList, NewSelection, DocumentWorkspace)
            {
                EnableSelectionTinting = false,
                EnableOutlineAnimation = false,
                Visible = false
            };
            RendererList.Add(NewSelectionRenderer, true);

            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            DocumentWorkspace.EnableSelectionTinting = false;

            if (Tracking)
            {
                Done();
            }

            base.OnDeactivate();

            SetCursors(null, null, null, null); // dispose 'em

            RendererList.Remove(NewSelectionRenderer);
            NewSelectionRenderer.Dispose();
            NewSelectionRenderer = null;
            NewSelection = null;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            Cursor = GetCursor();

            if (Tracking)
            {
                MoveOriginMode = true;
                LastPoint = new Point(e.X, e.Y);
                OnMouseMove(e);
            }
            else if ((e.Button & MouseButtons.Left) == MouseButtons.Left ||
                (e.Button & MouseButtons.Right) == MouseButtons.Right)
            {
                Tracking = true;
                HasMoved = false;
                StartTime = DateTime.Now;

                TracePoints = new List<Point>
                {
                    new Point(e.X, e.Y)
                };

                UndoAction = new SelectionHistoryMemento("sentinel", Image, DocumentWorkspace);

                WasNotEmpty = !Selection.IsEmpty;

                // Determine this.combineMode

                if ((ModifierKeys & Keys.Control) != 0 && e.Button == MouseButtons.Left)
                {
                    SelectionMode = CombineMode.Union;
                }
                else if ((ModifierKeys & Keys.Alt) != 0 && e.Button == MouseButtons.Left)
                {
                    SelectionMode = CombineMode.Exclude;
                }
                else if ((ModifierKeys & Keys.Control) != 0 && e.Button == MouseButtons.Right)
                {
                    SelectionMode = CombineMode.Xor;
                }
                else if ((ModifierKeys & Keys.Alt) != 0 && e.Button == MouseButtons.Right)
                {
                    SelectionMode = CombineMode.Intersect;
                }
                else
                {
                    SelectionMode = AppEnvironment.SelectionCombineMode;
                }


                DocumentWorkspace.EnableSelectionOutline = false;

                NewSelection.Reset();
                PdnGraphicsPath basePath = Selection.CreatePath();
                NewSelection.SetContinuation(basePath, CombineMode.Replace, true);
                NewSelection.CommitContinuation();

                bool newSelectionRendererVisible = true;

                // Act on this.combineMode
                switch (SelectionMode)
                {
                    case CombineMode.Xor:
                        Append = true;
                        Selection.ResetContinuation();
                        break;

                    case CombineMode.Union:
                        Append = true;
                        Selection.ResetContinuation();
                        break;

                    case CombineMode.Exclude:
                        Append = true;
                        Selection.ResetContinuation();
                        break;

                    case CombineMode.Replace:
                        Append = false;
                        Selection.Reset();
                        break;

                    case CombineMode.Intersect:
                        Append = true;
                        Selection.ResetContinuation();
                        break;

                    default:
                        throw new InvalidEnumArgumentException();
                }

                NewSelectionRenderer.Visible = newSelectionRendererVisible;
            }
        }

        protected virtual List<Point> TrimShapePath(List<Point> trimTheseTracePoints)
        {
            return trimTheseTracePoints;
        }

        protected virtual List<PointF> CreateShape(List<Point> inputTracePoints)
        {
            List<PointF> points = Utility.PointListToPointFList(inputTracePoints);
            return points;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (MoveOriginMode)
            {
                Size delta = new Size(e.X - LastPoint.X, e.Y - LastPoint.Y);
                
                for (int i = 0; i < TracePoints.Count; ++i)
                {
                    Point pt = TracePoints[i];
                    pt.X += delta.Width;
                    pt.Y += delta.Height;
                    TracePoints[i] = pt;
                }

                LastPoint = new Point(e.X, e.Y);
                Render();
            }
            else if (Tracking)
            {
                Point mouseXY = new Point(e.X, e.Y);

                if (mouseXY != TracePoints[TracePoints.Count - 1])
                {
                    TracePoints.Add(mouseXY);
                }
                
                HasMoved = true;
                Render();
            }
        }

        private PointF[] CreateSelectionPolygon()
        {
            List<Point> trimmedTrace = TrimShapePath(TracePoints);
            List<PointF> shapePoints = CreateShape(trimmedTrace);
            List<PointF> polygon;

            switch (SelectionMode)
            {
                case CombineMode.Xor:
                case CombineMode.Exclude:
                    polygon = shapePoints;
                    break;

                default:
                case CombineMode.Complement:
                case CombineMode.Intersect:
                case CombineMode.Replace:
                case CombineMode.Union:
                    polygon = Utility.SutherlandHodgman(DocumentWorkspace.Document.Bounds, shapePoints);
                    break;
            }

            return polygon.ToArray();
        }

        private void Render()
        {
            if (TracePoints != null && TracePoints.Count > 2)
            {
                PointF[] polygon = CreateSelectionPolygon();

                if (polygon.Length > 2)
                {
                    DocumentWorkspace.ResetOutlineWhiteOpacity();
                    NewSelectionRenderer.ResetOutlineWhiteOpacity();

                    Selection.SetContinuation(polygon, SelectionMode);

                    CombineMode cm;

                    if (SelectionMode == CombineMode.Replace)
                    {
                        cm = CombineMode.Replace;
                    }
                    else
                    {
                        cm = CombineMode.Xor;
                    }

                    NewSelection.SetContinuation(polygon, cm);

                    Update();
                }
            }
        }

        protected override void OnPulse()
        {
            if (Tracking)
            {
                DocumentWorkspace.ResetOutlineWhiteOpacity();
                NewSelectionRenderer.ResetOutlineWhiteOpacity();
            }

            base.OnPulse();
        }

        private enum WhatToDo
        {
            Clear,
            Emit,
            Reset,
        }

        private void Done()
        {
            if (Tracking)
            {
                // Truth table for what we should do based on three flags:
                //  append  | moved | tooQuick | result                             | optimized expression to yield true
                // ---------+-------+----------+-----------------------------------------------------------------------
                //     F    |   T   |    T     | clear selection                    | !append && (!moved || tooQuick)
                //     F    |   T   |    F     | emit new selected area             | !append && moved && !tooQuick
                //     F    |   F   |    T     | clear selection                    | !append && (!moved || tooQuick)
                //     F    |   F   |    F     | clear selection                    | !append && (!moved || tooQuick)
                //     T    |   T   |    T     | append to selection                | append && moved
                //     T    |   T   |    F     | append to selection                | append && moved
                //     T    |   F   |    T     | reset selection                    | append && !moved
                //     T    |   F   |    F     | reset selection                    | append && !moved
                //
                // append   --> If the user was holding control, then true. Else false.
                // moved    --> If they never moved the mouse, false. Else true.
                // tooQuick --> If they held the mouse button down for more than 50ms, false. Else true.
                //
                // "Clear selection" means to result in no selected area. If the selection area was previously empty,
                //    then no HistoryMemento is emitted. Otherwise a Deselect HistoryMemento is emitted.
                //
                // "Reset selection" means to reset the selected area to how it was before interaction with the tool,
                //    without a HistoryMemento.

                PointF[] polygon = CreateSelectionPolygon();
                HasMoved &= (polygon.Length > 1);

                // They were "too quick" if they weren't doing a selection for more than 50ms
                // This takes care of the case where someone wants to click to deselect, but accidentally moves
                // the mouse. This happens VERY frequently.
                bool tooQuick = Utility.TicksToMs((DateTime.Now - StartTime).Ticks) <= 50;

                // If their selection was completedly out of bounds, it will be clipped
                bool clipped = (polygon.Length == 0);

                // What the user drew had no effect on the selection, e.g. subtraction where there was nothing in the first place
                bool noEffect = false;

                WhatToDo whatToDo;

                // If their selection gets completely clipped (i.e. outside the image canvas),
                // then result in a no-op
                if (Append)
                {
                    if (!HasMoved || clipped || noEffect)
                    {   
                        whatToDo = WhatToDo.Reset;
                    }
                    else
                    {   
                        whatToDo = WhatToDo.Emit;
                    }
                }
                else
                {
                    if (HasMoved && !tooQuick && !clipped && !noEffect)
                    {   
                        whatToDo = WhatToDo.Emit;
                    }
                    else
                    {   
                        whatToDo = WhatToDo.Clear;
                    }
                }

                switch (whatToDo)
                {
                    case WhatToDo.Clear:
                        if (WasNotEmpty)
                        {
                            // emit a deselect history action
                            UndoAction.Name = DeselectFunction.StaticName;
                            UndoAction.Image = DeselectFunction.StaticImage;
                            HistoryStack.PushNewMemento(UndoAction);
                        }

                        Selection.Reset();
                        break;

                    case WhatToDo.Emit:
                        // emit newly selected area
                        UndoAction.Name = Name;
                        HistoryStack.PushNewMemento(UndoAction);
                        Selection.CommitContinuation();
                        break;

                    case WhatToDo.Reset:
                        // reset selection, no HistoryMemento
                        Selection.ResetContinuation();
                        break;
                }

                DocumentWorkspace.ResetOutlineWhiteOpacity();
                NewSelectionRenderer.ResetOutlineWhiteOpacity();
                NewSelection.Reset();
                NewSelectionRenderer.Visible = false;

                Tracking = false;

                DocumentWorkspace.EnableSelectionOutline = true;
                DocumentWorkspace.InvalidateSurface(Utility.RoundRectangle(DocumentWorkspace.VisibleDocumentRectangleF));
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            OnMouseMove(e);

            if (MoveOriginMode)
            {
                MoveOriginMode = false;
            }
            else
            {
                Done();
            }

            base.OnMouseUp(e);

            Cursor = GetCursor();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (Tracking)
            {
                Render();
            }

            Cursor = GetCursor();
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (Tracking)
            {
                Render();
            }

            Cursor = GetCursor();
        }

        protected override void OnClick()
        {
            base.OnClick();
            
            if (!MoveOriginMode)
            {
                Done();
            }
        }

        public SelectionTool(
            DocumentWorkspace documentWorkspace,
            ImageResource toolBarImage,
            string name,
            string helpText,
            char hotKey,
            ToolBarConfigItems toolBarConfigItems)
            : base(documentWorkspace,
                   toolBarImage,
                   name,
                   helpText,
                   hotKey,
                   false,
                   toolBarConfigItems | ToolBarConfigItems.SelectionCombineMode)
        {
            Tracking = false;
        }
    }
}
