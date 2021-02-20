using System;
using System.Collections.Generic;
using System.IO;
//using System.Linq;

//namespace SimpleCsvParser.Streams
//{
//    public class CsvRowStreamReader : CsvStreamReader
//    {
//        #region Constructors
//        /// <summary>
//        /// Constructor that will deal with streams.
//        /// </summary>
//        /// <param name="stream">The stream the csv data exists in.</param>
//        public CsvRowStreamReader(Stream stream)
//            : base(stream) { }

//        /// <summary>
//        /// Constructor that will deal with files and convert them into a stream.
//        /// </summary>
//        /// <param name="path">The file path that we need to create a stream from.</param>
//        public CsvRowStreamReader(string path)
//            : base(path) { }

//        /// <summary>
//        /// Constructor that will deal with files and convert them into a stream.
//        /// </summary>
//        /// <param name="path">The file path that we need to create a stream from.</param>
//        /// <param name="options">The stream options we need to use for parsing.</param>
//        public CsvRowStreamReader(string path, CsvStreamOptions options)
//            : base(path, options) { }

//        /// <summary>
//        /// Constructor that will deal with streams.
//        /// </summary>
//        /// <param name="stream">The stream the csv data exists in.</param>
//        /// <param name="options">The stream options we need to use for parsing.</param>
//        public CsvRowStreamReader(Stream stream, CsvStreamOptions options)
//            : base(stream, options) { }
//        #endregion

//        /// <summary>
//        /// Helper method to deal with getting all the rows so that they can be processed by outside sources to deal with the data.
//        /// </summary>
//        /// <param name="headerRow">The header row that will need to be parsed to determine the header information while building the objects.</param>
//        /// <returns>Will return an IEnumerable of strings to handle them one at a time.</returns>
//        public IEnumerable<string> AsEnumerable(out string headerRow)
//        {
//          headerRow = _options.ParseHeaders ?
//              _headerReader.AsEnumerable().Skip(_options.HeaderRow).Select(x => new String(x.ToArray())).FirstOrDefault() : null;
//          return _rowReader.AsEnumerable()
//              .Skip(_options.DataRow)
//              .Select(x => new String(x.ToArray()));
//        }
//    }
//}