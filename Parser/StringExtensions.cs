using System;
using System.Runtime.CompilerServices;

namespace SimpleCsvParser
{
  internal static class StringExtensions
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<char> MergeSpan(this Span<char> left, Span<char> right)
    {
      Span<char> result = new Span<char>(new char[left.Length + right.Length]);
      left.CopyTo(result);
      right.CopyTo(result.Slice(left.Length, right.Length));
      return result;
    }
  }
}