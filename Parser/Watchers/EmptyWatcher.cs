using System;

namespace Parser.Watchers
{
  /// <summary>
  /// Impementation of an empty <see cref="IWatcher"/, that will always return a minimum integer
  /// </summary>
  internal struct EmptyWatcher : IWatcher
  {
    /// <inheritdoc />
    public int FindIndex(char current, int index)
    {
      return int.MinValue;
    }
  }
}