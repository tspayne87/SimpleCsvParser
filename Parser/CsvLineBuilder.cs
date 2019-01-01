using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleCsvParser
{
    internal class CsvLineBuilder
    {
        /// <summary>
        /// Special options needed to determine what should be done with the parser.
        /// </summary>
        private readonly CsvStreamOptions _options;
        /// <summary>
        /// The row break char array that we need to check against.
        /// </summary>
        private readonly char[] _rowBreak;
        /// <summary>
        /// The current row break we need to check on the incoming string.
        /// </summary>
        private readonly char[] _currentRowBreak;
        /// <summary>
        /// The current queue we need to return for the rows.
        /// </summary>
        private Queue<char> _q;
        /// <summary>
        /// The current results we are processing.
        /// </summary>
        private List<string> _results;
        /// <summary>
        /// If we are escaped with the wrapper options.
        /// </summary>
        private bool _escaped;

        /// <summary>
        /// Constructor is meant to build out the line builder class.
        /// </summary>
        /// <param name="options">The options this converter should use when building out the objects.</param>
        public CsvLineBuilder(CsvStreamOptions options)
        {
            if (options.RowDelimiter.IndexOf(options.Wrapper) > -1 || options.RowDelimiter.IndexOf(options.Delimiter) > -1)
                throw new ArgumentException("Row delimiter cannot contain a value from Wrapper or Delimiter");
            if (options.Wrapper == options.Delimiter)
                throw new ArgumentException("Wrapper and Delimiter cannot be equal");

            _q = new Queue<char>();
            _options = options;
            _rowBreak = _options.RowDelimiter.ToCharArray();
            _currentRowBreak = new char[_rowBreak.Length];
            _results = new List<string>();
            _escaped = false;
        }

        /// <summary>
        /// Method is meant to parse the current char array and build out the lines for the csv object.
        /// </summary>
        /// <param name="current">The current array we are processing.</param>
        /// <param name="isEnd">If we are at the end of the file being processed.</param>
        /// <returns>Returns an IEnumerable with yields that return the results as they are asked for.</returns>
        public IEnumerable<Queue<char>> SplitRow(char[] current, bool isEnd)
        {
            for (var i = 0; i < current.Length; ++i)
            {
                if (_escaped)
                { // Deal with if we are in a wrapper.
                    if (current[i] == _options.Wrapper && i < current.Length - 1 && current[i + 1] == _options.Wrapper)
                    {
                        _q.Enqueue(current[i]);
                        _q.Enqueue(current[i++]);
                    }
                    else if (current[i] == _options.Wrapper)
                    {
                        _q.Enqueue(current[i]);
                        _escaped = false;
                    }
                    else
                    {
                        _q.Enqueue(current[i]);
                    }
                }
                else if (current[i] == _options.Wrapper)
                { // Escape the value and start parsing as such
                    _q.Enqueue(current[i]);
                    _escaped = true;
                }
                else
                { // Append the new char
                    _q.Enqueue(current[i]);
                }

                // Update the current row breaks.
                for (var j = 1; j < _currentRowBreak.Length; ++j)
                {
                    _currentRowBreak[j - 1] = _currentRowBreak[j];
                }
                _currentRowBreak[_currentRowBreak.Length - 1] = current[i];

                if (!_escaped)
                {
                    var end = true;
                    for (var j = 0; end && j < _currentRowBreak.Length; ++j)
                    {
                        end = end && _currentRowBreak[j] == _rowBreak[j];
                    }

                    if (end || (isEnd && i + 1 == current.Length))
                    {
                        if (_q.Count == 0 || _q.Count == _rowBreak.Length)
                        { // If the builder is the same length as the row delimiter than we can skip this set because it is empty.
                            _q = new Queue<char>();
                            continue;
                        }
                        yield return _q;
                        _q = new Queue<char>();
                    }
                }
            }
            if (isEnd && _escaped) throw new MalformedException("Stream was escaped and never unescaped.");
            if (isEnd && _q.Count > 0) throw new MalformedException($"End of stream and data still exists, this happens if there is an escaped value that was not closed.");
        }

        /// <summary>
        /// Method is meant to parse the current char array and build out the lines for the csv object.
        /// </summary>
        /// <param name="current">The current array we are processing.</param>
        /// <param name="isEnd">If we are at the end of the file being processed.</param>
        /// <returns>Returns an IEnumerable with yields that return the results as they are asked for.</returns>
        public List<string> SplitColumn(Queue<char> q)
        {
            var escaped = false;
            var noDelimiter = true;
            var results = new List<string>();
            var builder = new StringBuilder();
            while(q.Count > 0)
            {
                var current = q.Dequeue();
                if (escaped)
                { // Deal with if we are in a wrapper.
                    if (current == _options.Wrapper && q.Count > 0 && q.Peek() == _options.Wrapper)
                    {
                        builder.Append(q.Dequeue());
                    }
                    else if (current == _options.Wrapper)
                    {
                        escaped = false;
                    }
                    else
                    {
                        builder.Append(current);
                    }
                }
                else if (current == _options.Wrapper)
                { // Escape the value and start parsing as such
                    escaped = true;
                }
                else if (current == _options.Delimiter)
                { // If we encounter a delmitier we want to put the current string into the results and clear the builder.
                    noDelimiter = false;
                    results.Add(builder.ToString());
                    builder.Clear();
                }
                else
                { // Append the new char
                    builder.Append(current);
                }
            }

            if (noDelimiter) throw new MalformedException("No Delimiter was found.");
            if (builder.Length > 0)
            {
                results.Add(builder.EndsWith(_options.RowDelimiter) ? builder.ToString(0, builder.Length - _options.RowDelimiter.Length) : builder.ToString());
            }

            return results;
        }

        // /// <summary>
        // /// Method is meant to parse the current char array and build out the lines for the csv object.
        // /// </summary>
        // /// <param name="current">The current array we are processing.</param>
        // /// <param name="isEnd">If we are at the end of the file being processed.</param>
        // /// <returns>Returns an IEnumerable with yields that return the results as they are asked for.</returns>
        // public IEnumerable<List<string>> Parse(char[] current, bool isEnd)
        // {
        //     for (var i = 0; i < current.Length; ++i)
        //     {
        //         if (_escaped)
        //         { // Deal with if we are in a wrapper.
        //             if (current[i] == _options.Wrapper && i < current.Length - 1 && current[i + 1] == _options.Wrapper)
        //             {
        //                 _builder.Append(current[i++]);
        //             }
        //             else if (current[i] == _options.Wrapper)
        //             {
        //                 _escaped = false;
        //             }
        //             else
        //             {
        //                 _builder.Append(current[i]);
        //             }
        //         }
        //         else if (current[i] == _options.Wrapper)
        //         { // Escape the value and start parsing as such
        //             _escaped = true;
        //         }
        //         else if (current[i] == _options.Delimiter)
        //         { // If we encounter a delmitier we want to put the current string into the results and clear the builder.
        //             _results.Add(_builder.ToString());
        //             _builder.Clear();
        //         }
        //         else
        //         { // Append the new char
        //             _builder.Append(current[i]);
        //         }

        //         if (_builder.EndsWith(_options.RowDelimiter) || (isEnd && i + 1 == current.Length))
        //         { // If we are at the end of the line we need to parse the object properly.  And get it ready for the next one.
        //             if (_results.Count == 0 && _builder.Length == _options.RowDelimiter.Length)
        //             { // If the builder is the same length as the row delimiter than we can skip this set because it is empty.
        //                 _builder.Clear();
        //                 continue;
        //             }

        //             // Update the line number and determine if we need to throw some exceptions.
        //             _lineNumber++;
        //             if (_results.Count == 0) throw new MalformedException($"No Delimiter found on line {_lineNumber}, is the correct delimiter used in the options?");
        //             if (_escaped) throw new MalformedException($"Line {_lineNumber} does not end its data wrapper.");

        //             _results.Add((i + 1 == current.Length && !_builder.EndsWith(_options.RowDelimiter)) ? _builder.ToString() : _builder.ToString(0, _builder.Length - _options.RowDelimiter.Length));
        //             yield return _results;

        //             // Clear up the results.
        //             _results = new List<string>();
        //             _builder.Clear();
        //         }
        //     }
        // }
    }
}