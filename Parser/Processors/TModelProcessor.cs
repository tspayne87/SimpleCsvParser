using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;

namespace SimpleCsvParser.Processors
{
  /// <summary>
  /// The processor meant to handle each column being added to the object being created
  /// </summary>
  internal class TModelProcessor<TModel> : IObjectProcessor<TModel>
    where TModel : class, new()
  {
    /// <summary>
    /// The props used when assigning values
    /// </summary>
    private readonly Dictionary<int, SimpleInfo> _props;

    /// <summary>
    /// The current index for the row we have added to the model
    /// </summary>
    private int _index;

    /// <summary>
    /// The current object model meant to be generated for each row in the parsed data
    /// </summary>
    private TModel _model;

    /// <summary>
    /// Check to make sure a property is set otherwise it is most likely a empty object
    /// </summary>
    private bool _isNotSet;

    /// <summary>
    /// Boolean to determine if a column has been set or not
    /// </summary>
    private bool _isAColumnSet;

    /// <summary>
    /// Constructor for the processor to load in the headers that will be used when building out the objects
    /// </summary>
    public TModelProcessor(List<string> headers)
    {
      _index = 0;
      _model = new TModel();
      _isNotSet = true;
      _isAColumnSet = false;
      _props = typeof(TModel)
        .GetProperties()
        .Where(prop => Attribute.IsDefined(prop, typeof(CsvPropertyAttribute)))
        .Select(x => new KeyValuePair<PropertyInfo, CsvPropertyAttribute>(x, x.GetCustomAttribute<CsvPropertyAttribute>(true)))
        .OrderBy(x => x.Value.ColIndex)
        .ThenBy(x => x.Value.Header)
        .Select(x =>
        {
          if (string.IsNullOrEmpty(x.Value.Header))
            return new KeyValuePair<int, SimpleInfo>(x.Value.ColIndex, new SimpleInfo(x.Key));
          if (headers == null)
            return new KeyValuePair<int, SimpleInfo>(-1, null);
          return new KeyValuePair<int, SimpleInfo>(headers.IndexOf(x.Value.Header), new SimpleInfo(x.Key));
        })
        .Where(x => x.Key > -1)
        .ToDictionary(x => x.Key, x => x.Value);
    }

    /// <inheritdoc />
    public void AddColumn(string str)
    {
      if (_props.ContainsKey(_index) && !string.IsNullOrWhiteSpace(str))
      {
        _props[_index].Set(_model, str);
        _isNotSet = false;
      }
      _index++;
      _isAColumnSet = true;
    }

    /// <inheritdoc />
    public bool IsEmpty()
    {
      return _isNotSet;
    }

    /// <inheritdoc />
    public bool IsAColumnSet()
    {
      return _isAColumnSet;
    }

    /// <inheritdoc />
    public TModel GetObject()
    {
      return _model;
    }

    /// <inheritdoc />
    public void ClearObject()
    {
      _index = 0;
      _model = new TModel();
      _isNotSet = true;
      _isAColumnSet = false;
    }

    /// <summary>
    /// Simple info object meant to compile a setter through linq expressions so that it can be fast
    /// </summary>
    private class SimpleInfo
    {
      /// <summary>
      /// Constructor is meant to build out the expression for use down the line
      /// </summary>
      /// <param name="p">The property we are working with</param>
      public SimpleInfo(PropertyInfo p)
      {
        var isNullable = p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>);
        var propertyType = isNullable ? Nullable.GetUnderlyingType(p.PropertyType) : p.PropertyType;
        var prop = Expression.Parameter(typeof(TModel), "x");
        var propObject = Expression.Parameter(typeof(string), "y");

        var converter = typeof(StringExtensions).GetMethod("CastToValue", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        var converterCall = Expression.Call(null, converter, propObject, Expression.Constant(propertyType), Expression.Constant(isNullable));

        var body = Expression.Assign(Expression.Property(prop, p), Expression.Convert(converterCall, p.PropertyType));

        var lambda = Expression.Lambda<Action<TModel, string>>(body, new[] { prop, propObject });
        Set = lambda.Compile();
      }

      public readonly Action<TModel, string> Set;
    }
  }
}