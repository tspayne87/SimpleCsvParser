using System;

namespace Parser.Watchers
{
  internal class DualWatcher : IWatcher
  {
    private char _id1;
    private char _id2;

    private char _prev1;

    public CharWatcher(string identifier)
    {
      _id1 = identifier[0];
      _id2 = identifier[1];
    }

    public int FindIndex(char current, int index)
    {
      var result = int.MinValue;
      if (_prev1 == default) return result;
      if (_prev1 == _id1 && current == _id2)
        result = index - 1;

      _prev1 = current;
      return result;
    }
  }
}