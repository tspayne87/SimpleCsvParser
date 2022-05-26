using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;

namespace SimpleCsvParser.Processors
{
  /// <summary>
  /// The processor meant to handle each column being added to the object being created
  /// </summary>
  internal class ListStringProcessor : IObjectProcessor<IList<string>>
  {
    private string[] _result = default;
    private int _colIndex = 0;
    private int _colCount;
    /// <summary>
    /// Boolean to determine if a column has been set or not
    /// </summary>
    private bool _isAColumnSet = false;

    private char _wrapper;

    private string _doubleWrap, _singleWrap;

    public ListStringProcessor(char wrapper, int numCols)
    {
      _result = new string[numCols];
      _colCount = numCols;
      _wrapper = wrapper;
      _doubleWrap = $"{_wrapper}{_wrapper}";
      _singleWrap = $"{_wrapper}";
    }


    /// <inheritdoc />
    public void AddColumn(ReadOnlySpan<char> str)
    {
      if (str.Length > 0)
      {
        if (str[0] == _wrapper)
          _result[_colIndex++] = new string(str.Slice(1, str.Length - 2)).Clean(_doubleWrap, _singleWrap);
        else
          _result[_colIndex++] = new string(str).Clean(_doubleWrap, _singleWrap);
        _isAColumnSet = true;
      }
      else
      {
        _result[_colIndex++] = string.Empty;
      }
    }

    /// <inheritdoc />
    public void AddColumn(ReadOnlySpan<char> str, ReadOnlySpan<char> overflow)
    {
      AddColumn(overflow.MergeSpan(str));
    }

    /// <inheritdoc />
    public bool IsEmpty()
    {
      for (int i = 0; i < _colIndex; i++)
        if (!String.IsNullOrWhiteSpace(_result[i]))
          return false;
      return true;
    }

    /// <inheritdoc />
    public bool IsAColumnSet()
    {
      return _isAColumnSet;
    }

    /// <inheritdoc />
    public IList<string> GetObject()
    {
      var returnable = new string[_colCount];
      _result.AsSpan().CopyTo(returnable);
      return returnable;
    }

    /// <inheritdoc />
    public void ClearObject()
    {
      _colIndex = 0;
      _isAColumnSet = false;
    }
  }
}