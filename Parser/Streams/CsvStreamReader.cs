using Parser.Readers;
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

namespace SimpleCsvParser.Streams
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
        internal readonly PipelineReader _rowReader;
        /// <summary>
        /// The reader we will be using to get the header information.
        /// </summary>
        internal readonly PipelineReader _headerReader;
        /// <summary>
        /// The stream that we may need to dispose.
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
            : this(File.Open(path, FileMode.Open, FileAccess.Read), new CsvStreamOptions() { CloseStream = false }) { }

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
            _rowReader = new PipelineReader(_stream, _options.RowDelimiter, _options.Wrapper);
            _headerReader = new PipelineReader(_stream, _options.HeaderRowDelimiter, _options.Wrapper);
        }

        #endregion

        //internal CsvConverter CreateConverter(Func<int, string> emptyColumns = null)
        //{
        //    List<string> headers = _options.ParseHeaders ?
        //        CsvHelper.Split(_headerReader.AsEnumerable().Skip(_options.HeaderRow).FirstOrDefault(), _options.HeaderDelimiter, _options.Wrapper, _options.HeaderRowDelimiter, emptyColumns) :
        //        CsvHelper.Split(_rowReader.AsEnumerable().Skip(_options.DataRow).FirstOrDefault(), _options.Delimiter, _options.Wrapper, _options.RowDelimiter, emptyColumns, true);
        //    return new CsvConverter(_options, headers, emptyColumns);
        //}

        //internal CsvConverter<TModel> CreateConverter<TModel>()
        //    where TModel: class, new()
        //{
        //    List<string> headers = _options.ParseHeaders ?
        //        CsvHelper.Split(_headerReader.AsEnumerable().Skip(_options.HeaderRow).FirstOrDefault(), _options.HeaderDelimiter, _options.Wrapper, _options.HeaderRowDelimiter) : null;
        //    return new CsvConverter<TModel>(_options, headers);
        //}

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Method is meant to dispose this object after use.
        /// </summary>
        /// <param name="disposing">If we are needing to dispose the object.</param>
        protected virtual void Dispose(bool disposing)
        {
            //TODO: Justin says we should consider moving this to options. Callers may have their own plans for the stream; 
            // not sure we should force close it on them espeically nondeterministally. However, if we created the stream ourselves 
            // then sure, we're responsible for it
            if (!disposedValue)
            {
                if (disposing && _options.CloseStream)
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