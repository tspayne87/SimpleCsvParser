using SimpleCsvParser;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Readers
{
    internal class PipelineReader
    {
        private static ReadOnlySpan<byte> NewLine => new[] { (byte)'\r', (byte)'\n' }; //TODO: this is windows newline only

        private Stream stream;
        private string rowDelimiter;
        private char? wrapper;
        private int _counter = 0;

        public PipelineReader(Stream stream, string rowDelimiter, char? wrapper)
        {
            this.stream = stream;
            this.rowDelimiter = rowDelimiter;
            this.wrapper = wrapper;
        }

        //public async Task<string> ReadLineUsingPipelineVer2Async()
        ////{
        ////    _stream.Seek(0, SeekOrigin.Begin);

        ////    var reader = PipeReader.Create(_stream, new StreamPipeReaderOptions(leaveOpen: true));
        ////    string str;

        ////    while (true)
        ////    {
        ////        ReadResult result = await reader.ReadAsync();
        ////        ReadOnlySequence<byte> buffer = result.Buffer;

        ////        str = ProcessLine(ref buffer);

        ////        reader.AdvanceTo(buffer.Start, buffer.End);

        ////        if (result.IsCompleted) break;
        ////    }

        ////    await reader.CompleteAsync();
        ////    return str;
        ////}

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private static TModel ProcessLine<TModel>(ref ReadOnlySequence<byte> buffer, CsvConverter<TModel> converter) where TModel : class, new()
        //{
        //    TModel model = default;

        //    if (buffer.IsSingleSegment)
        //    {
        //        var span = buffer.FirstSpan;
        //        int consumed;
        //        while (span.Length > 0)
        //        {
        //            var rowEnd = span.IndexOf(NewLine);
        //            if (rowEnd == -1) 
        //                break;
        //            var row = span.Slice(0, rowEnd);

        //            consumed = row.Length + NewLine.Length;
        //            span = span.Slice(consumed);
        //            buffer = buffer.Slice(consumed);

        //            return converter.Parse(row, -1);
        //        }
        //    }
        //    else
        //    {
        //        var sequenceReader = new SequenceReader<byte>(buffer);

        //        while (!sequenceReader.End)
        //        {
        //            while (sequenceReader.TryReadTo(out var line, NewLine))
        //            {
        //                //str = Encoding.UTF8.GetString(line.ToArray());//TODO: we don't need this in .net 5 and it is wasting mem
        //                //// simulate string processing
        //                //str = str.AsSpan().Slice(0, 5).ToString();
        //                //var row = span.Slice(0, rowEnd);
        //                //return converter.Parse(line., -1);
        //            }

        //            buffer = buffer.Slice(sequenceReader.Position);
        //            sequenceReader.Advance(buffer.Length);
        //            return default;
        //        }
        //    }
        //    return default;
        //    //throw new Exception("oops there it is");
        //}

        //internal async IAsyncEnumerable<TModel> AsEnumerable<TModel>(CsvConverter<TModel> converter, CsvStreamOptions options) where TModel : class, new()
        //{
        //    stream.Seek(0, SeekOrigin.Begin);
        //    //var converter = new CsvConverter<TModel>(options);
        //    var reader = PipeReader.Create(stream, new StreamPipeReaderOptions(leaveOpen: true));

        //    TModel currentModel = new TModel();
        //    while (true)//looping over chunks of bytes
        //    {
        //        ReadResult result = await reader.ReadAsync();
        //        ReadOnlySequence<byte> buffer = result.Buffer;

        //        var eol = buffer.PositionOf((byte)'\n');
        //        for(int i=0; i < buffer.Length; i++)
        //        {
        //            //loops through buffer one char at a time;
        //            //TODO: probably faster to to register size at a time eg 64 bits
        //           if(buffer.)
        //        }

        //        //str = xx;
        //        yield return ProcessLine(ref buffer, converter);
        //        //yield return new TModel();

        //        reader.AdvanceTo(buffer.Start, buffer.End);

        //        if (result.IsCompleted) 
        //            break;
        //    }

        //    await reader.CompleteAsync();
        //}








        internal async IAsyncEnumerable<TModel> AsEnumerable<TModel>(CsvConverter<TModel> converter, CsvStreamOptions options) where TModel : class, new()
        {
            stream.Seek(0, SeekOrigin.Begin);
            var reader = PipeReader.Create(stream, new StreamPipeReaderOptions(leaveOpen: true));
            List<TModel> dumbList = new List<TModel>();
            while (true)
            {
                ReadResult result = await reader.ReadAsync().ConfigureAwait(false);
                ReadOnlySequence<byte> buffer = result.Buffer;













                ProcessRows(ref buffer, converter, dumbList);
                for (int i = 0; i < dumbList.Count; i++)
                    yield return dumbList[i];
                dumbList.Clear();

















                reader.AdvanceTo(buffer.Start, buffer.End);
                if (result.IsCompleted) break;
            }

            //await Task.WhenAll(_tasks).ConfigureAwait(false);

            await reader.CompleteAsync().ConfigureAwait(false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessRows<TModel>(ref ReadOnlySequence<byte> buffer, CsvConverter<TModel> converter, List<TModel> dumbList) where TModel : class, new()
        {
            if (buffer.IsSingleSegment)
            {
                var span = buffer.FirstSpan;
                int consumed;
                while (span.Length > 0)
                {
                    var newLine = span.IndexOf(NewLine);

                    if (newLine == -1) break;

                    var line = span.Slice(0, newLine);

                    dumbList.Add(converter.Parse(line, ++_counter));

                    consumed = line.Length + NewLine.Length;
                    span = span.Slice(consumed);
                    buffer = buffer.Slice(consumed);
                }
            }
            else
            {
                var sequenceReader = new SequenceReader<byte>(buffer);

                while (!sequenceReader.End)
                {
                    while (sequenceReader.TryReadTo(out ReadOnlySequence<byte> line, NewLine))
                    {
                            Span<byte> bytes = stackalloc byte[(int)line.Length];
                            line.CopyTo(bytes);
                            dumbList.Add(converter.Parse(bytes, ++_counter));
                    }

                    buffer = buffer.Slice(sequenceReader.Position);
                    sequenceReader.Advance(buffer.Length);
                }
            }
        }
    }
}
