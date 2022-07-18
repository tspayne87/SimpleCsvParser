using System;
using System.Runtime.CompilerServices;

namespace Parser.Watchers
{
  internal struct CharWatcher : IWatcher
  {
    private char _identifier;

    public CharWatcher(char identifier)
    {
      _identifier = identifier;
    }

    public int FindIndex(char current, int index)
    {
      return current == _identifier ? index : int.MinValue;
    }
  }
}