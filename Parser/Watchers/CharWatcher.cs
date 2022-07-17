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

    public int FindIndex(char current, int index, in char[] buffer, int bufferLength, in char[] overflow, int overflowLength)
    {
      return current == _identifier ? index : int.MinValue;
    }
  }
}