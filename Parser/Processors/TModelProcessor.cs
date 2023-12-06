using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using Parser.Converters;

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

    /// <summary>
    /// The wrapper we are dealing with
    /// </summary>
    private char _wrapper;

    /// <summary>
    /// The double and single wraps to cache it once and use it multiple times
    /// </summary>
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
          
          _props[index] = new PropertyLookup(prop, new DefaultConverter(prop));
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
          item.Property.SetValue(_model, item.Converter.Convert(str.Slice(1, str.Length - 2), _doubleWrap, _singleWrap, hasDoubleWrapper));
        else
          item.Property.SetValue(_model, item.Converter.Convert(str, _doubleWrap, _singleWrap, hasDoubleWrapper));
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
      public IPropertyConverter Converter;
      public PropertyInfo Property;
      
      public PropertyLookup(PropertyInfo property, IPropertyConverter converter)
      {
        Property = property;
        Converter = converter;
      }
    }
  }
}