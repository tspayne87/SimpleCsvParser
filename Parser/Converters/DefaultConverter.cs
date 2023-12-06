using SimpleCsvParser;
using System;
using System.Data.SqlTypes;
using System.Reflection;

namespace Parser.Converters
{
  internal class DefaultConverter : IPropertyConverter
  {
    private readonly Type _type;
    private readonly TypeCode _typeCode;
    private readonly bool _isNullable;
    private readonly bool _isEnum;

    public DefaultConverter(PropertyInfo property)
    {
      _isNullable = property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>);
      _type = _isNullable ? Nullable.GetUnderlyingType(property.PropertyType) : property.PropertyType;
      _typeCode = Type.GetTypeCode(_type);
      _isEnum = _type.IsEnum;
    }

    public object Convert(ReadOnlySpan<char> str, string doubleWrap, string singleWrap, bool hasDoubleWrapper)
    {
      if (_isNullable && str == "null") return null;

      if (_isEnum) return Enum.Parse(_type, str);
      if (_type == typeof(Guid))
      {
        try
        {
          return Guid.Parse(str);
        }
        catch
        {
          Console.WriteLine(new string(str));
          throw;
        }
      }

      switch (_typeCode)
      {
        case TypeCode.String: return hasDoubleWrapper ? new string(str).Replace(doubleWrap, singleWrap) : new string(str);
        case TypeCode.DateTime: return DateTime.Parse(str);
        case TypeCode.Int16: return short.Parse(str);
        case TypeCode.Int32: return int.Parse(str);
        case TypeCode.Int64: return long.Parse(str);
        case TypeCode.Char: return str[0];
        case TypeCode.Boolean: return bool.Parse(str);
        case TypeCode.Decimal: return decimal.Parse(str);
        case TypeCode.Double: return double.Parse(str);
        case TypeCode.Byte: return byte.Parse(str);
        case TypeCode.SByte: return sbyte.Parse(str);
        case TypeCode.Single: return Single.Parse(str);
        case TypeCode.UInt16: return ushort.Parse(str);
        case TypeCode.UInt32: return uint.Parse(str);
        case TypeCode.UInt64: return ulong.Parse(str);

        case TypeCode.Empty:
        case TypeCode.Object:
        case TypeCode.DBNull:
        default:
          throw new ArgumentException($"{_type.Name} cannot convert value.");
      }
    }
  }
}
