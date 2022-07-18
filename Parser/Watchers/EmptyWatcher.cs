using System;

namespace Parser.Watchers
{
  internal struct EmptyWatcher : IWatcher
  {
    public int FindIndex(char current, int index)
    {
      return int.MinValue;
    }
  }
}