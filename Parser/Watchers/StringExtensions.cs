using System;

namespace Parser.Watchers
{
  internal static class StringExtensions
  {

    /// <summary>
    /// Creates a watcher out of a string expression
    /// </summary>
    /// <param name="str">The String that we want to build out a watcher from</param>
    /// <returns>Will return watcher for finding chars in a string expression</returns>
    /// <exception cref="IndexOutOfRangeException">Will return an out of range exception if we see a string with more than 3 characters</exception>
    public static IWatcher ToWatcher(this string str)
    {
      switch (str.Length)
      {
        case 0: return new EmptyWatcher();
        case 1: return new CharWatcher(str[0]);
        case 2: return new DualWatcher(str);
        default: throw new IndexOutOfRangeException("Could not create a watcher string can only have a max of 2 characters");
      }
    }

    public static IWatcher ToWatcher(this char? c)
    {
      return c != null ? new CharWatcher(c.Value) : new EmptyWatcher();
    }
  }
}