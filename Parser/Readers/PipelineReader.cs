using SimpleCsvParser;
using System.Text;
using System.IO;
using SimpleCsvParser.Processors;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Parser.Readers
{
  internal class PipelineReader<T>
      where T : class, new()
  {
    private Stream _stream;
    private IObjectProcessor<T> _processor;
    private ParseOptions _options;
    private string _doubleWrap, _singleWrap;
    private char _wrapper;
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
    }

    internal void Parse(Action<T> rowHandler, CancellationToken cancellationToken)
    {
      _stream.Seek(0, SeekOrigin.Begin);
      using var reader = new StreamReader(_stream, Encoding.UTF8, true, 4 * 1024, true);
      Span<char> buffer = new Span<char>(new char[4 * 1024]);                                                     // Create a buffer to store the characters loaded from the stream.
      int bufferLength;                                                                                           // The current buffer length, the start of the column the end of the column
      int start = 0;                                                                                              // The current starting position of the column
      bool inWrapper = false;                                                                                     // If we are currently in a wrapper or not
      char firstRowDelimiter = string.IsNullOrEmpty(_options.RowDelimiter) ? default : _options.RowDelimiter[0];  // Grab the first character from the row delimiter since it is much faster to check char to char
      char firstDelimiter = string.IsNullOrEmpty(_options.Delimiter) ? default : _options.Delimiter[0];           // Grab the first character from the delimiter since it is much faster to check char to char
      uint row = 0;                                                                                               // The current row we are working on
      ReadOnlySpan<char> delimiterSpan = _options.Delimiter.AsSpan();
      ReadOnlySpan<char> rowDelimiterSpan = _options.RowDelimiter.AsSpan();

      Span<char> overflow = new Span<char>(new char[1024]);
      int overflowLength = 0;

      int lenRowDelimiter = rowDelimiterSpan.Length;
      int lenColDelimiter = delimiterSpan.Length;

      while ((bufferLength = reader.Read(buffer)) > 0)
      {
        if (cancellationToken.IsCancellationRequested)
          break;

        start = 0;
        for (int i = 0; i < bufferLength; ++i)
        {
          if (_wrapper == buffer[i])
          {
            if (inWrapper && (i + 1 >= bufferLength || buffer[i + 1] != _wrapper))
              inWrapper = false;
            else if (inWrapper && (i + 1 >= bufferLength || buffer[i + 1] == _wrapper))
              i++;
            else if (!inWrapper)
              inWrapper = true;
          }
          if (inWrapper)
            continue;

          if (firstDelimiter == buffer[i] && row >= _options.StartRow && (lenColDelimiter == 1 || delimiterSpan.EqualsCharSpan(buffer, i, i + lenColDelimiter)))
          {
            if (overflowLength > 0)
            {
              AddColumnWithOverflow(overflow.Slice(0, overflowLength), buffer, start, i);
              overflowLength = 0;
            }
            else
              AddColumn(buffer, start, i);
            start = i + lenColDelimiter;
          }
          else if (firstRowDelimiter == buffer[i] && (lenRowDelimiter == 1 || rowDelimiterSpan.EqualsCharSpan(buffer, i, i + lenRowDelimiter)))
          {
            if (row++ >= _options.StartRow)
            {
              if (overflowLength > 0)
              {
                AddColumnWithOverflow(overflow.Slice(0, overflowLength), buffer, start, i);
                overflowLength = 0;
              }
              else
                AddColumn(buffer, start, i);
              if (_processor.IsAColumnSet() && !_options.RemoveEmptyEntries || !_processor.IsEmpty())
              {
                rowHandler(_processor.GetObject());
                if (cancellationToken.IsCancellationRequested)
                  break;
              }
              _processor.ClearObject();
            }
            i += lenRowDelimiter - 1;
            start = i + 1;
          }
        }

        if (cancellationToken.IsCancellationRequested)
          break;

        if (start < bufferLength)
        {
          buffer.Slice(start, bufferLength - start).CopyTo(overflow);
          overflowLength = bufferLength - start;
        }
      }

      if (!cancellationToken.IsCancellationRequested && row++ >= _options.StartRow)
      {
        if (overflowLength > 0)
          _processor.AddColumn(overflow.Slice(0, overflowLength));
        if (_processor.IsAColumnSet() && !_options.RemoveEmptyEntries || !_processor.IsEmpty())
          rowHandler(_processor.GetObject());
        _processor.ClearObject();
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddColumn(ReadOnlySpan<char> buffer, int start, int i)
    {
      if (buffer == null) return;
      _processor.AddColumn(buffer.Slice(start, i - start));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddColumnWithOverflow(ReadOnlySpan<char> overflow, ReadOnlySpan<char> buffer, int start, int i)
    {
      if (buffer == null)
        _processor.AddColumn(overflow);
      else
        _processor.AddColumn(buffer.Slice(start, i - start), overflow);
    }
  }
}
