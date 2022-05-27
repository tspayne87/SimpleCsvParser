using Parser.Readers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using SimpleCsvParser.Processors;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using SimpleCsvParser.Options;

namespace SimpleCsvParser.Streams
{
    public class CsvStreamReader : IDisposable
    {
        /// <summary>
        /// The reader we will be getting data from.
        /// </summary>
        internal readonly PipelineReader<IList<string>> _reader;

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
            : this(stream, new CsvStreamReaderOptions()) { }

        /// <summary>
        /// Constructor that will deal with files and convert them into a stream.
        /// </summary>
        /// <param name="path">The file path that we need to create a stream from.</param>
        public CsvStreamReader(string path)
            : this(File.Open(path, FileMode.Open, FileAccess.Read), new CsvStreamReaderOptions() { }) { }

        /// <summary>
        /// Constructor that will deal with files and convert them into a stream.
        /// </summary>
        /// <param name="path">The file path that we need to create a stream from.</param>
        /// <param name="options">The stream options we need to use for parsing.</param>
        public CsvStreamReader(string path, CsvStreamReaderOptions options)
            : this(File.Open(path, FileMode.Open, FileAccess.Read), options) { }

        /// <summary>
        /// Constructor that will deal with streams.
        /// </summary>
        /// <param name="stream">The stream the csv data exists in.</param>
        /// <param name="options">The stream options we need to use for parsing.</param>
        public CsvStreamReader(Stream stream, CsvStreamReaderOptions options)
        {
            _stream = stream;

            var parseOptions = new ParseOptions()
            {
                Delimiter = options.Delimiter,
                Wrapper = options.Wrapper,
                RowDelimiter = options.RowDelimiter,
                RemoveEmptyEntries = options.RemoveEmptyEntries,
                StartRow = options.StartRow
            };
            _reader = new PipelineReader<IList<string>>(_stream, parseOptions, new ListStringProcessor(parseOptions.Wrapper ?? default));
        }

        #endregion

        public void Parse(Action<IList<string>> rowHandler, CancellationToken? cancellationToken = null)
        {
            _reader.Parse(rowHandler, cancellationToken ?? CancellationToken.None);
        }

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