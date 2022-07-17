using System;

namespace Parser.Watchers
{
  internal class EmptyWatcher : IWatcher
  {
    public int FindIndex(char current, int index, in char[] buffer, int bufferLength, in char[] overflow, int overflowLength)
    {
      return int.MinValue;
    }
  }
}