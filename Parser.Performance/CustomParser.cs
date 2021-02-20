using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Parser.Performance
{
    public class CustomParser
    {
        static byte[] NewLine = Encoding.ASCII.GetBytes(Environment.NewLine);
        public static List<DataModel> ParseFile(Stream stream)
        {
            var doubles = new List<double>();
            var ints = new List<int>();
            var result = new List<DataModel>(1000);
            double curMeasurement = 0.0d;
            int curMeasurement2 = 0;
            ReadOnlySpan<byte> newLineSpan = NewLine.AsSpan();
            var convert = new SimpleCsvParser.CsvConverter<DataModel>(new SimpleCsvParser.CsvStreamOptions() { ParseHeaders = false });
            const int bufferSize = 10 * 4096;
            byte[] buffer = new byte[bufferSize];
            int readBytes = 0;

            int lastLineSize = 0;
            while ((readBytes = stream.Read(buffer, lastLineSize, bufferSize - lastLineSize)) != 0)
            {
                Span<byte> bufferSpan = new Span<byte>(buffer, 0, readBytes + lastLineSize);

                if (bufferSpan.StartsWith(Encoding.UTF8.GetPreamble())) // skip byte order mark //TODO: lift this
                {
                    bufferSpan = bufferSpan.Slice(Encoding.UTF8.GetPreamble().Length);
                }

                int newLineStart = 0;
                while ((newLineStart = bufferSpan.IndexOf(newLineSpan)) > 0)
                {
                    var model = convert.Parse(bufferSpan.Slice(0, newLineStart), -1);
                    //var model = convert.Parse(bufferSpan.Slice(0, newLineStart).ToString().Split(','), -1);//TODO: so much wasted mem and work here
                    result.Add(model);
                    //if (ParseLine(bufferSpan.Slice(0, newLineStart), ref curMeasurement, ref curMeasurement2))
                    //{
                    //    doubles.Add(curMeasurement);
                    //    ints.Add(curMeasurement2);
                    //}
                    bufferSpan = bufferSpan.Slice(newLineStart + newLineSpan.Length);
                }

                bufferSpan.CopyTo(buffer.AsSpan());
                lastLineSize = bufferSpan.Length;
            }
            return result;
        }
    }
}
