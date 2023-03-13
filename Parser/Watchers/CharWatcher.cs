namespace Parser.Watchers
{
  /// <summary>
  /// Implementation of the of a <see cref="IWatcher" /> that will track a single character
  /// </summary>
  internal struct CharWatcher : IWatcher
  {
    /// <summary>
    /// The character that we are searching for
    /// </summary>
    private char _identifier;

    /// <summary>
    /// Constructor build out a watcher to look for a specific character
    /// </summary>
    /// <param name="identifier">The character that we are searching for</param>
    public CharWatcher(char identifier)
    {
      _identifier = identifier;
    }

    /// <inheritdoc />
    public int FindIndex(char current, int index)
    {
      if (current == _identifier)
        return index;
      return int.MinValue;
    }
  }
}