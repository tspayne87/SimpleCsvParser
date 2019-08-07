using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SimpleCsvParser.Readers;

namespace SimpleCsvParser
{
    public class CsvStreamReader : IDisposable
    {
        /// <summary>
        /// Special options needed to determine what should be done with the parser.
        /// </summary>
        protected readonly CsvStreamOptions _options;
        /// <summary>
        /// The reader we will be getting data from.
        /// </summary>
        internal readonly RowReader _rowReader;
        /// <summary>
        /// The reader we will be using to get the header information.
        /// </summary>
        internal readonly RowReader _headerReader;
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
            : this(stream, new CsvStreamOptions()) { }

        /// <summary>
        /// Constructor that will deal with files and convert them into a stream.
        /// </summary>
        /// <param name="path">The file path that we need to create a stream from.</param>
        public CsvStreamReader(string path)
            : this(File.Open(path, FileMode.Open, FileAccess.Read), new CsvStreamOptions()) { }

        /// <summary>
        /// Constructor that will deal with files and convert them into a stream.
        /// </summary>
        /// <param name="path">The file path that we need to create a stream from.</param>
        /// <param name="options">The stream options we need to use for parsing.</param>
        public CsvStreamReader(string path, CsvStreamOptions options)
            : this(File.Open(path, FileMode.Open, FileAccess.Read), options) { }

        /// <summary>
        /// Constructor that will deal with streams.
        /// </summary>
        /// <param name="stream">The stream the csv data exists in.</param>
        /// <param name="options">The stream options we need to use for parsing.</param>
        public CsvStreamReader(Stream stream, CsvStreamOptions options)
        {
            _options = options;
            _stream = stream;
            _rowReader = new RowReader(new CharReader(_stream), _options.RowDelimiter, _options.Wrapper);
            _headerReader = new RowReader(new CharReader(_stream), _options.HeaderRowDelimiter, _options.Wrapper);
        }
        #endregion

        internal CsvConverter CreateConverter()
        {
            List<string> headers = _options.ParseHeaders ?
                CsvHelper.Split(_headerReader.AsEnumerable().Skip(_options.HeaderRow).FirstOrDefault(), _options.HeaderDelimiter, _options.Wrapper, _options.HeaderRowDelimiter) : null;
            return new CsvConverter(_options, headers);
        }

