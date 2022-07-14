using SimpleCsvParser;
using System.Text;
using System.IO;
using SimpleCsvParser.Processors;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Collections.Generic;

namespace Parser.Readers
{
  internal class PipelineReader<T>
  {
    private Stream _stream;
    private IObjectProcessor<T> _processor;
    private ParseOptions _options;
    private string _doubleWrap, _singleWrap;
    private char _wrapper;

    private char[] _buffer;
    private char[] _overflow;
    private readonly char[] _delimiterSpan;
    private readonly char[] _rowDelimiterSpan;

    private Span<char> buffer => _buffer;
    private Span<char> overflow => _overflow;
    private ReadOnlySpan<char> delimiterSpan => _delimiterSpan;
    private ReadOnlySpan<char> rowDelimiterSpan => _rowDelimiterSpan;
    

    public PipelineReader(Stream stream, ParseOptions options, IObjectProcessor<T> processor)
    {
      if (options.Wrapper != null && options.RowDelimiter.IndexOf(options.Wrapper.Value) > -1)
        throw new ArgumentException("Row delimiter cannot contain a value from Wrapper or Delimiter");
      if (!string.IsNullOrEmpty(options.Delimiter) && options.RowDelimiter.IndexOf(options.Delimiter) > -1)
        throw new ArgumentException("Row delimiter cannot contain a value from Wrapper or Delimiter");
      if (options.Wrapper != null && !string.IsNullOrEmpty(options.Delimiter) && options.Wrapper.ToString() == options.Delimiter)
        throw new ArgumentException("Wrapper and Delimiter cannot be equal");

      _stream = stream;
      _processor = processor;
      _options = options;
      _wrapper = _options.Wrapper == null ? default : _options.Wrapper.Value;
      _doubleWrap = $"{_wrapper}{_wrapper}";
      _singleWrap = $"{_wrapper}";
      _buffer = new char[4 * 1024];
      _overflow = new char[1024];
      _delimiterSpan = _options.Delimiter.ToCharArray();
      _rowDelimiterSpan = _options.RowDelimiter.ToCharArray();
    }

