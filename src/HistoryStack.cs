/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.HistoryMementos;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// The HistoryStack class for the History "concept".  
    /// Serves as the undo and redo stacks.  
    /// </summary>
    [Serializable]
    internal class HistoryStack
    {
        private DocumentWorkspace documentWorkspace;
        private int stepGroupDepth;
        private int isExecutingMemento = 0; // 0 -> false, >0 -> true

        public bool IsExecutingMemento
        {
            get
            {
                return this.isExecutingMemento > 0;
            }
        }

        private void PushExecutingMemento()
        {
            ++this.isExecutingMemento;
        }

        private void PopExecutingMemento()
        {
            --this.isExecutingMemento;
        }

        public List<HistoryMemento> UndoStack { get; private set; }

        public List<HistoryMemento> RedoStack { get; private set; }

        public void BeginStepGroup()
        {
            ++this.stepGroupDepth;
        }

        public void EndStepGroup()
        {
            --this.stepGroupDepth;

            if (this.stepGroupDepth == 0)
            {
                OnFinishedStepGroup();
            }
        }

        public event EventHandler FinishedStepGroup;
        protected void OnFinishedStepGroup()
        {
            FinishedStepGroup?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler SteppedBackward;
        protected void OnSteppedBackward()
        {
            SteppedBackward?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler SteppedForward;
        protected void OnSteppedForward()
        {
            SteppedForward?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event handler for when a new history memento has been added.
        /// </summary>
        public event EventHandler NewHistoryMemento;
        protected void OnNewHistoryMemento()
        {
            NewHistoryMemento?.Invoke(this, EventArgs.Empty);
        }
                
        /// <summary>
        /// Event handler for when changes have been made to the history.
        /// </summary>
        public event EventHandler Changed;
        protected void OnChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Changing;
        protected void OnChanging()
        {
            Changing?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler HistoryFlushed;
        protected void OnHistoryFlushed()
        {
            HistoryFlushed?.Invoke(this, EventArgs.Empty);
        }

        public event ExecutingHistoryMementoEventHandler ExecutingHistoryMemento;
        protected void OnExecutingHistoryMemento(ExecutingHistoryMementoEventArgs e)
        {
            ExecutingHistoryMemento?.Invoke(this, e);
        }

        public event ExecutedHistoryMementoEventHandler ExecutedHistoryMemento;
        protected void OnExecutedHistoryMemento(ExecutedHistoryMementoEventArgs e)
        {
            ExecutedHistoryMemento?.Invoke(this, e);
        }

        public void PerformChanged()
        {
            OnChanged();
        }

        public HistoryStack(DocumentWorkspace documentWorkspace)
        {
            this.documentWorkspace = documentWorkspace;
            UndoStack = new List<HistoryMemento>();
            RedoStack = new List<HistoryMemento>();
        }

        private HistoryStack(
            List<HistoryMemento> undoStack,
            List<HistoryMemento> redoStack)
        {
            this.UndoStack = new List<HistoryMemento>(undoStack);
            this.RedoStack = new List<HistoryMemento>(redoStack);
        }

        /// <summary>
        /// When the user does something new, it will clear out the redo stack.
        /// </summary>
        public void PushNewMemento(HistoryMemento value)
        {
            Utility.GCFullCollect();

            OnChanging();

            ClearRedoStack();
            UndoStack.Add(value);
            OnNewHistoryMemento();

            OnChanged();

            value.Flush();
            Utility.GCFullCollect();
        }

        /// <summary>
        /// Takes one item from the redo stack, "redoes" it, then places the redo
        /// memento object to the top of the undo stack.
        /// </summary>
        public void StepForward()
        {
            PushExecutingMemento();

            try
            {
                StepForwardImpl();
            }

            finally
            {
                PopExecutingMemento();
            }
        }

        private void StepForwardImpl()
        {
            HistoryMemento topMemento = RedoStack[0];
            ToolHistoryMemento asToolHistoryMemento = topMemento as ToolHistoryMemento;

            if (asToolHistoryMemento != null && asToolHistoryMemento.ToolType != this.documentWorkspace.GetToolType())
            {
                this.documentWorkspace.SetToolFromType(asToolHistoryMemento.ToolType);
                StepForward();
            }
            else
            {
                OnChanging();

                ExecutingHistoryMementoEventArgs ehaea1 = new ExecutingHistoryMementoEventArgs(topMemento, true, false);

                if (asToolHistoryMemento == null && topMemento.SeriesGuid != Guid.Empty)
                {
                    ehaea1.SuspendTool = true;
                }

                OnExecutingHistoryMemento(ehaea1);

                if (ehaea1.SuspendTool)
                {
                    this.documentWorkspace.PushNullTool();
                }
            
                HistoryMemento redoMemento = RedoStack[0];

                // Possibly useful invariant here:
                //     ehaea1.HistoryMemento.SeriesGuid == ehaea2.HistoryMemento.SeriesGuid == ehaea3.HistoryMemento.SeriesGuid
                ExecutingHistoryMementoEventArgs ehaea2 = new ExecutingHistoryMementoEventArgs(redoMemento, false, ehaea1.SuspendTool);
                OnExecutingHistoryMemento(ehaea2);

                HistoryMemento undoMemento = redoMemento.PerformUndo();
            
                RedoStack.RemoveAt(0);
                UndoStack.Add(undoMemento);

                ExecutedHistoryMementoEventArgs ehaea3 = new ExecutedHistoryMementoEventArgs(undoMemento);
                OnExecutedHistoryMemento(ehaea3);

                OnChanged();
                OnSteppedForward();

                undoMemento.Flush();

                if (ehaea1.SuspendTool)
                {
                    this.documentWorkspace.PopNullTool();
                }       
            }

            if (this.stepGroupDepth == 0)
            {
                OnFinishedStepGroup();
            }
        }

        /// <summary>
        /// Undoes the top of the undo stack, then places the redo memento object to the
        /// top of the redo stack.
        /// </summary>
        public void StepBackward()
        {
            PushExecutingMemento();

            try
            {
                StepBackwardImpl();
            }

            finally
            {
                PopExecutingMemento();
            }
        }

        private void StepBackwardImpl()
        {
            HistoryMemento topMemento = UndoStack[UndoStack.Count - 1];
            ToolHistoryMemento asToolHistoryMemento = topMemento as ToolHistoryMemento;

            if (asToolHistoryMemento != null && asToolHistoryMemento.ToolType != this.documentWorkspace.GetToolType())
            {
                this.documentWorkspace.SetToolFromType(asToolHistoryMemento.ToolType);
                StepBackward();
            }
            else
            {
                OnChanging();

                ExecutingHistoryMementoEventArgs ehaea1 = new ExecutingHistoryMementoEventArgs(topMemento, true, false);

                if (asToolHistoryMemento == null && topMemento.SeriesGuid == Guid.Empty)
                {
                    ehaea1.SuspendTool = true;
                }

                OnExecutingHistoryMemento(ehaea1);

                if (ehaea1.SuspendTool)
                {
                    this.documentWorkspace.PushNullTool();
                }

                HistoryMemento undoMemento = UndoStack[UndoStack.Count - 1];

                ExecutingHistoryMementoEventArgs ehaea2 = new ExecutingHistoryMementoEventArgs(undoMemento, false, ehaea1.SuspendTool);
                OnExecutingHistoryMemento(ehaea2);

                HistoryMemento redoMemento = UndoStack[UndoStack.Count - 1].PerformUndo();
                UndoStack.RemoveAt(UndoStack.Count - 1);
                RedoStack.Insert(0, redoMemento);

                // Possibly useful invariant here:
                //     ehaea1.HistoryMemento.SeriesGuid == ehaea2.HistoryMemento.SeriesGuid == ehaea3.HistoryMemento.SeriesGuid
                ExecutedHistoryMementoEventArgs ehaea3 = new ExecutedHistoryMementoEventArgs(redoMemento);
                OnExecutedHistoryMemento(ehaea3);

                OnChanged();
                OnSteppedBackward();

                redoMemento.Flush();

                if (ehaea1.SuspendTool)
                {
                    this.documentWorkspace.PopNullTool();
                }
            }

            if (this.stepGroupDepth == 0)
            {
                OnFinishedStepGroup();
            }
        }

        public void ClearAll()
        {
            OnChanging();

            foreach (HistoryMemento ha in UndoStack)
            {
                ha.Flush();
            }

            foreach (HistoryMemento ha in RedoStack)
            {
                ha.Flush();
            }

            UndoStack = new List<HistoryMemento>();
            RedoStack = new List<HistoryMemento>();
            OnChanged();
            OnHistoryFlushed();
        }

        public void ClearRedoStack()
        {
            foreach (HistoryMemento ha in RedoStack)
            {
                ha.Flush();
            }

            OnChanging();
            RedoStack = new List<HistoryMemento>();
            OnChanged();
        }
    }
}
