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
    private char[] _buffer;
    private char[] _overflow;
    private readonly char _wrapper;
    private readonly bool _hasOptionWrapper;

    private readonly char _firstDelimiter;
    private readonly char _secondDelimiter;
    private readonly bool _hasOptionDelimiter;
    private readonly bool _hasOneDelimiter;

    private readonly char _firstRowDelimiter;
    private readonly char _secondRowDelimiter;
    private readonly bool _hasOneRowDelimiter;

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

      _wrapper = _options.Wrapper ?? default;
      _hasOptionWrapper = _options.Wrapper != null;

      _firstDelimiter = _options.Delimiter.Length > 0 ? _options.Delimiter[0] : default;
      _secondDelimiter = _options.Delimiter.Length > 1 ? _options.Delimiter[1] : default;
      _hasOptionDelimiter = _options.Delimiter.Length > 0;
      _hasOneDelimiter = _options.Delimiter.Length == 1;

      _firstRowDelimiter = _options.RowDelimiter.Length > 0 ? _options.RowDelimiter[0] : default;
      _secondRowDelimiter = _options.RowDelimiter.Length > 1 ? _options.RowDelimiter[1] : default;
      _hasOneRowDelimiter = _options.RowDelimiter.Length == 1;
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
      bool checkNextWrapper = false;           // If we should check for the next wrapper to determine if it is escaped or not
      int startRow = _options.StartRow;        // Cached value since the git operation can get expensive since we call it so many times
      bool isFirstDelimiterMatched = false;    // Check to see if the first delimiter was matched or not
      bool isFirstRowDelimiterMatched = false;
      bool foundColumn = false;


      while ((bufferLength = reader.Read(_buffer)) > 0)
      {
        start = 0;
        for (int i = 0; i < bufferLength; ++i)
        {
          var current = _buffer[i];

          #region Wrapper Checks
          if (_hasOptionWrapper)
          {
            if (checkNextWrapper)
            {
              checkNextWrapper = false;
              if (_wrapper != current)
                inWrapper = false;
              else
                hasDoubleWrapper = true;
            }
            else if (_wrapper == current)
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
          if (_hasOptionDelimiter && row >= startRow)
          {
            if (_hasOneDelimiter && current == _firstDelimiter)
            {
              foundColumn = true;
              if (overflowLength > 0)
              {
                _processor.AddColumn(_overflow.AsSpan(0, overflowLength).MergeSpan(_buffer.AsSpan(0, i)), hasWrapper, hasDoubleWrapper);
                overflowLength = 0;
              }
              else
              {
                _processor.AddColumn(_buffer.AsSpan(start, i - start), hasWrapper, hasDoubleWrapper);
              }
            }
            else
            {
              if (!isFirstDelimiterMatched && current == _firstDelimiter)
                isFirstDelimiterMatched = true;
              else if (isFirstDelimiterMatched && current == _secondDelimiter)
              {
                foundColumn = true;
                if (i == 0)
                {
                  _processor.AddColumn(_overflow.AsSpan(0, overflowLength - 1), hasWrapper, hasDoubleWrapper);
                  overflowLength = 0;
                }
                else if (overflowLength > 0)
                {
                  _processor.AddColumn(_overflow.AsSpan(0, overflowLength).MergeSpan(_buffer.AsSpan(0, i)), hasWrapper, hasDoubleWrapper);
                  overflowLength = 0;
                }
                else
                {
                  _processor.AddColumn(_buffer.AsSpan(start, i - start - 1), hasWrapper, hasDoubleWrapper);
                }
              }
              else
                isFirstDelimiterMatched = false;

            }

            if (foundColumn)
            {
              isFirstDelimiterMatched = false;
              foundColumn = false;
              hasDoubleWrapper = false;
              hasWrapper = false;
              start = i + 1;
            }
          }
          #endregion

          #region Row Delimiter Checks
          if (_hasOneRowDelimiter && current == _firstRowDelimiter)
          {
            if (row++ >= startRow)
            {
              if (overflowLength > 0)
              {
                _processor.AddColumn(_overflow.AsSpan(0, overflowLength).MergeSpan(_buffer.AsSpan(0, i)), hasWrapper, hasDoubleWrapper);
                overflowLength = 0;
              }
              else
              {
                _processor.AddColumn(_buffer.AsSpan(start, i - start), hasWrapper, hasDoubleWrapper);
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
          else
          {
            if (!isFirstRowDelimiterMatched && current == _firstRowDelimiter)
              isFirstRowDelimiterMatched = true;
            else if (isFirstRowDelimiterMatched && current == _secondRowDelimiter)
            {
              if (row++ >= startRow)
              {
                if (i == 0)
                {
                  _processor.AddColumn(_overflow.AsSpan(0, overflowLength - 1), hasWrapper, hasDoubleWrapper);
                  overflowLength = 0;
                }
                else if (overflowLength > 0)
                {
                  _processor.AddColumn(_overflow.AsSpan(0, overflowLength).MergeSpan(_buffer.AsSpan(0, i)), hasWrapper, hasDoubleWrapper);
                  overflowLength = 0;
                }
                else
                {
                  _processor.AddColumn(_buffer.AsSpan(start, i - start - 1), hasWrapper, hasDoubleWrapper);
                }

                if (_processor.IsAColumnSet() && !_options.RemoveEmptyEntries || !_processor.IsEmpty())
                {
                  yield return _processor.GetObject();
                }
                _processor.ClearObject();
              }

              isFirstRowDelimiterMatched = false;
              hasDoubleWrapper = false;
              hasWrapper = false;
              start = i + 1;
            }
            else
              isFirstRowDelimiterMatched = false;
          }
          #endregion
        }

        if (start < bufferLength)
        {
          _buffer.AsSpan(start, bufferLength - start).CopyTo(_overflow);
          overflowLength = bufferLength - start;
        }
      }

      if (row++ >= startRow)
      {
        if (overflowLength > 0)
          _processor.AddColumn(_overflow.AsSpan(0, overflowLength), hasWrapper, hasDoubleWrapper);
        if (_processor.IsAColumnSet() && !_options.RemoveEmptyEntries || !_processor.IsEmpty())
          yield return _processor.GetObject();
        _processor.ClearObject();
      }
    }
  }
}
