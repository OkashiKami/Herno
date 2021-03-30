using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace Herno.UI
{
    public interface IUIComponent : IDisposable
    {
        public string Name { get; set; }
        void Render(CommandList cl);
        new void Dispose()
        { }
    }
}
