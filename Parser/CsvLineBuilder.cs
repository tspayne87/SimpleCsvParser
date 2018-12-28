using System;
using System.Collections.Generic;
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
        /// The current string builder we are processing.
        /// </summary>
        private readonly StringBuilder _builder;
        /// <summary>
        /// The current results we are processing.
        /// </summary>
        private List<string> _results;
        /// <summary>
        /// The current line number for the file.
        /// </summary>
        private ulong _lineNumber;
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

            _options = options;
            _builder = new StringBuilder();
            _results = new List<string>();
            _escaped = false;
            _lineNumber = 0;
        }

        /// <summary>
        /// Method is meant to parse the current char array and build out the lines for the csv object.
        /// </summary>
        /// <param name="current">The current array we are processing.</param>
        /// <param name="isEnd">If we are at the end of the file being processed.</param>
        /// <returns>Returns an IEnumerable with yields that return the results as they are asked for.</returns>
        public IEnumerable<List<string>> Parse(char[] current, bool isEnd)
        {
            for (var i = 0; i < current.Length; ++i)
            {
                if (_escaped)
                { // Deal with if we are in a wrapper.
                    if (current[i] == _options.Wrapper && i < current.Length - 1 && current[i + 1] == _options.Wrapper)
                    {
                        _builder.Append(current[i++]);
                    }
                    else if (current[i] == _options.Wrapper)
                    {
                        _escaped = false;
                    }
                    else
                    {
                        _builder.Append(current[i]);
                    }
                }
                else if (current[i] == _options.Wrapper)
                { // Escape the value and start parsing as such
                    _escaped = true;
                }
                else if (current[i] == _options.Delimiter)
                { // If we encounter a delmitier we want to put the current string into the results and clear the builder.
                    _results.Add(_builder.ToString());
                    _builder.Clear();
                }
                else
                { // Append the new char
                    _builder.Append(current[i]);
                }

                if (_builder.EndsWith(_options.RowDelimiter) || (isEnd && i + 1 == current.Length))
                { // If we are at the end of the line we need to parse the object properly.  And get it ready for the next one.
                    if (_results.Count == 0 && _builder.Length == _options.RowDelimiter.Length)
                    { // If the builder is the same length as the row delimiter than we can skip this set because it is empty.
                        _builder.Clear();
                        continue;
                    }

                    // Update the line number and determine if we need to throw some exceptions.
                    _lineNumber++;
                    if (_results.Count == 0) throw new MalformedException($"No Delimiter found on line {_lineNumber}, is the correct delimiter used in the options?");
                    if (_escaped) throw new MalformedException($"Line {_lineNumber} does not end its data wrapper.");

                    _results.Add((i + 1 == current.Length && !_builder.EndsWith(_options.RowDelimiter)) ? _builder.ToString() : _builder.ToString(0, _builder.Length - _options.RowDelimiter.Length));
                    yield return _results;

                    // Clear up the results.
                    _results = new List<string>();
                    _builder.Clear();
                }
            }
        }
    }
}