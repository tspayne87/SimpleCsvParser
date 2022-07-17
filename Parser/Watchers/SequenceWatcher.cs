using System;
using SimpleCsvParser;

namespace Parser.Watchers
{
  internal class SequenceWatcher : IWatcher
  {
    private char _firstIdentifier;
    private char _lastIdentifier;
    private int _checkOverflow;
    private char[] _identifier;

    public SequenceWatcher(string identifier)
    {
      _firstIdentifier = identifier[0];
      _lastIdentifier = identifier[identifier.Length - 1];
      _checkOverflow = identifier.Length - 1;
      _identifier = identifier.ToCharArray();
    }

    public int FindIndex(char current, int index, in char[] buffer, int bufferLength, in char[] overflow, int overflowLength)
    {
      if (overflowLength == 0 && index < _checkOverflow) return int.MinValue;

      var first = _checkOverflow > index ? overflow[overflowLength - _checkOverflow - index] : buffer[index - _checkOverflow];

      /// We need to short circut out if the first and last identifier are not the same
      if (first != _firstIdentifier || current != _lastIdentifier) return int.MinValue;
      if (_identifier.Length == 2) return index - 1;


      if (_checkOverflow > index) {
        /// If we are dealing with multiple buffers to check if this sequence might be the sequence we are looking for
        if (_identifier.EqualsBetweenCharArray(buffer, index, overflow, overflowLength + index - _checkOverflow)) {
          return index - _checkOverflow;
        }
      } else {
        /// If we are dealing with buffer and nothing but the buffer
        if (_identifier.EqualsCharArray(buffer, index - _checkOverflow, index)) {
          return index - _checkOverflow;
        }
      }
      return int.MinValue;
    }
  }
}