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

namespace SimpleCsvParser.Streams
{
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