using System.Collections.Generic;
using System;
using System.Linq;

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

    /// <inheritdoc />
    public void AddColumn(string str)
    {
      _result.Add(str);
      _isAColumnSet = true;
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