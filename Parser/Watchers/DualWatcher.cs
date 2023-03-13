namespace Parser.Watchers
{
  /// <summary>
  /// Implementation of a <see cref="IWatcher" /> for watching two characters at once
  /// </summary>
  internal struct DualWatcher : IWatcher
  {
    /// <summary>
    /// The previous character we should check against
    /// </summary>
    private char _id1;

    /// <summary>
    /// The leading character we should check against
    /// </summary>
    private char _id2;

    /// <summary>
    /// The previous value we saw for the last value
    /// </summary>
    private char _prev1;

    /// <summary>
    /// Constructor to build out the dual watcher
    /// </summary>
    /// <param name="identifier">The string identifier to extract the strings out of</param>
    public DualWatcher(string identifier)
    {
      _id1 = identifier[0];
      _id2 = identifier[1];

      _prev1 = default;
    }

    /// <inheritdoc />
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