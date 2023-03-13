namespace Parser.Watchers
{
  /// <summary>
  /// Watcher interface to pass different stream character watchers off to an abstracted layer
  /// </summary>
  internal interface IWatcher
  {
    /// <summary>
    /// Will find the current index based on if we have a match, this will most always be the current
    /// index but has a chance to being a different index based on the type of watcher
    /// </summary>
    /// <param name="current">The current character we are checking against</param>
    /// <param name="index">The current index we are checking</param>
    /// <returns>Will return an index of the position or the minimum value if no index was found</returns>
    int FindIndex(char current, int index);
  }
}