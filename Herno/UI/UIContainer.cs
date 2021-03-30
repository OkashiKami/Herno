using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace Herno.UI
{
    public abstract class UIContainer : IUIContainer
    {
        public string Name { get; set; }
        public List<IUIComponent> Children { get; } = new List<IUIComponent>();

        public UIContainer(IEnumerable<IUIComponent> children)
        {
            Children.AddRange(children);
        }

        public UIContainer() { }

        public abstract void Render(CommandList cl);

        protected void RenderChildren(CommandList cl)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].Render(cl);

            }
        }

        public virtual void Dispose()
        {
            foreach (var child in Children)
            {
                child.Dispose();
            }
        }
    }
}
