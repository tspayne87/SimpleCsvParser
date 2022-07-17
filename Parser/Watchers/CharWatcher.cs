using System;

namespace Parser.Watchers
{
  internal class CharWatcher : IWatcher
  {
    private char _identifier;

    public CharWatcher(char identifier)
    {
      _identifier = identifier;
    }

    public int FindIndex(char current)
    {
      return current == _identifier ? index : int.MinValue;
    }
  }
}