﻿using SimpleCsvParser;
using System.Text;
using System.Collections.Generic;
using System.IO;
using SimpleCsvParser.Processors;
using System;

namespace Parser.Readers
{
  internal class PipelineReader<T>
      where T : class, new()
  {
    private Stream _stream;
    private IObjectProcessor<T> _processor;
    private ParseOptions _options;

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
    }

    internal IEnumerable<T> Parse()
    {
      _stream.Seek(0, SeekOrigin.Begin);
      using var reader = new StreamReader(_stream, Encoding.UTF8, true, 4 * 1024, true);
      char[] buffer = new char[4 * 1024];                                                                         // Create a buffer to store the characters loaded from the stream.
      StringBuilder overflow = new StringBuilder();                                                               // The overflow buffer to catch anything that was added before
      int bufferLength;                                                                                           // The current buffer length, the start of the column the end of the column
      int start = 0;                                                                                              // The current starting position of the column
      bool inWrapper = false;                                                                                     // If we are currently in a wrapper or not
      char firstRowDelimiter = string.IsNullOrEmpty(_options.RowDelimiter) ? default : _options.RowDelimiter[0];  // Grab the first character from the row delimiter since it is much faster to check char to char
      char firstDelimiter = string.IsNullOrEmpty(_options.Delimiter) ? default : _options.Delimiter[0];           // Grab the first character from the delimiter since it is much faster to check char to char
      char wrapper = _options.Wrapper == null ? default : _options.Wrapper.Value;                                 // Cache the wrapper into the default value so we can test against that
      uint row = 0;                                                                                               // The current row we are working on

      while ((bufferLength = reader.Read(buffer, 0, buffer.Length)) > 0)
      {
        start = 0;
        for (int i = 0; i < bufferLength; ++i)
        {
          if (wrapper == buffer[i])
          {
            if (inWrapper && (i + 1 >= bufferLength || buffer[i + 1] != wrapper))
              inWrapper = false;
            else if (inWrapper && (i + 1 >= bufferLength || buffer[i + 1] == wrapper))
              i++;
            else if (!inWrapper)
              inWrapper = true;
          }
          if (inWrapper) continue;

          if (row >= _options.StartRow && firstDelimiter == buffer[i] && (_options.Delimiter.Length == 1 || _options.Delimiter.EqualsCharArray(buffer, i, i + _options.Delimiter.Length)))
          {
            AddColumn(overflow, buffer, start, i);
            start = i + _options.Delimiter.Length;
          }
          else if (firstRowDelimiter == buffer[i] && (_options.RowDelimiter.Length == 1 || _options.RowDelimiter.EqualsCharArray(buffer, i, i + _options.RowDelimiter.Length)))
          {
            if (row++ >= _options.StartRow)
            {
              AddColumn(overflow, buffer, start, i);
              if (_processor.IsAColumnSet() && !_options.RemoveEmptyEntries || !_processor.IsEmpty())
                yield return _processor.GetObject();
              _processor.ClearObject();
            }
            i += _options.RowDelimiter.Length - 1;
            start = i + 1;
          }
        }

        overflow.Append(buffer, start, bufferLength - start);
        start = 0;
      }

      if (row++ >= _options.StartRow)
      {
        if (overflow.Length > 0)
        {
          if (overflow[0] == wrapper)
            _processor.AddColumn(overflow.Replace($"{wrapper}{wrapper}", $"{wrapper}").ToString(1, overflow.Length - 2));
          else
            _processor.AddColumn(overflow.ToString());
        }
        if (_processor.IsAColumnSet() && !_options.RemoveEmptyEntries || !_processor.IsEmpty())
          yield return _processor.GetObject();
        _processor.ClearObject();
      }
    }

    private void AddColumn(StringBuilder overflow, char[] buffer, int start, int i)
    {
      if (_options.Wrapper != null && buffer[start] == _options.Wrapper.Value)
      {
        if (buffer != null)
          overflow.Append(buffer, start + 1, i - start - 2);
        _processor.AddColumn(overflow.Replace($"{_options.Wrapper.Value}{_options.Wrapper.Value}", $"{_options.Wrapper.Value}").ToString());
      }
      else
      {
        if (buffer != null)
          overflow.Append(buffer, start, i - start);
        _processor.AddColumn(overflow.ToString());
      }
      overflow.Clear();
    }
  }
}
