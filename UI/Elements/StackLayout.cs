using McMikeEngine.Mathematics;
using McMikeEngine.Mathematics._2D;
using McMikeEngine.Objects;
using McMikeEngine.Utils;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace McMikeEngine.UI.Layouts
{
    /// <summary>
    /// Changes child Transform.Position to stack in list with other elements
    /// </summary>
    public class StackLayout : Layout
    {
        private List<UIElement> mElements = new List<UIElement>();

        private object mElemLock = new object();

        public int Count => mElements.Count;

        [XmlAttribute]
        public bool IsVertical { get; set; }

        [XmlAttribute]
        public float Spacing { get; set; }

        public StackLayout()
        {
            IsVertical = true;
            Padding = new Mathematics._2D.Rect(0);
        }

        public StackLayout(StackLayout stack) : base(stack)
        {
            IsVertical = stack.IsVertical;
            Spacing = stack.Spacing;
            Padding = stack.Padding.Copy();
        }

        public override void Add(GameObject obj)
        {
            base.Add(obj);

            var ui = obj as UIElement;

            if (ui == null)
                return;

            //Remove from mElements
            lock (mElemLock)
            {
                if (ui.Parent == this)
                {
                    mElements.Add(ui);

                    RefreshLayout();
                    Refresh();
                }
            }
        }

        public override void Remove(GameObject obj)
        {
            base.Remove(obj);

            var ui = obj as UIElement;

            if (ui == null)
                return;

            //Remove from mElements
            lock (mElemLock)
            {
                mElements.Remove(ui);

                RefreshLayout();
                Refresh();
            }
        }

        public virtual void Insert(UIElement ui, int index)
        {
            base.Add(ui);

            lock (mElemLock)
            {
                if (index < mElements.Count)
                    mElements.Insert(index, ui);

                else
                    mElements.Add(ui);
            }

            RefreshLayout();
            Refresh();
        }

        public void Move(UIElement ui, int newIndex)
        {
            lock (mElemLock)
            {
                mElements.Remove(ui);
                mElements.Insert(newIndex, ui);
            }

            RefreshLayout();
            Refresh();
        }

        public override bool Contains(UIElement ui)
        {
            lock (mElemLock)
            {
                return mElements.Contains(ui);
            }
        }

        public int GetIndexOf(UIElement ui)
        {
            if (!mElements.Contains(ui))
                return -1;

            return mElements.IndexOf(ui);
        }

        /// <summary>
        /// Converts relative position to index in stack
        /// 
        /// </summary>
        /// <param name="y">Position relative to the StackLayout origin</param>
        /// <returns>Index [0,Count)</returns>
        public int GetIndexAt(float y)
        {
            lock (mElemLock)
            {
                //Step from top through elements
                for (int i = 0; i < Count; i++)
                {
                    //Find the bottom of the element
                    var elem = mElements[i];

                    var bottom = elem.Position.Y + elem.Height;

                    if (y <= bottom)
                        return i;
                }

                return Math.Max(Count - 1, 0);
            }
        }

        public UIElement GetElementAt(int index)
        {
            return mElements[index];
        }

        public override void CalculateSizeUp()
        {
            //Bottom-up (children first)
            foreach (var child in UIChildren)
            {
                if (!child.IsEnabled)
                    continue;

                child.CalculateSizeUp();
            }

            //Width
            if (WidthMode == SizeMode.WRAP_CONTENT)
            {
                Width = Padding.Left;

                if (IsVertical)
                {
                    //Find max width
                    float maxChild = 0;

                    lock (mElemLock)
                    {
                        foreach (var child in mElements)
                        {
                            if (!child.IsEnabled)
                                continue;

                            maxChild = Math.Max(maxChild, child.Width);
                        }
                    }

                    Width += maxChild;
                }
                else
                {
                    //Sum widths
                    lock (mElemLock)
                    {
                        foreach (var child in mElements)
                        {
                            if (!child.IsEnabled)
                                continue;

                            Width += child.Margin.Left + child.Width + child.Margin.Right + Spacing;
                        }
                    }

                    Width -= Spacing;
                }

                Width += Padding.Right;
            }

            //Height
            if (HeightMode == SizeMode.WRAP_CONTENT)
            {
                Height = Padding.Top;

                if (IsVertical)
                {
                    lock (mElemLock)
                    {
                        //Sum heights
                        foreach (var child in mElements)
                        {
                            if (!child.IsEnabled)
                                continue;

                            Height += child.Margin.Top + child.Height + child.Margin.Bottom + Spacing;
                        }
                    }

                    Height -= Spacing;
                }

                else
                {
                    //Find max child height
                    float maxChild = 0;

                    lock (mElemLock)
                    {
                        foreach (var child in mElements)
                        {
                            if (!child.IsEnabled)
                                continue;

                            maxChild = Math.Max(maxChild, child.Height);
                        }
                    }

                    Height += maxChild;
                }

                Height += Padding.Bottom;
            }
        }

        public override void CalculateSizeDown()
        {
            //Set child non-primary sizes
            foreach (var child in UIChildren)
            {
                if (!child.IsEnabled)
                    continue;

                //Widths for vertical stack
                if (IsVertical && child.WidthMode == SizeMode.MATCH_PARENT)
                {
                    if (HorizontalAlignment == HorizontalAlignment.NONE)
                        child.Width = Size.X;
                    else
                        child.Width = Size.X - Padding.Left - Padding.Right - child.Margin.Left - child.Margin.Right;
                }

                //Heights for horizontal stack
                else if (!IsVertical && child.HeightMode == SizeMode.MATCH_PARENT)
                {
                    if (VerticalAlignment == VerticalAlignment.NONE)
                        child.Height = Size.Y;
                    else
                        child.Height = Size.Y - Padding.Top - Padding.Bottom - child.Margin.Top - child.Margin.Bottom;
                }
            }

            //Calculate size of weighted children
            //Sum free size and total weight
            var free = Size.Copy();

            //Subtract padding
            free.X -= (Padding.Left + Padding.Right);
            free.Y -= (Padding.Top + Padding.Bottom);

            float totalWeight = 0;

            lock (mElemLock)
            {
                foreach (var child in mElements)
                {
                    if (IsVertical)
                    {
                        //Child height is RIGID or WRAP_CONTENT
                        if (child.HeightMode != SizeMode.WEIGHTED && child.HeightMode != SizeMode.MATCH_PARENT)
                            free.Y -= child.Height;

                        //Child height is MATCH_PARENT or WEIGHTED
                        else
                            totalWeight += child.Weight;

                        free.Y -= (child.Margin.Top + child.Margin.Bottom + Spacing);
                    }
                    else
                    {
                        //Child width is RIGID or WRAP_CONTENT
                        if (child.WidthMode != SizeMode.WEIGHTED && child.WidthMode != SizeMode.MATCH_PARENT)
                            free.X -= child.Width;

                        //Child width is MATCH_PARENT or WEIGHTED
                        else
                            totalWeight += child.Weight;

                        free.X -= (child.Margin.Left + child.Margin.Right + Spacing);
                    }
                }

                if (mElements.Count > 0 && IsVertical)
                    free.Y += Spacing;
                else
                    free.X += Spacing;

                //Assign child sizes by weight
                foreach (var child in mElements)
                {
                    if (IsVertical)
                    {
                        //Treat MATCH_PARENT like WEIGHTED
                        if (child.HeightMode == SizeMode.WEIGHTED || child.HeightMode == SizeMode.MATCH_PARENT)
                            child.Height = (int)(free.Y * child.Weight / totalWeight);
                    }
                    else
                    {
                        //Treat MATCH_PARENT like WEIGHTED
                        if (child.WidthMode == SizeMode.WEIGHTED || child.WidthMode == SizeMode.MATCH_PARENT)
                            child.Width = (int)(free.X * child.Weight / totalWeight);
                    }
                }
            }

            //Top-down (parents first)
            foreach (var child in UIChildren)
                child.CalculateSizeDown();
        }

        public override void Organize()
        {
            lock (mElemLock)
            {
                foreach (var child in mElements)
                {
                    child.Organize();
                }
            }

            if (IsVertical)
                OnOrganizeVertical();
            else
                OnOrganizeHorizontal();
        }

        private void OnOrganizeVertical()
        {
            float y = Padding.Top;

            lock (mElemLock)
            {
                foreach (var ui in mElements)
                {
                    //Bottom-up (children first)
                    ui.Organize();

                    if (ui.HorizontalAlignment == HorizontalAlignment.NONE)
                        continue;

                    //Vertical position
                    y += ui.Margin.Top;

                    ui.Transform.Position.Y = y;

                    y += ui.Transform.Scaling.Y * ui.Height + ui.Margin.Bottom + Spacing;

                    //Horizontal position
                    switch (ui.HorizontalAlignment)
                    {
                        case HorizontalAlignment.NONE:

                            break;

                        case HorizontalAlignment.LEFT:
                            ui.Transform.Position.X = Padding.Left + ui.Margin.Left;
                            break;

                        case HorizontalAlignment.CENTER:
                            ui.Transform.Position.X = Width / 2f - ui.Width / 2f;
                            break;

                        case HorizontalAlignment.RIGHT:
                            ui.Transform.Position.X = Width - ui.Width - Padding.Right - ui.Margin.Right;
                            break;
                    }
                }
            }
        }

        private void OnOrganizeHorizontal()
        {
            float x = Padding.Left;

            lock (mElemLock)
            {
                foreach (var child in mElements)
                {
                    //Bottom-up (children first)
                    child.Organize();

                    if (child.VerticalAlignment == VerticalAlignment.NONE)
                        continue;

                    //Horizontal position
                    x += child.Margin.Left;

                    child.Transform.Position.X = x;

                    x += child.Transform.Scaling.X * child.Width + child.Margin.Right + Spacing;

                    //Vertical position
                    switch (child.VerticalAlignment)
                    {
                        case VerticalAlignment.NONE:

                            break;

                        case VerticalAlignment.TOP:
                            child.Transform.Position.Y = Padding.Top + child.Margin.Top;
                            break;

                        case VerticalAlignment.CENTER:
                            child.Transform.Position.Y = Height / 2f - child.Height / 2f;
                            break;

                        case VerticalAlignment.BOTTOM:
                            child.Transform.Position.Y = Height - child.Height - Padding.Bottom - child.Margin.Bottom;
                            break;
                    }
                }
            }
        }

        public override void ClearChildren()
        {
            base.ClearChildren();

            lock (mElemLock)
            {
                mElements.Clear();
            }
        }
    }
}
