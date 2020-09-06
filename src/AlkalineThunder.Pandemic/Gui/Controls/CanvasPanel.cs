using System;
using AlkalineThunder.Pandemic.Gui.Markup;
using Microsoft.Xna.Framework;

namespace AlkalineThunder.Pandemic.Gui.Controls
{
    /// <summary>
    /// A control that allows it's children to be arbitrarily positioned and sized.  CanvasPanel is used
    /// as the root for the entire user interface.
    /// </summary>
    [MarkupElement("canvas")]
    public class CanvasPanel : ContainerControl
    {
        /// <summary>
        /// Represents the name of the attached Anchor property.
        /// </summary>
        public const string AnchorProperty = "Anchor";
        
        /// <summary>
        /// Represents the name of the attached autosize property.
        /// </summary>
        public const string AutoSizeProperty = "AutoSize";
        
        /// <summary>
        /// Represents the name of the attached position property.
        /// </summary>
        public const string PositionProperty = "Position";
        
        /// <summary>
        /// Represents the name of the attached fixed size property.
        /// </summary>
        public const string SizeProperty = "Size";
        
        /// <summary>
        /// Represents the name of the attached origin property.
        /// </summary>
        public const string OriginProperty = "Origin";
        
        /// <summary>
        /// Represents a canvas child's anchor points.
        /// </summary>
        public struct Anchor
        {
            /// <summary>
            /// The anchor point of the control's left edge.
            /// </summary>
            public float Left;
            
            /// <summary>
            /// The anchor point of the control's top edge.
            /// </summary>
            public float Top;
            
            /// <summary>
            /// The anchor point of the control's right edge.
            /// </summary>
            public float Right;
            
            /// <summary>
            /// The anchor point of the control's bottom edge.
            /// </summary>
            public float Bottom;

            /// <summary>
            /// Creates a new instance of the <see cref="Anchor"/> structure.
            /// </summary>
            /// <param name="left">The left edge's anchor point.</param>
            /// <param name="top">The top edge's anchor point.</param>
            /// <param name="right">The right edge's anchor point.</param>
            /// <param name="bottom">The bottom edge's anchor point.</param>
            public Anchor(float left, float top, float right, float bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            /// <summary>
            /// Represents a value that anchors a control to the top left corner of the canvas.
            /// </summary>
            public static readonly Anchor TopLeft = new Anchor(0, 0, 0, 0);
            /// <summary>
            /// Represents a value that anchors a control to the top middle of the canvas.
            /// </summary>
            public static readonly Anchor TopCenter = new Anchor(0.5f, 0, 0.5f, 0);
            
            /// <summary>
            /// Represents a value that anchors a control to the top right corner of the canvas.
            /// </summary>
            public static readonly Anchor TopRight = new Anchor(1, 0, 1, 0);
            
            /// <summary>
            /// Represents a value that anchors a control to the left of the canvas.
            /// </summary>
            public static readonly Anchor MiddleLeft = new Anchor(0, 0.5f, 0, 0.5f);
            
            /// <summary>
            /// Represents a value that anchors a control to the middle of a canvas.
            /// </summary>
            public static readonly Anchor Middle = new Anchor(0.5f, 0.5f, 0.5f, 0.5f);
            
            /// <summary>
            /// Represents a value that anchors a control to the right of the canvas.
            /// </summary>
            public static readonly Anchor MiddleRight = new Anchor(1, 0.5f, 1, 0.5f);
            
            /// <summary>
            /// Represents a value that anchors a control to the bottom left corner of the canvas.
            /// </summary>
            public static readonly Anchor BottomLeft = new Anchor(0, 1, 0, 1);
            
            /// <summary>
            /// Represents a value that anchors a control to the bottom middle of a canvas.
            /// </summary>
            public static readonly Anchor BottomCenter = new Anchor(0.5f, 1, 0.5f, 1);
            
            /// <summary>
            /// Represents a value that anchors a control to the bottom right corner of the canvas.
            /// </summary>
            public static readonly Anchor BottomRight = new Anchor(1, 1, 1, 1);

            /// <summary>
            /// Represents a value that spans a control vertically along the left of the canvas.
            /// </summary>
            public static readonly Anchor LeftSide = new Anchor(0, 0, 0, 1);
            
            /// <summary>
            /// Represents a value that spans a control vertically along the right of the canvas.
            /// </summary>
            public static readonly Anchor RightSide = new Anchor(1, 0, 1, 1);
            
            /// <summary>
            /// Represents a value that spans a control horizontally along the top of a canvas.
            /// </summary>
            public static readonly Anchor TopSide = new Anchor(0, 0, 1, 0);
            
            /// <summary>
            /// Represents a value that spans a control horizontally across the bottom edge of the canvas.
            /// </summary>
            public static readonly Anchor BottomSide = new Anchor(0, 1, 1, 1);

            /// <summary>
            /// Represents a value that spans a control horizontally through the middle of the canvas.
            /// </summary>
            public static readonly Anchor LeftToRight = new Anchor(0, 0.5f, 1, 0.5f);
            
            /// <summary>
            /// Represents a value that spans a control vertically through the middle of a canvas.
            /// </summary>
            public static readonly Anchor TopToBottom = new Anchor(0.5f, 0, 0.5f, 1);
            
            /// <summary>
            /// Represents a value that spans a control across the entire canvas.
            /// </summary>
            public static readonly Anchor Fill = new Anchor(0, 0, 1, 1);
        }

