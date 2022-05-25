﻿using SimpleCsvParser;
using System.Text;
using System.Collections.Generic;
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
        public PipelineReader(Stream stream, ParseOptions options, IObjectProcessor<T> processor)
        {
            if (options.Wrapper != null && options.RowDelimiter.IndexOf(options.Wrapper.Value) > -1)
                throw new ArgumentException("Row delimiter cannot contain a value from Wrapper or Delimiter");
            if (options.RowDelimiter.IndexOf(options.Delimiter) > -1)
                throw new ArgumentException("Row delimiter cannot contain a value from Wrapper or Delimiter");
            if (options.Wrapper.ToString() == options.Delimiter)
                throw new ArgumentException("Wrapper and Delimiter cannot be equal");

            _stream = stream;
            _processor = processor;
            _options = options;
            _doubleWrap = $"{_options.Wrapper.Value}{_options.Wrapper.Value}";
            _singleWrap = _options.Wrapper.Value.ToString();
        }

        internal void Parse(Action<T> rowHandler, CancellationToken cancellationToken)
        {
            _stream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(_stream, Encoding.UTF8, true, 4 * 1024, true);
            char[] buffer = new char[4 * 1024];                 // Create a buffer to store the characters loaded from the stream.
            StringBuilder overflow = new StringBuilder();       // The overflow buffer to catch anything that was added before
            int bufferLength;                                   // The current buffer length, the start of the column the end of the column
            int start = 0;                                      // The current starting position of the column
            bool inWrapper = false;                             // If we are currently in a wrapper or not
            char firstRowDelimiter = _options.RowDelimiter[0];  // Grab the first character from the row delimiter since it is much faster to check char to char
            char firstDelimiter = _options.Delimiter[0];        // Grab the first character from the delimiter since it is much faster to check char to char
            uint row = 0;// The current row we are working on

            int lenRowDelimiter = _options.RowDelimiter.Length;
            int lenColDelimiter = _options.Delimiter.Length;

            while ((bufferLength = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                start = 0;
                for (int i = 0; i < bufferLength; ++i)
                {
                    if (_options.Wrapper != null && _options.Wrapper.Value == buffer[i])
                    {
                        if (inWrapper && (i + 1 >= bufferLength || buffer[i + 1] != _options.Wrapper.Value))
                            inWrapper = false;
                        else if (inWrapper && (i + 1 >= bufferLength || buffer[i + 1] == _options.Wrapper.Value))
                            i++;
                        else if (!inWrapper)
                            inWrapper = true;
                    }
                    if (inWrapper)
                        continue;

                    if (row >= _options.StartRow && firstDelimiter == buffer[i] && (lenColDelimiter == 1 || _options.Delimiter.EqualsCharArray(buffer, i, i + lenColDelimiter)))
                    {
                        AddColumn(overflow, buffer, start, i);
                        start = i + lenColDelimiter;
                    }
                    else if (firstRowDelimiter == buffer[i] && (lenRowDelimiter == 1 || _options.RowDelimiter.EqualsCharArray(buffer, i, i + lenRowDelimiter)))
                    {
                        if (row++ >= _options.StartRow)
                        {
                            AddColumn(overflow, buffer, start, i);
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

                overflow.Append(buffer, start, bufferLength - start);
                start = 0;
            }

            if (row++ >= _options.StartRow)
            {
                if (overflow.Length > 0)
                {
                    if (_options.Wrapper != null && overflow.Length > 0 && overflow[0] == _options.Wrapper.Value)
                        _processor.AddColumn(overflow.Replace(_doubleWrap, _singleWrap).ToString(1, overflow.Length - 2));
                    else
                        _processor.AddColumn(overflow.ToString());
                }
                if (_processor.IsAColumnSet() && !_options.RemoveEmptyEntries || !_processor.IsEmpty())
                    rowHandler(_processor.GetObject());
                _processor.ClearObject();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddColumn(StringBuilder overflow, char[] buffer, int start, int i)
        {
            if (_options.Wrapper != null && buffer[start] == _options.Wrapper.Value)
            {
                if (buffer != null)
                    overflow.Append(buffer, start + 1, i - start - 2);
                _processor.AddColumn(overflow.Replace(_doubleWrap, _singleWrap).ToString());
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
