using System;

namespace Parser.Watchers
{
  internal interface IWatcher
  {
    int FindIndex(char current, int index, in char[] buffer, int bufferLength, in char[] overflow, int overflowLength);
  }
}