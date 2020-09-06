using System;
using System.Linq;
using AlkalineThunder.Pandemic.Gui.Markup;
using AlkalineThunder.Pandemic.Input;
using AlkalineThunder.Pandemic.Rendering;
using AlkalineThunder.Pandemic.Skinning;
using Microsoft.Xna.Framework;

namespace AlkalineThunder.Pandemic.Gui.Controls
{
    /// <summary>
    /// A simple selectable list of string values.
    /// </summary>
    [MarkupElement("select")]
    public sealed class SelectList : ItemsControl<string>
    {
        private FontStyle _itemFont = SkinFontStyle.ListItem;
        private int _sexyIndex = -1; // item hot-tracking.
        
        /// <summary>
        /// Gets or sets the font to be used for each item.
        /// </summary>
        public FontStyle ItemFont
        {
            get => _itemFont;
            set
            {
                if (_itemFont != value)
                {
                    if (value == null)
                        throw new ArgumentNullException(nameof(value));

                    _itemFont = value;
                    InvalidateMeasure();
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the text color of unselected items.
        /// </summary>
        public ControlColor ItemColor { get; set; } = ControlColor.Text;
        
        /// <summary>
        /// Gets or sets the text color of the selected item.
        /// </summary>
        public ControlColor SelectedItemColor { get; set; } = Color.White;
        
        /// <summary>
        /// Gets or sets the background color of the selected item.
        /// </summary>
        public ControlColor SelectedItemBackground { get; set; } = ControlColor.Primary;

        private int HitTestListItem(int x, int y)
        {
            var font = ItemFont.GetFont(this);
            var itemsRect = ContentRectangle;
            var itemY = itemsRect.Top;

            if (x >= itemsRect.Left && x <= itemsRect.Right)
            {
                var i = 0;
                foreach (var item in Items)
                {
                    var measure = font.MeasureString(item);
                    
                    if (y >= itemY && y <= itemY + measure.Y)
                        return i;

                    itemY += (int) measure.Y;
                    i++;
                }
            }

            return -1;
        }

        /// <inheritdoc />
        protected override bool OnMouseMove(MouseMoveEventArgs e)
        {
            _sexyIndex = HitTestListItem(e.X, e.Y);
            return base.OnMouseMove(e);
        }

        /// <inheritdoc />
        protected override bool OnMouseLeave(MouseMoveEventArgs e)
        {
            _sexyIndex = -1;
            return base.OnMouseLeave(e);
        }

        /// <inheritdoc />
        protected override bool OnClick(MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Left)
            {
                SelectedIndex = HitTestListItem(e.X, e.Y);
                return base.OnClick(e) || true;
            }
            
            return base.OnClick(e);
        }

        /// <inheritdoc />
        protected override Vector2 MeasureOverride(Vector2 alottedSize)
        {
            var font = ItemFont.GetFont(this);
            var measure = Vector2.Zero;

            foreach (var item in Items.Select(x => x.StripNewLines()))
            {
                var textMeasure = font.MeasureString(item);
                measure.X = Math.Max(measure.X, textMeasure.X);
                measure.Y += textMeasure.Y;
            }
            
            return measure;
        }

        /// <inheritdoc />
        protected override void OnPaint(SpriteRocket2D renderer)
        {
            var font = ItemFont.GetFont(this);
            var itemTextColor = ItemColor.GetColor(this);
            var itemActiveTextColor = SelectedItemColor.GetColor(this);
            var itemActiveHighlight = SelectedItemBackground.GetColor(this);
            var itemsRect = ContentRectangle;
            var y = itemsRect.Top;
            var i = 0;
            
            renderer.Begin();

            foreach (var item in Items.Select(x => x.StripNewLines()))
            {
                var m = font.MeasureString(item);
                
                if (i == SelectedIndex)
                {
                    renderer.FillRectangle(new Rectangle(itemsRect.Left, y, itemsRect.Width, (int) m.Y), itemActiveHighlight);
                    renderer.DrawString(font, item, new Vector2(itemsRect.Left, y), itemActiveTextColor);
                }
                else
                {
                    if (i == _sexyIndex)
                    {
                        renderer.FillRectangle(new Rectangle(itemsRect.Left, y, itemsRect.Width, (int) m.Y), itemActiveHighlight * 0.4f);
                    }
                    renderer.DrawString(font, item, new Vector2(itemsRect.Left, y), itemTextColor);
                }

                y += (int) m.Y;
                i++;
            }
            
            renderer.End();
        }
    }
}