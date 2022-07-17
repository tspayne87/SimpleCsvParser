namespace Parser.Watchers
{
  internal static class StringExtensions
  {
    public static IWatcher ToWatcher(this string str)
    {
      switch (str.Length)
      {
        case 0: return new EmptyWatcher();
        case 1: return new CharWatcher(str[0]);
        case 2: return new DualWatcher(str);
        default: throw new ArgumentException("");
      }
    }

    public static IWatcher ToWatcher(this char? c)
    {
      return c != null ? new CharWatcher(c.Value) : new EmptyWatcher();
    }
  }
}