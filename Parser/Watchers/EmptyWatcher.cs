using System;

namespace Parser.Watchers
{
  internal class EmptyWatcher : IWatcher
  {
    public int FindIndex(char current)
    {
      return int.MinValue;
    }
  }
}