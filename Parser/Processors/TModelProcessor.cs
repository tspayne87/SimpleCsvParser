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
    private readonly Dictionary<int, (Action<TModel, object>, TypeCode, Type, bool, bool)> _props;

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

    private char _wrapper;

    private string _doubleWrap, _singleWrap;

    /// <summary>
    /// Constructor for the processor to load in the headers that will be used when building out the objects
    /// </summary>
    public TModelProcessor(List<string> headers, char wrapper)
    {
      _index = 0;
      _model = new TModel();
      _isNotSet = true;
      _isAColumnSet = false;
      _wrapper = wrapper;
      _doubleWrap = $"{_wrapper}{_wrapper}";
      _singleWrap = $"{_wrapper}";

      _props = new Dictionary<int, (Action<TModel, object>, TypeCode, Type, bool, bool)>();
      foreach(var prop in typeof(TModel).GetProperties())
      {
        if (!Attribute.IsDefined(prop, typeof(CsvPropertyAttribute)))
          continue;
          
        var attr = prop.GetCustomAttribute<CsvPropertyAttribute>(true);
        if (string.IsNullOrWhiteSpace(attr.Header))
          _props[attr.ColIndex] = GeneratePropertyDetails(prop);

        if (headers == null)
          continue;
        
        var index = headers.IndexOf(attr.Header);
        if (index != -1)
          _props[index] = GeneratePropertyDetails(prop);
      }
    }

    /// <inheritdoc />
    public void AddColumn(ReadOnlySpan<char> str)
    {
      if (_props.ContainsKey(_index) && str.Length > 0)
      {
        if (str[0] == _wrapper)
          _props[_index].Item1(_model, str.Slice(1, str.Length - 2).CastToValue(_props[_index].Item2, _props[_index].Item3, _props[_index].Item4, _props[_index].Item5, _doubleWrap, _singleWrap));
        else
          _props[_index].Item1(_model, str.CastToValue(_props[_index].Item2, _props[_index].Item3, _props[_index].Item4, _props[_index].Item5, _doubleWrap, _singleWrap));
        _isNotSet = false;
      }
      _index++;
      _isAColumnSet = true;
    }

    /// <inheritdoc />
    public void AddColumn(ReadOnlySpan<char> str, ReadOnlySpan<char> overflow)
    {
      AddColumn(overflow.MergeSpan(str));
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
    /// Constructor is meant to build out the expression for use down the line
    /// </summary>
    /// <param name="p">The property we are working with</param>
    private (Action<TModel, object>, TypeCode, Type, bool, bool) GeneratePropertyDetails(PropertyInfo p)
    {
      var isNullable = p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>);
      var propertyType = isNullable ? Nullable.GetUnderlyingType(p.PropertyType) : p.PropertyType;
      var prop = Expression.Parameter(typeof(TModel), "x");
      var propObject = Expression.Parameter(typeof(object), "y");

      var body = Expression.Assign(Expression.Property(prop, p), Expression.Convert(propObject, p.PropertyType));
      var lambda = Expression.Lambda<Action<TModel, object>>(body, new[] { prop, propObject });
      return (lambda.Compile(), Type.GetTypeCode(propertyType), propertyType, isNullable, propertyType.IsEnum);
    }
  }
}