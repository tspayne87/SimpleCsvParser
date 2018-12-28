using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleCsvParser
{
    public class CsvStreamReader<TModel> : IDisposable
        where TModel: class, new()
    {
        /// <summary>
        /// Special options needed to determine what should be done with the parser.
        /// </summary>
        private readonly CsvStreamOptions _options;
        /// <summary>
        /// The line parser to convert each line to a list of string that will be parsed.
        /// </summary>
        private readonly CsvLineBuilder _lineBuilder;
        /// <summary>
        /// The converter that will deal with changing the list of strings into an object.
        /// </summary>
        private CsvLineConverter<TModel> _converter;
        /// <summary>
        /// Internal use of the previous count to make sure the csv file is formatted correctly.
        /// </summary>
        private int _previousCount = -1;
        /// <summary>
        /// The reader we will be getting data from.
        /// </summary>
        private StreamReader _reader;
        /// <summary>
        /// The stream that we need to dispose.
        /// </summary>
        private Stream _stream;

        #region Constructors
        /// <summary>
        /// Constructor that will deal with streams.
        /// </summary>
        /// <param name="stream">The stream the csv data exists in.</param>
        public CsvStreamReader(Stream stream)
        {
            _options = new CsvStreamOptions();
            _lineBuilder = new CsvLineBuilder(_options);
            _stream = stream;
            _reader = new StreamReader(_stream);
        }

        /// <summary>
        /// Constructor that will deal with files and convert them into a stream.
        /// </summary>
        /// <param name="path">The file path that we need to create a stream from.</param>
        public CsvStreamReader(string path)
        {
            _options = new CsvStreamOptions();
            _lineBuilder = new CsvLineBuilder(_options);
            _stream = File.Open(path, FileMode.Open, FileAccess.Read);
            _reader = new StreamReader(_stream);
        }

        /// <summary>
        /// Constructor that will deal with streams.
        /// </summary>
        /// <param name="stream">The stream the csv data exists in.</param>
        /// <param name="options">The stream options we need to use for parsing.</param>
        public CsvStreamReader(Stream stream, CsvStreamOptions options)
        {
            _options = options;
            _lineBuilder = new CsvLineBuilder(_options);
            _stream = stream;
            _reader = new StreamReader(_stream);
        }

        /// <summary>
        /// Constructor that will deal with files and convert them into a stream.
        /// </summary>
        /// <param name="path">The file path that we need to create a stream from.</param>
        /// <param name="options">The stream options we need to use for parsing.</param>
        public CsvStreamReader(string path, CsvStreamOptions options)
        {
            _options = options;
            _lineBuilder = new CsvLineBuilder(_options);
            _stream = File.Open(path, FileMode.Open, FileAccess.Read);
            _reader = new StreamReader(_stream);
        }
        #endregion

        /// <summary>
        /// Converts the stream into a enumerable list of models.
        /// </summary>
        /// <param name="eachItem">The callback that should be used for each item.</param>
        public IEnumerable<TModel> AsEnumerable()
        {
            var index = 0;
            foreach (var line in InternalRead())
            {
                if (_converter == null)
                {
                    if (_options.ParseHeaders)
                    {
                        _previousCount = line.Count;
                        _converter = new CsvLineConverter<TModel>(_options, line);
                    }
                    else
                    {
                        _converter = new CsvLineConverter<TModel>(_options, null);
                        var model = ReadLine(line, ++index);
                        if (model != null) yield return model;
                    }
                }
                else
                {
                    var model = ReadLine(line, ++index);
                    if (model != null) yield return model;
                }
            }
        }

        /// <summary>
        /// Helper method that will convert the streams into a list of char characters based on each of the rows.
        /// </summary>
        /// <returns>Will return an enumerable list of char arrays represented by the row of characters.</returns>
        private IEnumerable<List<string>> InternalRead()
        {
            char[] buffer = new char[256 * 1024]; // Create a buffer to store the characters loaded from the stream.
            while (_reader.Peek() != -1)
            {
                var blockIndex = _reader.ReadBlock(buffer, 0, buffer.Length);
                var isEnd = _reader.Peek() == -1;
                foreach(var line in _lineBuilder.Parse(isEnd ? buffer.Take(blockIndex).ToArray() : buffer, isEnd))
                {
                    yield return line;
                }
            }
        }

        /// <summary>
        /// Helper method is meant to read a char array and convert it to the desired object.
        /// </summary>
        /// <param name="line">The char array we need to parse.</param>
        /// <param name="lineNumber">The current line number this line is in the file or string.</param>
        /// <returns>Will return the desired object or null if nothing needs to be processed.</returns>
        private TModel ReadLine(List<string> line, int lineNumber)
        {
            if (_previousCount != -1 && _previousCount != line.Count) throw new MalformedException($"Line {lineNumber} has {line.Count} but should have {_previousCount}.");
            _previousCount = line.Count;
            if (!_options.RemoveEmptyEntries || line.Where(x => string.IsNullOrEmpty(x)).Count() != _previousCount)
                return _converter.Parse(line, lineNumber);
            return null;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Method is meant to dispose this object after use.
        /// </summary>
        /// <param name="disposing">If we are needing to dispose the object.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _stream.Dispose();
                    _reader.Dispose();
                }
                disposedValue = true;
            }
        }

        /// <summary>
        /// Method is meant to dispose of this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}