        /// <summary>
        /// Gets the origin value of the child control.
        /// </summary>
        /// <param name="child">Screw off, I'm not done setting my laptop up yet. Nobody can read this.</param>
        /// <returns>Home</returns>
        public Vector2 GetOrigin(Control child)
        {
            if (child != null && child.HasAttachedProperty<Vector2>(OriginProperty))
            {
                return child.GetAttachedProperty<Vector2>(OriginProperty);
            }

            return Vector2.Zero;
        }

        /// <summary>
        /// Sets the origin point of a control relative to the control's top-left corner.
        /// </summary>
        /// <param name="child">The control to set the value of.</param>
        /// <param name="origin">The origin point of the control.</param>
        public void SetOrigin(Control child, Vector2 origin)
        {
            child?.SetAttachedProperty(OriginProperty, origin);
        }
        
        /// <summary>
        /// Gets the anchor points of a control.
        /// </summary>
        /// <param name="child">The control to read the property from.</param>
        /// <returns>The control's anchor points.</returns>
        public Anchor GetAnchor(Control child)
        {
            if (child != null && child.HasAttachedProperty<Anchor>(AnchorProperty))
            {
                return child.GetAttachedProperty<Anchor>(AnchorProperty);
            }

            return Anchor.Fill;
        }

        /// <summary>
        /// Sets the anchor points of a control.
        /// </summary>
        /// <param name="child">The control to set the anchor points of.</param>
        /// <param name="anchor">The anchor points of the control.</param>
        public void SetAnchor(Control child, Anchor anchor)
        {
            child?.SetAttachedProperty(AnchorProperty, anchor);
        }
        
        /// <summary>
        /// Gets the position of a control.
        /// </summary>
        /// <param name="child">The control to get the position of.</param>
        /// <returns>The position of the control.</returns>
        public Vector2 GetPosition(Control child)
        {
            if (child != null && child.HasAttachedProperty<Vector2>(PositionProperty))
            {
                return child.GetAttachedProperty<Vector2>(PositionProperty);
            }

            return Vector2.Zero;
        }
        
        /// <summary>
        /// Gets the fixed size of a control.
        /// </summary>
        /// <param name="child">The control to get the fixed size of.</param>
        /// <returns>The fixed size of the control.</returns>
        public Vector2 GetSize(Control child)
        {
            if (child != null && child.HasAttachedProperty<Vector2>(SizeProperty))
            {
                return child.GetAttachedProperty<Vector2>(SizeProperty);
            }

            return Vector2.Zero;
        }

