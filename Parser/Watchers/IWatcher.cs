using System;

namespace Parser.Watchers
{
  internal interface IWatcher
  {
    int FindIndex(char current, int index);
  }
}