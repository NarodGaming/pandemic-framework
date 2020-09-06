using System.Collections.Generic;
using System.Linq;

namespace AlkalineThunder.Pandemic.Gui.Controls
{
    /// <summary>
    /// Provides the base functionality for all user interface elements that can contain multiple children.
    /// </summary>
    public abstract class ContainerControl : Control
    {
        /// <summary>
        /// Gets a list of all children in the container.
        /// </summary>
        public virtual IEnumerable<Control> Children => InternalChildren;
        
        /// <summary>
        /// Adds a child to the container.
        /// </summary>
        /// <param name="child">The child to add to the container.</param>
        public void AddChild(Control child)
        {
            InternalChildren.Add(child);
        }

        /// <summary>
        /// Checks whether a child is inside this container.
        /// </summary>
        /// <param name="child">The child control to look for.</param>
        /// <returns>Whether the child is, in fact, inside this container.</returns>
        /// <remarks>
        /// When Stanley approached a set of two open doors, Stanley walked through the
        /// one on his left.
        /// </remarks>
        public bool Contains(Control child)
        {
            return InternalChildren.Contains(child);
        }

        /// <summary>
        /// Removes a child from the container.
        /// </summary>
        /// <param name="child">The child to remove.</param>
        public void RemoveChild(Control child)
        {
            InternalChildren.Remove(child);
        }
        
        /// <summary>
        /// Removes all children from the container.
        /// </summary>
        public void Clear()
        {
            while (Children.Any())
            {
                var child = Children.First();
                RemoveChild(child);
            }
        }
    }
}