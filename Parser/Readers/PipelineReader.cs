using SimpleCsvParser;
using System.Text;
using System.IO;
using SimpleCsvParser.Processors;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Collections.Generic;
using Parser.Watchers;

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
      int lenRowDelimiter = _rowDelimiterSpan.Length;
      int lenColDelimiter = _delimiterSpan.Length;

      while ((bufferLength = reader.Read(_buffer)) > 0)
      {
        start = 0;
        for (int i = 0; i < bufferLength; ++i)
        {
          var current = _buffer[i];
          int rowBufferCurrentIndex = i - lenRowDelimiter + 1;
          int colBufferCurrentIndex = i - lenColDelimiter + 1;
          char? rowBufferCurrent = overflowLength > 0 && rowBufferCurrentIndex < 0 ? _overflow[overflowLength + rowBufferCurrentIndex] : null;
          char? colBufferCurrent = overflowLength > 0 && colBufferCurrentIndex < 0 ? _overflow[overflowLength + colBufferCurrentIndex] : null;

          if (_wrapper == current)
          {
            if (!inWrapper)
            {
              inWrapper = true;
              hasWrapper = true;
            }
            else
            {
              var next = _buffer[i + 1];
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
              || (firstDelimiter == current && _delimiterSpan.EqualsCharArray(_buffer, i, i + lenColDelimiter))
              || (firstDelimiter == colBufferCurrent && _delimiterSpan.EqualsBetweenCharArray(_buffer, i, _overflow, colBufferCurrentIndex))
            )
          )
          {
            if (overflowLength > 0)
            {
              var merge = MergeBuffers(_overflow, overflowLength, _buffer, i);
              AddColumn(merge, 0, i, hasWrapper, hasDoubleWrapper);
              overflowLength = 0;
              hasDoubleWrapper = false;
              hasWrapper = false;
            }
            else
            {
              AddColumn(_buffer, start, i, hasWrapper, hasDoubleWrapper);
              hasDoubleWrapper = false;
              hasWrapper = false;
            }
            start = i + lenColDelimiter;
          }
          else if (
            (firstRowDelimiter == current && lenRowDelimiter == 1)
            || (firstRowDelimiter == current && _rowDelimiterSpan.EqualsCharArray(_buffer, i, i + lenRowDelimiter))
            || (firstRowDelimiter == rowBufferCurrent && _rowDelimiterSpan.EqualsBetweenCharArray(_buffer, i, _overflow, rowBufferCurrentIndex))
          )
          {
            if (row++ >= _options.StartRow)
            {
              if (overflowLength > 0)
              {
                var merge = MergeBuffers(_overflow, overflowLength, _buffer, i);
                AddColumn(merge, 0, i, hasWrapper, hasDoubleWrapper);
                overflowLength = 0;
                hasDoubleWrapper = false;
                hasWrapper = false;
              }
              else
              {
                AddColumn(_buffer, start, i, hasWrapper, hasDoubleWrapper);
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
          _buffer.Slice(start, bufferLength - start).CopyTo(_overflow);
          overflowLength = bufferLength - start;
        }
      }

      if (row++ >= _options.StartRow)
      {
        if (overflowLength > 0)
          _processor.AddColumn(_overflow, 0, overflowLength, hasWrapper, hasDoubleWrapper);
        if (_processor.IsAColumnSet() && !_options.RemoveEmptyEntries || !_processor.IsEmpty())
          yield return _processor.GetObject();
        _processor.ClearObject();
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddColumn(in char[] buffer, int start, int i, bool hasWrapper, bool hasDoubleWrapper)
    {
      if (buffer == null) return;
      _processor.AddColumn(buffer, start, i, hasWrapper, hasDoubleWrapper);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private char[] MergeBuffers(in char[] overflow, int overflowLength, in char[] buffer, int end)
    {
      var result = new char[overflowLength + end];
      var index = 0;
      for (var i = 0; i < overflowLength; ++i)
        result[index++] = overflow[i];
      for (var i = 0; i < end; ++i)
        result[index++] = buffer[i];
      return result;
    }
  }
}
