using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SimpleCsvParser.Options;
using SimpleCsvParser.Processors;
using Parser.Readers;
using System.Threading;

namespace SimpleCsvParser.Streams
{
  public class CsvStreamModelReader<TModel> : IDisposable
      where TModel : class, new()
  {
    /// <summary>
    /// The reader we will be getting data from.
    /// </summary>
    private PipelineReader<TModel> _rowReader;

    /// <summary>
    /// The reader we will be getting data from.
    /// </summary>
    private readonly PipelineReader<IList<string>> _headerReader;

    /// <summary>
    /// The options used for the reader
    /// </summary>
    private readonly CsvStreamReaderWithHeaderOptions _options;

    /// <summary>
    /// The stream that we may need to dispose.
    /// </summary>
    private Stream _stream;

    #region Constructors
    /// <summary>
    /// Constructor that will deal with streams.
    /// </summary>
    /// <param name="stream">The stream the csv data exists in.</param>
    public CsvStreamModelReader(Stream stream)
        : this(stream, new CsvStreamReaderWithHeaderOptions()) { }

    /// <summary>
    /// Constructor that will deal with files and convert them into a stream.
    /// </summary>
    /// <param name="path">The file path that we need to create a stream from.</param>
    public CsvStreamModelReader(string path)
        : this(File.Open(path, FileMode.Open, FileAccess.Read), new CsvStreamReaderWithHeaderOptions()) { }

    /// <summary>
    /// Constructor that will deal with files and convert them into a stream.
    /// </summary>
    /// <param name="path">The file path that we need to create a stream from.</param>
    /// <param name="options">The stream options we need to use for parsing.</param>
    public CsvStreamModelReader(string path, CsvStreamReaderWithHeaderOptions options)
        : this(File.Open(path, FileMode.Open, FileAccess.Read), options) { }

    /// <summary>
    /// Constructor that will deal with streams.
    /// </summary>
    /// <param name="stream">The stream the csv data exists in.</param>
    /// <param name="options">The stream options we need to use for parsing.</param>
    public CsvStreamModelReader(Stream stream, CsvStreamReaderWithHeaderOptions options)
    {
      _stream = stream;
      _options = options;

      var headerOptions = new ParseOptions()
      {
        Delimiter = options.HeaderDelimiter,
        Wrapper = options.HeaderWrapper,
        RowDelimiter = options.HeaderRowDelimiter,
        RemoveEmptyEntries = options.HeaderRemoveEmptyEntries,
        StartRow = options.HeaderStartRow
      };
      _headerReader = new PipelineReader<IList<string>>(_stream, headerOptions, new ListStringProcessor(headerOptions.Wrapper ?? default));
    }

    #endregion

    /// <summary>
    /// Load Headers
    /// </summary>
    public void LoadHeaders()
    {
      var rowOptions = new ParseOptions()
      {
        Delimiter = _options.ColumnDelimiter,
        Wrapper = _options.EscapeChar,
        RowDelimiter = _options.RowDelimiter,
        RemoveEmptyEntries = _options.RemoveEmptyEntries,
        StartRow = _options.StartRow
      };
      if (!_options.IgnoreHeaders)
      {
        var ct = new CancellationTokenSource();
        var headers = _headerReader.Parse().FirstOrDefault().ToList();
        _rowReader = new PipelineReader<TModel>(_stream, rowOptions, new TModelProcessor<TModel>(headers, rowOptions.Wrapper ?? default));
      }
      else
        _rowReader = new PipelineReader<TModel>(_stream, rowOptions, new TModelProcessor<TModel>(null, rowOptions.Wrapper ?? default));
    }

    public IEnumerable<TModel> Parse()
    {
      return _rowReader.Parse();
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