using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
        private readonly CsvLineParser _line;
        /// <summary>
        /// The converter that will deal with changing the list of strings into an object.
        /// </summary>
        private CsvLineConverter<TModel> _converter;
        /// <summary>
        /// Internal use of the line number.
        /// </summary>
        private int _lineNumber = 0;
        /// <summary>
        /// Internal use of the previous count to make sure the csv file is formatted correctly.
        /// </summary>
        private int _previousCount = -1;
        /// <summary>
        /// The reader we will be getting data from.
        /// </summary>
        private StreamReader _reader;
        /// <summary>
        /// The buffered stream that we need to dispose
        /// </summary>

        private BufferedStream _buffered;
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
            _line = new CsvLineParser(_options);
            _stream = stream;
            _buffered = new BufferedStream(stream);
            _reader = new StreamReader(_buffered);
        }

        /// <summary>
        /// Constructor that will deal with files and convert them into a stream.
        /// </summary>
        /// <param name="path">The file path that we need to create a stream from.</param>
        public CsvStreamReader(string path)
        {
            _options = new CsvStreamOptions();
            _line = new CsvLineParser(_options);
            _stream = File.Open(path, FileMode.Open);
            _buffered = new BufferedStream(_stream);
            _reader = new StreamReader(_buffered);
        }

        /// <summary>
        /// Constructor that will deal with streams.
        /// </summary>
        /// <param name="stream">The stream the csv data exists in.</param>
        /// <param name="options">The stream options we need to use for parsing.</param>
        public CsvStreamReader(Stream stream, CsvStreamOptions options)
        {
            _options = options;
            _line = new CsvLineParser(_options);
            _stream = stream;
            _buffered = new BufferedStream(stream);
            _reader = new StreamReader(_buffered);
        }

        /// <summary>
        /// Constructor that will deal with files and convert them into a stream.
        /// </summary>
        /// <param name="path">The file path that we need to create a stream from.</param>
        /// <param name="options">The stream options we need to use for parsing.</param>
        public CsvStreamReader(string path, CsvStreamOptions options)
        {
            _options = options;
            _line = new CsvLineParser(_options);
            _stream = File.Open(path, FileMode.Open);
            _buffered = new BufferedStream(_stream);
            _reader = new StreamReader(_buffered);
        }
        #endregion

        /// <summary>
        /// Read all of the models from the stream.
        /// </summary>
        /// <returns>Will return a list of objects that are parsed from the stream.</returns>
        public List<TModel> ReadAll()
        {
            var items = new List<TModel>();
            Read(x => items.Add(x));
            return items;
        }

        /// <summary>
        /// Will read through each item and call the action for each item
        /// </summary>
        /// <param name="eachItem">The callback that should be used for each item.</param>
        public void Read(Action<TModel> eachItem)
        {
            string line;
            int lineNumber = 0;
            while ((line = _reader.ReadLine()) != null)
            {
                lineNumber++;
                if (_previousCount == -1 && _options.ParseHeaders)
                {
                    var headers = _line.Process(line, lineNumber);
                    _previousCount = headers.Count;
                    _converter = new CsvLineConverter<TModel>(_options, headers);
                }
                else
                {
                    if (_converter == null) _converter = new CsvLineConverter<TModel>(_options, null);
                    var parsed = _line.Process(line, lineNumber);
                    if (_previousCount != -1 && _previousCount != parsed.Count) throw new MalformedException($"Line {_lineNumber} has {parsed.Count} but should have {_previousCount}.");
                    _previousCount = parsed.Count;

                    if (!_options.RemoveEmptyEntries || parsed.Where(x => string.IsNullOrEmpty(x)).Count() != _previousCount)
                        eachItem(_converter.Process(parsed, lineNumber));
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _stream.Dispose();
                    _buffered.Dispose();
                    _reader.Dispose();
                }
                disposedValue = true;
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}