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
    public class CsvStreamReader<TModel> : IDisposable
        where TModel: class, new()
    {
        /// <summary>
        /// Special options needed to determine what should be done with the parser.
        /// </summary>
        private readonly CsvStreamOptions _options;
        /// <summary>
        /// The converter that will deal with changing the list of strings into an object.
        /// </summary>
        private CsvConverter<TModel> _converter;
        /// <summary>
        /// The reader we will be getting data from.
        /// </summary>
        private readonly RowReader _rowReader;
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
            _stream = stream;
            _rowReader = new RowReader(new CharReader(_stream), _options);
        }

        /// <summary>
        /// Constructor that will deal with files and convert them into a stream.
        /// </summary>
        /// <param name="path">The file path that we need to create a stream from.</param>
        public CsvStreamReader(string path)
        {
            _options = new CsvStreamOptions();
            _stream = File.Open(path, FileMode.Open, FileAccess.Read);
            _rowReader = new RowReader(new CharReader(_stream), _options);
        }

        /// <summary>
        /// Constructor that will deal with streams.
        /// </summary>
        /// <param name="stream">The stream the csv data exists in.</param>
        /// <param name="options">The stream options we need to use for parsing.</param>
        public CsvStreamReader(Stream stream, CsvStreamOptions options)
        {
            _options = options;
            _stream = stream;
            _rowReader = new RowReader(new CharReader(_stream), _options);
        }

        /// <summary>
        /// Constructor that will deal with files and convert them into a stream.
        /// </summary>
        /// <param name="path">The file path that we need to create a stream from.</param>
        /// <param name="options">The stream options we need to use for parsing.</param>
        public CsvStreamReader(string path, CsvStreamOptions options)
        {
            _options = options;
            _stream = File.Open(path, FileMode.Open, FileAccess.Read);
            _rowReader = new RowReader(new CharReader(_stream), _options);
        }
        #endregion

        /// <summary>
        /// Helper method that will convert the streams into a list of char characters based on each of the rows.
        /// </summary>
        /// <returns>Will return an enumerable list of char arrays represented by the row of characters.</returns>
        public IEnumerable<TModel> AsEnumerable()
        {
            _converter = new CsvConverter<TModel>(_options, _options.ParseHeaders ? CsvHelper.Split(_rowReader.AsEnumerable().First(), _options) : null);

            return _rowReader.AsEnumerable()
                .Skip(_options.ParseHeaders ? 1 : 0)
                .Select((row, index) => {
                    var splitRow = CsvHelper.Split(row, _options);
                    if (!_options.RemoveEmptyEntries || splitRow.Where(x => !string.IsNullOrEmpty(x)).Count() != 0)
                        return _converter.Parse(splitRow, index);
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
            _converter = new CsvConverter<TModel>(_options, _options.ParseHeaders ? CsvHelper.Split(_rowReader.AsEnumerable().First(), _options) : null);

            return _rowReader.AsEnumerable()
                .Skip(_options.ParseHeaders ? 1 : 0)
                .AsParallel()
                .AsOrdered()
                .Select((row, index) => {
                    var splitRow = CsvHelper.Split(row, _options);
                    if (!_options.RemoveEmptyEntries || splitRow.Where(x => !string.IsNullOrEmpty(x)).Count() != 0)
                        return _converter.Parse(splitRow, index);
                    return null;
                })
                .Where(x => x != null);
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
}