        internal CsvConverter<TModel> CreateConverter<TModel>()
            where TModel: class, new()
        {
            List<string> headers = _options.ParseHeaders ?
                CsvHelper.Split(_headerReader.AsEnumerable().Skip(_options.HeaderRow).FirstOrDefault(), _options.HeaderDelimiter, _options.Wrapper, _options.HeaderRowDelimiter) : null;
            return new CsvConverter<TModel>(_options, headers);
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

    public class CsvDictionaryStreamReader : CsvStreamReader
    {
        #region Constructors
        /// <summary>
        /// Constructor that will deal with streams.
        /// </summary>
        /// <param name="stream">The stream the csv data exists in.</param>
        public CsvDictionaryStreamReader(Stream stream)
            : base(stream) { }

        /// <summary>
        /// Constructor that will deal with files and convert them into a stream.
        /// </summary>
        /// <param name="path">The file path that we need to create a stream from.</param>
        public CsvDictionaryStreamReader(string path)
            : base(path) { }

        /// <summary>
        /// Constructor that will deal with files and convert them into a stream.
        /// </summary>
        /// <param name="path">The file path that we need to create a stream from.</param>
        /// <param name="options">The stream options we need to use for parsing.</param>
        public CsvDictionaryStreamReader(string path, CsvStreamOptions options)
            : base(path, options) { }

        /// <summary>
        /// Constructor that will deal with streams.
        /// </summary>
        /// <param name="stream">The stream the csv data exists in.</param>
        /// <param name="options">The stream options we need to use for parsing.</param>
        public CsvDictionaryStreamReader(Stream stream, CsvStreamOptions options)
            : base(stream, options) { }
        #endregion

        /// <summary>
        /// Helper method that will convert the streams into a list of char characters based on each of the rows.
        /// </summary>
        /// <returns>Will return an enumerable list of char arrays represented by the row of characters.</returns>
        public IEnumerable<Dictionary<string, string>> AsEnumerable()
        {
            var converter = CreateConverter();
            return _rowReader.AsEnumerable()
                .Skip(_options.DataRow)
                .Select((row, index) => {
                    var splitRow = CsvHelper.Split(row, _options.Delimiter, _options.Wrapper, _options.RowDelimiter);
                    if (!_options.RemoveEmptyEntries || splitRow.Where(x => !string.IsNullOrEmpty(x)).Count() != 0)
                        return converter.ToDictionary(splitRow, index);
                    return null;
                })
                .Where(x => x != null);
        }

        /// <summary>
        /// Helper method that will convert the streams into a list of char characters based on each of the rows.
        /// </summary>
        /// <returns>Will return an enumerable list of char arrays represented by the row of characters.</returns>
        public ParallelQuery<Dictionary<string, string>> AsParallel()
        {
            var converter = CreateConverter();
            return _rowReader.AsEnumerable()
                .Skip(_options.DataRow)
                .AsParallel()
                .AsOrdered()
                .Select((row, index) => {
                    var splitRow = CsvHelper.Split(row, _options.Delimiter, _options.Wrapper, _options.RowDelimiter);
                    if (!_options.RemoveEmptyEntries || splitRow.Where(x => !string.IsNullOrEmpty(x)).Count() != 0)
                        return converter.ToDictionary(splitRow, index);
                    return null;
                })
                .Where(x => x != null);
        }
    }

    public class CsvStreamReader<TModel> : CsvStreamReader
        where TModel: class, new()
    {
        #region Constructors
        /// <summary>
        /// Constructor that will deal with streams.
        /// </summary>
        /// <param name="stream">The stream the csv data exists in.</param>
        public CsvStreamReader(Stream stream)
            : base(stream) { }

        /// <summary>
        /// Constructor that will deal with files and convert them into a stream.
        /// </summary>
        /// <param name="path">The file path that we need to create a stream from.</param>
        public CsvStreamReader(string path)
            : base(path) { }

        /// <summary>
        /// Constructor that will deal with files and convert them into a stream.
        /// </summary>
        /// <param name="path">The file path that we need to create a stream from.</param>
        /// <param name="options">The stream options we need to use for parsing.</param>
        public CsvStreamReader(string path, CsvStreamOptions options)
            : base(path, options) { }

        /// <summary>
        /// Constructor that will deal with streams.
        /// </summary>
        /// <param name="stream">The stream the csv data exists in.</param>
        /// <param name="options">The stream options we need to use for parsing.</param>
        public CsvStreamReader(Stream stream, CsvStreamOptions options)
            : base(stream, options) { }
        #endregion

        /// <summary>
        /// Helper method that will convert the streams into a list of char characters based on each of the rows.
        /// </summary>
        /// <returns>Will return an enumerable list of char arrays represented by the row of characters.</returns>
        public IEnumerable<TModel> AsEnumerable()
        {
            var converter = CreateConverter<TModel>();
            return _rowReader.AsEnumerable()
                .Skip(_options.DataRow)
                .Select((row, index) => {
                    var splitRow = CsvHelper.Split(row, _options.Delimiter, _options.Wrapper, _options.RowDelimiter);
                    if (!_options.RemoveEmptyEntries || splitRow.Where(x => !string.IsNullOrEmpty(x)).Count() != 0)
                        return converter.Parse(splitRow, index);
                    return null;
                })
                .Where(x => x != null);
        }

        /// <summary>
        /// Helper method that will convert the streams into a list of char characters based on each of the rows.
        /// </summary>
        /// <returns>Will return an enumerable list of char arrays represented by the row of characters.</returns>
        public ParallelQuery<TModel> AsParallel()
        {
            var converter = CreateConverter<TModel>();
            return _rowReader.AsEnumerable()
                .Skip(_options.DataRow)
                .AsParallel()
                .AsOrdered()
                .Select((row, index) => {
                    var splitRow = CsvHelper.Split(row, _options.Delimiter, _options.Wrapper, _options.RowDelimiter);
                    if (!_options.RemoveEmptyEntries || splitRow.Where(x => !string.IsNullOrEmpty(x)).Count() != 0)
                        return converter.Parse(splitRow, index);
                    return null;
                })
                .Where(x => x != null);
        }
    }
}