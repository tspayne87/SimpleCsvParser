namespace Parser.Watchers
{
  internal static class StringExtensions
  {
    public static IWatcher ToWatcher(this string str)
    {
      if (string.IsNullOrEmpty(str)) return new EmptyWatcher();
      return str.Length == 1 ? new CharWatcher(str[0]) : new SequenceWatcher(str);
    }

    public static IWatcher ToWatcher(this char? c)
    {
      return c != null ? new CharWatcher(c.Value) : new EmptyWatcher();
    }
  }
}