        /// <summary>
        /// Sets the position of a control.
        /// </summary>
        /// <param name="child">The control to set the position of.</param>
        /// <param name="position">The position of the control.</param>
        public void SetPosition(Control child, Vector2 position)
        {
            child?.SetAttachedProperty(PositionProperty, position);
        }
        
        /// <summary>
        /// Sets the size of a control.
        /// </summary>
        /// <param name="child">The control to resize.</param>
        /// <param name="size">The size of the control.</param>
        public void SetSize(Control child, Vector2 size)
        {
            child?.SetAttachedProperty(SizeProperty, size);
        }

        /// <summary>
        /// Gets a value indicating whether the given control should be auto-sized.
        /// </summary>
        /// <param name="child">The control to read the value from.</param>
        /// <returns>Whether or not the control should be auto-sized.</returns>
        public bool GetAutoSize(Control child)
        {
            if (child != null && child.HasAttachedProperty<bool>(AutoSizeProperty))
            {
                return child.GetAttachedProperty<bool>(AutoSizeProperty);
            }

            return false;
        }

        /// <summary>
        /// Sets a value indicating whether the control should be auto-sized.
        /// </summary>
        /// <param name="child">The control to auto-size.</param>
        /// <param name="autoSize">Whether the control should be auto-sized.</param>
        public void SetAutoSize(Control child, bool autoSize)
        {
            child?.SetAttachedProperty(AutoSizeProperty, autoSize);
        }

        /// <inheritdoc />
        protected override Vector2 MeasureOverride(Vector2 alottedSize)
        {
            var size = Vector2.Zero;

            foreach (var child in Children)
            {
                var measure = child.Measure();

                size.X = Math.Max(size.X, measure.X);
                size.Y = Math.Max(size.Y, measure.Y);
            }
            
            return size;
        }

        /// <inheritdoc />
        protected override void Arrange(Rectangle bounds)
        {
            foreach (var child in Children)
            {
                // have the layout system measure the child.
                var desiredSize = child.Measure();
                
                // get all the attached properties of the child that we need.
                var autoSize = GetAutoSize(child);
                var position = GetPosition(child);
                var fixedSize = GetSize(child);
                var anchor = GetAnchor(child);
                var origin = GetOrigin(child);
                
                // Calculate where the child's position anchor is located on our bounding box
                var anchorPosition = new Vector2(
                    bounds.Left + (bounds.Width * MathHelper.Clamp(anchor.Left, 0, 1)),
                    bounds.Top + (bounds.Height * MathHelper.Clamp(anchor.Top, 0, 1))
                );

                // Apply the top and left anchor to the position property
                var actualPosition = anchorPosition + position;

                // self explanatory, this is given to the layout system when computed.
                var layoutRect = new Rectangle();

                // initial layout pos is actualPosition, origin is applied later
                layoutRect.X = (int) actualPosition.X;
                layoutRect.Y = (int) actualPosition.Y;

                if (autoSize)
                {
                    // size is that reported by the layout system, bottom and right anchor is ignored.
                    layoutRect.Width = (int) desiredSize.X;
                    layoutRect.Height = (int) desiredSize.Y;
                }
                else
                {
                    // use the fixed size property for the child, anchored to this relative position.
                    var anchorSize = new Vector2(
                        bounds.Width * MathHelper.Clamp(anchor.Right, 0, 1),
                        bounds.Height * MathHelper.Clamp(anchor.Bottom, 0, 1)
                    );

                    fixedSize += anchorSize;

                    layoutRect.Width = (int) fixedSize.X;
                    layoutRect.Height = (int) fixedSize.Y;
                }

                // calculate the absolute position of the origin point on the child layout rect
                var absoluteOrigin = new Vector2(
                    layoutRect.Width * MathHelper.Clamp(origin.X, 0, 1),
                    layoutRect.Y * MathHelper.Clamp(origin.Y, 0, 1)
                );

                // apply the origin to the layout position
                layoutRect.X -= (int) absoluteOrigin.X;
                layoutRect.Y -= (int) absoluteOrigin.Y;

                // and bam, we're done.
                child.Layout(layoutRect);
            }
        }
    }
}