    internal IEnumerable<T> Parse()
    {
      _stream.Seek(0, SeekOrigin.Begin);
      using var reader = new StreamReader(_stream, Encoding.UTF8, true, 4 * 1024, true);
      int bufferLength;                                                                                           // The current buffer length, the start of the column the end of the column
      int start = 0;                                                                                              // The current starting position of the column
      bool inWrapper = false;                                                                                     // If we are currently in a wrapper or not
      char firstRowDelimiter = string.IsNullOrEmpty(_options.RowDelimiter) ? default : _options.RowDelimiter[0];  // Grab the first character from the row delimiter since it is much faster to check char to char
      char firstDelimiter = string.IsNullOrEmpty(_options.Delimiter) ? default : _options.Delimiter[0];           // Grab the first character from the delimiter since it is much faster to check char to char
      uint row = 0;                                                                                               // The current row we are working on
      bool hasDoubleWrapper = false;
      bool hasWrapper = false;
      int overflowLength = 0;
      int lenRowDelimiter = rowDelimiterSpan.Length;
      int lenColDelimiter = delimiterSpan.Length;

      while ((bufferLength = reader.Read(buffer)) > 0)
      {
        start = 0;
        for (int i = 0; i < bufferLength; ++i)
        {
          var current = buffer[i];
          int rowBufferCurrentIndex = i - lenRowDelimiter + 1;
          int colBufferCurrentIndex = i - lenColDelimiter + 1;
          char? rowBufferCurrent = overflowLength > 0 && rowBufferCurrentIndex < 0 ? overflow[overflowLength + rowBufferCurrentIndex] : null;
          char? colBufferCurrent = overflowLength > 0 && colBufferCurrentIndex < 0 ? overflow[overflowLength + colBufferCurrentIndex] : null;

          if (_wrapper == current)
          {
            if (!inWrapper)
            {
              inWrapper = true;
              hasWrapper = true;
            }
            else
            {
              var next = buffer[i + 1];
              if (inWrapper && (i + 1 >= bufferLength || next != _wrapper))
                inWrapper = false;
              else if (inWrapper && (i + 1 >= bufferLength || next == _wrapper))
              {
                i++;
                hasDoubleWrapper = true;
              }
            }
          }
          if (inWrapper)
            continue;

          if (
            row >= _options.StartRow &&
            (
              (firstDelimiter == current && lenColDelimiter == 1)
              || (firstDelimiter == current && delimiterSpan.EqualsCharSpan(buffer, i, i + lenColDelimiter))
              || (firstDelimiter == colBufferCurrent && delimiterSpan.EqualsCharSpan(overflow.Slice(overflowLength + colBufferCurrentIndex, Math.Abs(colBufferCurrentIndex)).MergeSpan(buffer.Slice(0, lenColDelimiter + colBufferCurrentIndex)), 0, lenColDelimiter))
            )
          )
          {
            if (overflowLength > 0)
            {
              AddColumnWithOverflow(overflow.Slice(0, overflowLength), buffer, start, i, hasWrapper, hasDoubleWrapper);
              overflowLength = 0;
              hasDoubleWrapper = false;
              hasWrapper = false;
            }
            else
            {
              AddColumn(buffer, start, i, hasWrapper, hasDoubleWrapper);
              hasDoubleWrapper = false;
              hasWrapper = false;
            }
            start = i + lenColDelimiter;
          }
          else if (
            (firstRowDelimiter == current && lenRowDelimiter == 1)
            || (firstRowDelimiter == current && rowDelimiterSpan.EqualsCharSpan(buffer, i, i + lenRowDelimiter))
            || (firstRowDelimiter == rowBufferCurrent && rowDelimiterSpan.EqualsCharSpan(overflow.Slice(overflowLength + rowBufferCurrentIndex, Math.Abs(rowBufferCurrentIndex)).MergeSpan(buffer.Slice(0, lenRowDelimiter + rowBufferCurrentIndex)), 0, lenRowDelimiter))
          )
          {
            if (row++ >= _options.StartRow)
            {
              if (overflowLength > 0)
              {
                AddColumnWithOverflow(overflow.Slice(0, overflowLength), buffer, start, i, hasWrapper, hasDoubleWrapper);
                overflowLength = 0;
                hasDoubleWrapper = false;
                hasWrapper = false;
              }
              else
              {
                AddColumn(buffer, start, i, hasWrapper, hasDoubleWrapper);
                hasDoubleWrapper = false;
                hasWrapper = false;
              }
              if (_processor.IsAColumnSet() && !_options.RemoveEmptyEntries || !_processor.IsEmpty())
              {
                yield return _processor.GetObject();
              }
              _processor.ClearObject();
            }
            i += lenRowDelimiter - 1;
            start = i + 1;
          }
        }

        if (start < bufferLength)
        {
          buffer.Slice(start, bufferLength - start).CopyTo(overflow);
          overflowLength = bufferLength - start;
        }
      }

      if (row++ >= _options.StartRow)
      {
        if (overflowLength > 0)
          _processor.AddColumn(overflow.Slice(0, overflowLength), hasWrapper, hasDoubleWrapper);
        if (_processor.IsAColumnSet() && !_options.RemoveEmptyEntries || !_processor.IsEmpty())
          yield return _processor.GetObject();
        _processor.ClearObject();
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddColumn(ReadOnlySpan<char> buffer, int start, int i, bool hasWrapper, bool hasDoubleWrapper)
    {
      if (buffer == null) return;
      _processor.AddColumn(buffer.Slice(start, i - start), hasWrapper, hasDoubleWrapper);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddColumnWithOverflow(ReadOnlySpan<char> overflow, ReadOnlySpan<char> buffer, int start, int i, bool hasWrapper, bool hasDoubleWrapper)
    {
      if (buffer == null)
        _processor.AddColumn(overflow, hasWrapper, hasDoubleWrapper);
      else
        _processor.AddColumn(buffer.Slice(start, i - start), overflow, hasWrapper, hasDoubleWrapper);
    }
  }
}
