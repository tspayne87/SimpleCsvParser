using System;
using System.Runtime.CompilerServices;

namespace Parser.Watchers
{
  internal struct DualWatcher : IWatcher
  {
    private char _id1;
    private char _id2;

    private char _prev1;

    public DualWatcher(string identifier)
    {
      _id1 = identifier[0];
      _id2 = identifier[1];

      _prev1 = default;
    }

    public int FindIndex(char current, int index)
    {
      var result = int.MinValue;
      if (_prev1 == _id1 && current == _id2 )
        result = index - 1;

      _prev1 = current;
      return result;
    }
  }
}