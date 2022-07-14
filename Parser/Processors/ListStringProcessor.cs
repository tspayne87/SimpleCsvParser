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
    private string[] _result;
    private int _colIndex;
    private int _colCount;

    /// <summary>
    /// Boolean to determine if a column has been set or not
    /// </summary>
    private bool _isAColumnSet = false;

    private char _wrapper;

    private string _doubleWrap, _singleWrap;

    public ListStringProcessor(char wrapper)
    {
      _result = new string[0];
      _colIndex = 0;
      _wrapper = wrapper;
      _doubleWrap = $"{_wrapper}{_wrapper}";
      _singleWrap = $"{_wrapper}";
    }


    /// <inheritdoc />
    public void AddColumn(ReadOnlySpan<char> str, bool hasWrapper, bool hasDoubleWrapper)
    {
      if (_result.Length < _colIndex + 1)
      {
        Array.Resize(ref _result, _colIndex + 1);
        _colCount = _colIndex + 1;
      }

      if (str.Length > 0)
      {
        if (hasDoubleWrapper)
        {
          if (hasWrapper)
            _result[_colIndex++] = new string(str.Slice(1, str.Length - 2)).Clean(_doubleWrap, _singleWrap);
          else
            _result[_colIndex++] = new string(str).Clean(_doubleWrap, _singleWrap);
        }
        else
        {
          if (hasWrapper)
            _result[_colIndex++] = new string(str.Slice(1, str.Length - 2));
          else
            _result[_colIndex++] = new string(str);
        }
        _isAColumnSet = true;
      }
      else
      {
        _result[_colIndex++] = string.Empty;
      }
    }

    /// <inheritdoc />
    public void AddColumn(ReadOnlySpan<char> str, ReadOnlySpan<char> overflow, bool hasWrapper, bool hasDoubleWrapper)
    {
      AddColumn(overflow.MergeSpan(str), hasWrapper, hasDoubleWrapper);
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