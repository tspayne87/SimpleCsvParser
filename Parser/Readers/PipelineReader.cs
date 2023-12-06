﻿using SimpleCsvParser;
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
    private char[] _buffer;
    private char[] _overflow;
    private readonly IWatcher _wrapperWatcher;
    private readonly IWatcher _delimiterWatcher;
    private readonly IWatcher _rowDelimiterWatcher;

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
      _buffer = new char[4 * 1024];
      _overflow = new char[1024];

      _wrapperWatcher = _options.Wrapper.ToWatcher();
      _delimiterWatcher = _options.Delimiter.ToWatcher();
      _rowDelimiterWatcher = _options.RowDelimiter.ToWatcher();
    }

    internal IEnumerable<T> Parse()
    {
      /// Prime the stream and reader so we can start working
      _stream.Seek(0, SeekOrigin.Begin);
      using var reader = new StreamReader(_stream, Encoding.UTF8, true, 4 * 1024, true);

      int bufferLength;                        // The current buffer length, the start of the column the end of the column
      int start = 0;                           // The current starting position of the column
      bool inWrapper = false;                  // If we are currently in a wrapper or not
      uint row = 0;                            // The current row we are working on
      bool hasDoubleWrapper = false;           // If we are working with a double wrapper
      bool hasWrapper = false;                 // If we are working with just a wrapper
      int overflowLength = 0;                  // The length over our overflow buffer
      int watcherResult = 0;                   // The watcher result we want to check against
      bool checkNextWrapper = false;           // If we should check for the next wrapper to determine if it is escaped or not
      int startRow = _options.StartRow;        // Cached value since the git operation can get expensive since we call it so many times
      bool hasOptionWrapper = _options.Wrapper != null;
      bool hasOptionDelimiter = _options.Delimiter.Length > 0;

      while ((bufferLength = reader.Read(_buffer)) > 0)
      {
        start = 0;
        for (int i = 0; i < bufferLength; ++i)
        {
          var current = _buffer[i];

          #region Wrapper Checks
          if (hasOptionWrapper)
          {
            watcherResult = _wrapperWatcher.FindIndex(current, i);
            if (checkNextWrapper)
            {
              checkNextWrapper = false;
              if (watcherResult == int.MinValue)
                inWrapper = false;
              else
                hasDoubleWrapper = true;
            }
            else if (watcherResult != int.MinValue)
            {
              if (!inWrapper)
              {
                inWrapper = true;
                hasWrapper = true;
              }
              else
              {
                checkNextWrapper = true;
              }
            }
            if (inWrapper)
              continue;
          }
          #endregion

          #region Delimiter Checks
          if (hasOptionDelimiter && row >= startRow && (watcherResult = _delimiterWatcher.FindIndex(current, i)) != int.MinValue)
          {
            if (watcherResult < 0)
            {
              _processor.AddColumn(_overflow.Slice(0, overflowLength + watcherResult), hasWrapper, hasDoubleWrapper);
              overflowLength = 0;
            }
            else if (overflowLength > 0)
            {
              _processor.AddColumn(_overflow.Slice(0, overflowLength).MergeSpan(_buffer.Slice(0, watcherResult)), hasWrapper, hasDoubleWrapper);
              overflowLength = 0;
            }
            else
            {
              _processor.AddColumn(_buffer.Slice(start, watcherResult - start), hasWrapper, hasDoubleWrapper);
            }
            hasDoubleWrapper = false;
            hasWrapper = false;
            start = i + 1;
          }
          #endregion

          #region Row Delimiter Checks
          else if ((watcherResult = _rowDelimiterWatcher.FindIndex(current, i)) != int.MinValue)
          {
            if (row++ >= startRow)
            {
              if (watcherResult < 0)
              {
                _processor.AddColumn(_overflow.Slice(0, overflowLength + watcherResult), hasWrapper, hasDoubleWrapper);
                overflowLength = 0;
              }
              else if (overflowLength > 0)
              {
                _processor.AddColumn(_overflow.Slice(0, overflowLength).MergeSpan(_buffer.Slice(0, watcherResult)), hasWrapper, hasDoubleWrapper);
                overflowLength = 0;
              }
              else
              {
                _processor.AddColumn(_buffer.Slice(start, watcherResult - start), hasWrapper, hasDoubleWrapper);
              }

              if (_processor.IsAColumnSet() && !_options.RemoveEmptyEntries || !_processor.IsEmpty())
              {
                yield return _processor.GetObject();
              }
              _processor.ClearObject();
            }

            hasDoubleWrapper = false;
            hasWrapper = false;
            start = i + 1;
          }
          #endregion
        }

        if (row >= startRow && start < bufferLength)
        {
          _buffer.Slice(start, bufferLength - start).CopyTo(_overflow);
          overflowLength = bufferLength - start;
        }
      }

      if (row++ >= startRow)
      {
        if (overflowLength > 0)
          _processor.AddColumn(_overflow.Slice(0, overflowLength), hasWrapper, hasDoubleWrapper);
        if (_processor.IsAColumnSet() && !_options.RemoveEmptyEntries || !_processor.IsEmpty())
          yield return _processor.GetObject();
        _processor.ClearObject();
      }
    }
  }
}
