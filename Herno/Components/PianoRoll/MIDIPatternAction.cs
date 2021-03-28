using Herno.Components.Common;
using Herno.MIDI;
using Herno.Util;
using Herno.Util.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Herno.Components.PianoRoll
{
  public abstract class MIDIPatternAction : UserAction
  {
    public MIDIPatternConnect PianoRollPattern { get; }

    public MIDIPatternAction(MIDIPatternConnect pattern)
    {
      PianoRollPattern = pattern;
    }
  }
}
