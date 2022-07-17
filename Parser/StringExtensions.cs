using System;
using System.Runtime.CompilerServices;

namespace SimpleCsvParser
{
  internal static class StringExtensions
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsCharArray(this char[] str, in char[] buffer, int start, int end)
    {
      if (start < 0 || str.Length != end - start || end > buffer.Length) return false;
      for (var i = 0; i < str.Length; ++i)
        if (str[i] != buffer[start + i]) return false;
      return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsBetweenCharArray(this char[] str, in char[] buffer, int end, in char[] overflow, int start)
    {
      var index = 0;
      for (var i = start; i < overflow.Length; ++i)
        if (str[index++] != overflow[i]) return false;
      for (var i = 0; i < end; ++i)
        if (str[index++] != buffer[i]) return false;
      return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Clean(this string str, string doubleWrap, string singleWrap)
    {
      return str.Replace(doubleWrap, singleWrap);//using escapes is somewhat exceptional but still this allocates another string unnessarily 
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<T> Slice<T>(this T[] arr, int start, int length)
    {
      return new ReadOnlySpan<T>(arr, start, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static object CastToValue(this char[] str, int start, int length, TypeCode typeCode, Type type, bool isNullable, bool isEnum, string doubleWrap, string singleWrap, bool hasDoubleWrapper)
    {
      if (isNullable && str.Slice(start, length).SequenceEqual("null")) return null;

      if (isEnum) return Enum.Parse(type, new string(str, start, length));
      if(type ==typeof(Guid)) return Guid.Parse(str.Slice(start, length));
                
      switch (typeCode)
      {
        case TypeCode.String: return hasDoubleWrapper ? new string(str, start, length).Clean(doubleWrap, singleWrap) : new string(str, start, length);
        case TypeCode.DateTime: return DateTime.Parse(str.Slice(start, length));
        case TypeCode.Int16: return short.Parse(str.Slice(start, length));
        case TypeCode.Int32: return int.Parse(str.Slice(start, length));
        case TypeCode.Int64: return long.Parse(str.Slice(start, length));
        case TypeCode.Char: return str[start];
        case TypeCode.Boolean: return bool.Parse(str.Slice(start, length));
        case TypeCode.Decimal: return decimal.Parse(str.Slice(start, length));
        case TypeCode.Double: return double.Parse(str.Slice(start, length));
        case TypeCode.Byte: return byte.Parse(str.Slice(start, length));
        case TypeCode.SByte: return sbyte.Parse(str.Slice(start, length));
        case TypeCode.Single: return Single.Parse(str.Slice(start, length));
        case TypeCode.UInt16: return ushort.Parse(str.Slice(start, length));
        case TypeCode.UInt32: return uint.Parse(str.Slice(start, length));
        case TypeCode.UInt64: return ulong.Parse(str.Slice(start, length));

        case TypeCode.Empty:
        case TypeCode.Object:
        case TypeCode.DBNull:
        default:
          throw new ArgumentException($"{type.Name} cannot convert value.");
      }
    }
  }
}