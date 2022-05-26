using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;

namespace SimpleCsvParser.Processors
{
  /// <summary>
  /// The processor meant to handle each column being added to the object being created
  /// </summary>
  internal class ListStringProcessor : IObjectProcessor<List<string>>
  {
    /// <summary>
    /// The internal result used to store data into
    /// </summary>
    private List<string> _result = new List<string>();

    /// <summary>
    /// Boolean to determine if a column has been set or not
    /// </summary>
    private bool _isAColumnSet = false;

    private char _wrapper;

    private string _doubleWrap, _singleWrap;

    public ListStringProcessor(char wrapper)
    {
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
          _result.Add(new string(str.Slice(1, str.Length - 2)).Clean(_doubleWrap, _singleWrap));
        else
          _result.Add(new string(str).Clean(_doubleWrap, _singleWrap));
        _isAColumnSet = true;
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
      return _result.Where(x => !string.IsNullOrWhiteSpace(x)).Count() == 0;
    }

    /// <inheritdoc />
    public bool IsAColumnSet()
    {
      return _isAColumnSet;
    }

    /// <inheritdoc />
    public List<string> GetObject()
    {
      return _result.GetRange(0, _result.Count);
    }

    /// <inheritdoc />
    public void ClearObject()
    {
      _result.Clear();
      _isAColumnSet = false;
    }
  }
}