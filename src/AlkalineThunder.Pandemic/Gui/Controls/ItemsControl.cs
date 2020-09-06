using System;
using System.Collections.Generic;

namespace AlkalineThunder.Pandemic.Gui.Controls
{
    /// <summary>
    /// Provides the base functionality for all user interface elements that can contain and display a list of values.
    /// </summary>
    /// <typeparam name="T">The type of value for each item in the control.</typeparam>
    public abstract class ItemsControl<T> : Control
    {
        private readonly List<T> _items = new List<T>();
        private int _selected = -1;
        
        /// <summary>
        /// Gets a collection of all items in the control.
        /// </summary>
        public IEnumerable<T> Items => _items;
        
        /// <summary>
        /// Gets a value indicating the number of items in the control.
        /// </summary>
        public int Count => _items.Count;
        
        /// <summary>
        /// Gets the value of the currently selected item.
        /// </summary>
        public T SelectedItem => (_selected >= 0 && _selected < _items.Count) ? _items[_selected] : default(T);
        
        /// <summary>
        /// Gets or sets the index of the selected item.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">When changing the selected index, the provided index is below -1 or outside of the bounds of the items list.</exception>
        public int SelectedIndex
        {
            get => _selected;
            set
            {
                if (_selected != value)
                {
                    if (value >= -1 && value < _items.Count)
                    {
                        _selected = value;
                        OnSelectedItemChanged(value);
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(nameof(value));
                    }
                }
            }
        }

        /// <summary>
        /// Occurs when the selected item of the control is changed either by the user or programmatically.
        /// </summary>
        public event EventHandler SelectedIndexChanged;

        /// <summary>
        /// Called when an item is added to the control.
        /// </summary>
        /// <param name="item">The item added to the control.</param>
        protected virtual void OnItemAdded(T item) {}
        
        /// <summary>
        /// Called when an item is removed from the control.
        /// </summary>
        /// <param name="item">The item removed from the control.</param>
        protected virtual void OnItemRemoved(T item) {}

        /// <summary>
        /// Called when the selected item is changed.
        /// </summary>
        /// <param name="index">The index of the newly selected item.</param>
        protected virtual void OnSelectedItemChanged(int index)
        {
            SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
        }
        
        /// <summary>
        /// Determines whether the control contains the given item.
        /// </summary>
        /// <param name="item">The value of the item to find.</param>
        /// <returns>A value indicating whether the item was found.</returns>
        public bool Contains(T item)
        {
            return _items.Contains(item);
        }

        /// <summary>
        /// Adds the given item to the control.
        /// </summary>
        /// <param name="item">The value of the item to add.</param>
        /// <exception cref="ArgumentNullException">The given item value is null.</exception>
        /// <exception cref="InvalidOperationException">An attempt was made to add a duplicate item to the control.</exception>
        public void Add(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            
            if (Contains(item))
                throw new InvalidOperationException("Cannot add duplicate item.");
            
            _items.Add(item);
            OnItemAdded(item);
            InvalidateMeasure();
            SelectedIndex = -1;
        }

        /// <summary>
        /// Removes the given item from the control.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>True if the item was removed, false if the control didn't contain the item.</returns>
        public bool Remove(T item)
        {
            if (item == null)
                return false;

            if (!Contains(item))
                return false;

            _items.Remove(item);
            SelectedIndex = -1;
            OnItemRemoved(item);
            InvalidateMeasure();
            return true;
        }

        /// <summary>
        /// Removes all items from the control.
        /// </summary>
        public void Clear()
        {
            _items.Clear();
            SelectedIndex = -1;
            InvalidateMeasure();
        }

        /// <summary>
        /// Finds the index of the given item.
        /// </summary>
        /// <param name="item">The value of the item to find.</param>
        /// <returns>The index of the item if it was found, or -1 if the control doesn't contain the item.</returns>
        public int Find(T item)
        {
            if (Contains(item))
                return _items.IndexOf(item);
            return -1;
        }
    }
}