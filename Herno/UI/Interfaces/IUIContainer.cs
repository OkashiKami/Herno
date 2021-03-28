using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Herno.UI
{
  public interface IUIContainer : IUIComponent,  IDisposable
  {
    List<IUIComponent> Children { get; }
  }
}
