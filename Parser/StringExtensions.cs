using System;

namespace SimpleCsvParser
{
  internal static class StringExtensions
  {
    public static bool EqualsCharArray(this string str, ReadOnlySpan<char> buffer, int start, int end)
    {
      if (start < 0 || str.Length != end - start) return false;
      for (var i = 0; i < str.Length; ++i)
        if (str[i] != buffer[start + i]) return false;
      return true;
    }

    public static object CastToValue(this string str, Type type, bool isNullable)
    {
      if (isNullable && str == "null") return null;

      if (type.IsEnum) return Enum.Parse(type, str);
      switch (Type.GetTypeCode(type))
      {
        case TypeCode.String:
          return str;
        case TypeCode.DateTime:
          return DateTime.Parse(str);
        case TypeCode.Object:
        case TypeCode.DBNull:
          throw new ArgumentException($"{type.Name} cannot convert value.");
        default:
          return Convert.ChangeType(str, type); //TODO: This could be improved
      }
    }
  }
}