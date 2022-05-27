using System;
using System.Runtime.CompilerServices;

namespace SimpleCsvParser
{
  internal static class StringExtensions
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsCharSpan(this ReadOnlySpan<char> str, ReadOnlySpan<char> buffer, int start, int end)
    {
      if (start < 0 || str.Length != end - start || end > buffer.Length) return false;
      return str.SequenceEqual(buffer.Slice(start, end - start));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Clean(this string str, string doubleWrap, string singleWrap)
    {
      return str.Replace(doubleWrap, singleWrap);//using escapes is somewhat exceptional but still this allocates another string unnessarily 
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<char> MergeSpan(this ReadOnlySpan<char> left, ReadOnlySpan<char> right)
    {
      Span<char> result = new Span<char>(new char[left.Length + right.Length]);
      left.CopyTo(result);
      right.CopyTo(result.Slice(left.Length, right.Length));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static object CastToValue(this ReadOnlySpan<char> str, TypeCode typeCode, Type type, bool isNullable, bool isEnum, string doubleWrap, string singleWrap)
    {
      if (isNullable && str == "null") return null;

      if (isEnum) return Enum.Parse(type, new string(str));
      if(type ==typeof(Guid)) return Guid.Parse(str);
                
      switch (typeCode)
      {
        case TypeCode.String: return new string(str).Clean(doubleWrap, singleWrap);
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
          throw new ArgumentException($"{type.Name} cannot convert value.");
      }
    }
  }
}