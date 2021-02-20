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
        private static ReadOnlySpan<byte> NewLine => new[] { (byte)'\r', (byte)'\n' };

        private Stream _stream;
        private Stream stream;
        private string rowDelimiter;
        private char? wrapper;

        public PipelineReader(Stream stream, string rowDelimiter, char? wrapper)
        {
            this.stream = stream;
            this.rowDelimiter = rowDelimiter;
            this.wrapper = wrapper;
        }

        public async Task<string> ReadLineUsingPipelineVer2Async()
        {
            _stream.Seek(0, SeekOrigin.Begin);

            var reader = PipeReader.Create(_stream, new StreamPipeReaderOptions(leaveOpen: true));
            string str;

            while (true)
            {
                ReadResult result = await reader.ReadAsync();
                ReadOnlySequence<byte> buffer = result.Buffer;

                str = ProcessLine(ref buffer);

                reader.AdvanceTo(buffer.Start, buffer.End);

                if (result.IsCompleted) break;
            }

            await reader.CompleteAsync();
            return str;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string ProcessLine(ref ReadOnlySequence<byte> buffer)
        {
            string str = null;

            if (buffer.IsSingleSegment)
            {
                var span = buffer.FirstSpan;
                int consumed;
                while (span.Length > 0)
                {
                    var newLine = span.IndexOf(NewLine);

                    if (newLine == -1) break;

                    var line = span.Slice(0, newLine);
                    str = Encoding.UTF8.GetString(line);

                    // simulate string processing
                    str = str.AsSpan().Slice(0, 5).ToString();

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
                    while (sequenceReader.TryReadTo(out var line, NewLine))
                    {
                        str = Encoding.UTF8.GetString(line.ToArray());//TODO: we don't need this in .net 5 and it is wasting mem
                        // simulate string processing
                        str = str.AsSpan().Slice(0, 5).ToString();
                    }

                    buffer = buffer.Slice(sequenceReader.Position);
                    sequenceReader.Advance(buffer.Length);
                }
            }

            return str;
        }

        internal IEnumerable<TModel> AsEnumerable<TModel>(CsvConverter<TModel> converter, CsvStreamOptions options) where TModel : class, new()
        {
            throw new NotImplementedException();
        }
    }
}
