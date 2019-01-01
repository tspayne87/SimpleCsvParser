using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleCsvParser
{
    public class CsvStreamWriter<TModel> : IDisposable
        where TModel: class, new()
    {
        /// <summary>
        /// Special options needed to determine what should be done with the parser.
        /// </summary>
        private readonly CsvStreamOptions _options;
        /// <summary>
        /// The converter that will deal with changing the list of strings into an object.
        /// </summary>
        private readonly CsvLineConverter<TModel> _converter;
        /// <summary>
        /// The writer we will be writing to.
        /// </summary>
        private StreamWriter _writer;
        /// <summary>
        /// The stream that we need to dispose.
        /// </summary>
        private Stream _stream;
        /// <summary>
        /// A buffer of data to save and write in chunks.
        /// </summary>
        private StringBuilder _buffer;

        #region Constructors
        /// <summary>
        /// Constructor that will deal with streams.
        /// </summary>
        /// <param name="stream">The stream the csv data exists in.</param>
        public CsvStreamWriter(Stream stream)
        {
            _options = new CsvStreamOptions();
            _stream = stream;
            _converter = new CsvLineConverter<TModel>(_options, new List<string>());
            _writer = new StreamWriter(_stream, Encoding.UTF8);
            _buffer = new StringBuilder(256 * 1024);
        }

        /// <summary>
        /// Constructor that will deal with files and convert them into a stream.
        /// </summary>
        /// <param name="path">The file path that we need to create a stream from.</param>
        public CsvStreamWriter(string path)
        {
            _options = new CsvStreamOptions();
            _stream = File.Open(path, FileMode.Truncate, FileAccess.Write);
            _converter = new CsvLineConverter<TModel>(_options, new List<string>());
            _writer = new StreamWriter(_stream, Encoding.UTF8);
            _buffer = new StringBuilder(256 * 1024);
        }

        /// <summary>
        /// Constructor that will deal with streams.
        /// </summary>
        /// <param name="stream">The stream the csv data exists in.</param>
        /// <param name="options">The stream options we need to use for parsing.</param>
        public CsvStreamWriter(Stream stream, CsvStreamOptions options)
        {
            _options = options;
            _stream = stream;
            _converter = new CsvLineConverter<TModel>(_options, new List<string>());
            _writer = new StreamWriter(_stream, Encoding.UTF8);
            _buffer = new StringBuilder(256 * 1024);
        }

        /// <summary>
        /// Constructor that will deal with files and convert them into a stream.
        /// </summary>
        /// <param name="path">The file path that we need to create a stream from.</param>
        /// <param name="options">The stream options we need to use for parsing.</param>
        public CsvStreamWriter(string path, CsvStreamOptions options)
        {
            _options = options;
            _stream = File.Open(path, FileMode.Truncate, FileAccess.Write);
            _converter = new CsvLineConverter<TModel>(_options, new List<string>());
            _writer = new StreamWriter(_stream, Encoding.UTF8);
            _buffer = new StringBuilder(256 * 1024);
        }
        #endregion

        /// <summary>
        /// Method is meant to write a line to the stream.
        /// </summary>
        /// <param name="model">The model to write to the stream.</param>
        public void WriteLine(TModel model)
        {
            addLine(_converter.Stringify(model));
        }

        /// <summary>
        /// Method is meant to write the headers of the stream.
        /// </summary>
        public void WriteHeader()
        {
            addLine(_converter.Stringify());
        }

        /// <summary>
        /// Helper method to add the line to a buffer string builder and write to the stream only when needed.
        /// </summary>
        /// <param name="line">The line that needs to be added to the buffer.</param>
        private void addLine(string line)
        {
            if (_buffer.MaxCapacity <= _buffer.Length + line.Length + _options.RowDelimiter.Length)
            {
                _writer.Write(_buffer.ToString());
                _buffer.Clear();
            }

            _buffer.Append(line);
            _buffer.Append(_options.RowDelimiter);
        }

        /// <summary>
        /// Method is meant to flush to the stream that was given to this writer.  And get it ready for reading.
        /// </summary>
        public void Flush()
        {
            if (_buffer.Length > 0)
            {
                _writer.Write(_buffer.ToString());
                _buffer.Clear();
            }
            _writer.Flush();
            _stream.Position = 0;
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
                    Flush();
                    _writer.Dispose();
                    _stream.Dispose();
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