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
    private readonly PropertyLookup[] _props;

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

      _props = new PropertyLookup[0];
      foreach(var prop in typeof(TModel).GetProperties())
      {
        if (!Attribute.IsDefined(prop, typeof(CsvPropertyAttribute)))
          continue;
        
        var attr = prop.GetCustomAttribute<CsvPropertyAttribute>(true);
        var index = string.IsNullOrWhiteSpace(attr.Header) ? attr.ColIndex : (headers == null ? -1 : headers.IndexOf(attr.Header));

        if (index > -1)
        {
          if (index + 1 > _props.Length)
            Array.Resize(ref _props, index + 1);
          
          _props[index] = new PropertyLookup(prop);
        }
      }
    }

    /// <inheritdoc />
    public void AddColumn(ReadOnlySpan<char> str, bool hasWrapper, bool hasDoubleWrapper)
    {
      PropertyLookup item;
      if (_props.Length > _index && (item = _props[_index]) != null && str.Length > 0)
      {
        if (hasWrapper)
          item.Setter(_model, str.Slice(1, str.Length - 2).CastToValue(item.PropertyTypeCode, item.PropertyType, item.IsNullable, item.IsEnum, _doubleWrap, _singleWrap, hasDoubleWrapper));
        else
          item.Setter(_model, str.CastToValue(item.PropertyTypeCode, item.PropertyType, item.IsNullable, item.IsEnum, _doubleWrap, _singleWrap, hasDoubleWrapper));
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

    private class PropertyLookup
    {
      public Action<TModel, object> Setter;
      public TypeCode PropertyTypeCode;
      public Type PropertyType;
      public bool IsNullable, IsEnum;
      
      public PropertyLookup(PropertyInfo p)
      {
        IsNullable = p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>);
        PropertyType = IsNullable ? Nullable.GetUnderlyingType(p.PropertyType) : p.PropertyType;
        PropertyTypeCode = Type.GetTypeCode(PropertyType);
        IsEnum = PropertyType.IsEnum;

        var prop = Expression.Parameter(typeof(TModel), "x");
        var propObject = Expression.Parameter(typeof(object), "y");

        var body = Expression.Assign(Expression.Property(prop, p), Expression.Convert(propObject, p.PropertyType));
        var lambda = Expression.Lambda<Action<TModel, object>>(body, new[] { prop, propObject });
        Setter = lambda.Compile();
      }
    }
  }
}