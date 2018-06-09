using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SimpleCsvParser
{
    public class CsvStreamReader<TModel> : StreamReader
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

        #region Constructors
        #region Constructors Without options
        public CsvStreamReader(Stream stream)
            : base(stream)
        {
            _options = new CsvStreamOptions();
            _line = new CsvLineParser(_options);
        }

        public CsvStreamReader(string path)
            : base(path)
        {
            _options = new CsvStreamOptions();
            _line = new CsvLineParser(_options);
        }

        public CsvStreamReader(Stream stream, bool detectEncodingFromByteOrderMarks)
            : base(stream, detectEncodingFromByteOrderMarks)
        {
            _options = new CsvStreamOptions();
            _line = new CsvLineParser(_options);
        }

        public CsvStreamReader(Stream stream, Encoding encoding)
            : base(stream, encoding)
        {
            _options = new CsvStreamOptions();
            _line = new CsvLineParser(_options);
        }

        public CsvStreamReader(string path, bool detectEncodingFromByteOrderMarks)
            : base(path, detectEncodingFromByteOrderMarks)
        {
            _options = new CsvStreamOptions();
            _line = new CsvLineParser(_options);
        }

        public CsvStreamReader(string path, Encoding encoding)
            : base(path, encoding)
        {
            _options = new CsvStreamOptions();
            _line = new CsvLineParser(_options);
        }

        public CsvStreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks)
            : base(stream, encoding, detectEncodingFromByteOrderMarks)
        {
            _options = new CsvStreamOptions();
            _line = new CsvLineParser(_options);
        }

        public CsvStreamReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks)
            : base(path, encoding, detectEncodingFromByteOrderMarks)
        {
            _options = new CsvStreamOptions();
            _line = new CsvLineParser(_options);
        }

        public CsvStreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
            : base(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize)
        {
            _options = new CsvStreamOptions();
            _line = new CsvLineParser(_options);
        }

        public CsvStreamReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
            : base(path, encoding, detectEncodingFromByteOrderMarks, bufferSize)
        {
            _options = new CsvStreamOptions();
            _line = new CsvLineParser(_options);
        }

        public CsvStreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize, bool leaveOpen)
            : base(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize, leaveOpen)
        {
            _options = new CsvStreamOptions();
            _line = new CsvLineParser(_options);
        }
        #endregion
        #region Constructors With options
        public CsvStreamReader(Stream stream, CsvStreamOptions options)
            : base(stream)
        {
            _options = options;
            _line = new CsvLineParser(_options);
        }

        public CsvStreamReader(string path, CsvStreamOptions options)
            : base(path)
        {
            _options = options;
            _line = new CsvLineParser(_options);
        }

        public CsvStreamReader(Stream stream, bool detectEncodingFromByteOrderMarks, CsvStreamOptions options)
            : base(stream, detectEncodingFromByteOrderMarks)
        {
            _options = options;
            _line = new CsvLineParser(_options);
        }

        public CsvStreamReader(Stream stream, Encoding encoding, CsvStreamOptions options)
            : base(stream, encoding)
        {
            _options = options;
            _line = new CsvLineParser(_options);
        }

        public CsvStreamReader(string path, bool detectEncodingFromByteOrderMarks, CsvStreamOptions options)
            : base(path, detectEncodingFromByteOrderMarks)
        {
            _options = options;
            _line = new CsvLineParser(_options);
        }

        public CsvStreamReader(string path, Encoding encoding, CsvStreamOptions options)
            : base(path, encoding)
        {
            _options = options;
            _line = new CsvLineParser(_options);
        }

        public CsvStreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, CsvStreamOptions options)
            : base(stream, encoding, detectEncodingFromByteOrderMarks)
        {
            _options = options;
            _line = new CsvLineParser(_options);
        }

        public CsvStreamReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, CsvStreamOptions options)
            : base(path, encoding, detectEncodingFromByteOrderMarks)
        {
            _options = options;
            _line = new CsvLineParser(_options);
        }

        public CsvStreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize, CsvStreamOptions options)
            : base(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize)
        {
            _options = options;
            _line = new CsvLineParser(_options);
        }

        public CsvStreamReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize, CsvStreamOptions options)
            : base(path, encoding, detectEncodingFromByteOrderMarks, bufferSize)
        {
            _options = options;
            _line = new CsvLineParser(_options);
        }

        public CsvStreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize, bool leaveOpen, CsvStreamOptions options)
            : base(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize, leaveOpen)
        {
            _options = options;
            _line = new CsvLineParser(_options);
        }
        #endregion
        #endregion

        /// <summary>
        /// Method is meant to read the headers from the csv file and generate the line converter based on that. This method
        /// can only be used as the first method called after creation of the stream otherwise it will throw errors.
        /// </summary>
        /// <exception cref="System.ArgumentException">Is thrown if not the first method called</exception>
        /// <exception cref="SimpleCsvParser.MalformedException">Is thrown if there is a malformity in the string being parsed.</exception>
        /// <returns>Will return a list of headers for outside processing.</returns>
        public List<string> ReadHeader()
        {
            if (_previousCount != -1) throw new ArgumentException("Headers have already been processed cannot be called twice");
            var line = ReadNext();
            if (line == null) return null;
            var headers = _line.Process(line, _lineNumber);
            _previousCount = headers.Count;
            _converter = new CsvLineConverter<TModel>(_options, headers);
            return headers;
        }
        /// <summary>
        /// Method is meant to read a row from the stream and convert the string into an object.
        /// </summary>
        /// <exception cref="SimpleCsvParser.MalformedException">Is thrown if there is a malformity in the string being parsed.</exception>
        /// <returns>Will return the converted object from the current row in the stream.</returns>
        public TModel ReadRow()
        {
            if (_converter == null) _converter = new CsvLineConverter<TModel>(_options, null);
            var line = ReadNext();
            if (line == null) return null;
            var parsed = _line.Process(line, _lineNumber);
            if (_previousCount != -1 && _previousCount != parsed.Count) throw new MalformedException($"Line {_lineNumber} has {parsed.Count} but should have {_previousCount}.");
            _previousCount = parsed.Count;
            return _converter.Process(parsed, _lineNumber);
        }

        /// <summary>
        /// Method is meant to read a row from the stream and convert the string into an object.
        /// </summary>
        /// <param name="isEmpty">Determines if all the fields are empty in the row pulled from the stream.</param>
        /// <exception cref="SimpleCsvParser.MalformedException">Is thrown if there is a malformity in the string being parsed.</exception>
        /// <returns>Will return the converted object from the current row in the stream.</returns>
        public TModel ReadRow(out bool isEmpty)
        {
            if (_converter == null) _converter = new CsvLineConverter<TModel>(_options, null);
            var line = ReadNext();
            isEmpty = false;
            if (line == null) return null;
            var parsed = _line.Process(line, _lineNumber);
            if (_previousCount != -1 && _previousCount != parsed.Count) throw new MalformedException($"Line {_lineNumber} has {parsed.Count} but should have {_previousCount}.");
            isEmpty = parsed.Where(x => string.IsNullOrEmpty(x)).Count() == _previousCount;
            _previousCount = parsed.Count;
            return _converter.Process(parsed, _lineNumber);
        }

        /// <summary>
        /// Helper method that will read the next line and increment the line number we are at.
        /// </summary>
        /// <returns>Returns the string that we read from the stream.</returns>
        private string ReadNext()
        {
            var line = ReadLine();
            _lineNumber++;
            return line;
        }
    }
}