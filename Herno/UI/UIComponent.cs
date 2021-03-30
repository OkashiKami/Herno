using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace Herno.UI
{
    public abstract class UIComponent : IUIComponent
    {
        public string Name { get; set; } = "UI Component";
        public virtual void Dispose()
        { }

        public abstract void Render(CommandList cl);
    }
}
