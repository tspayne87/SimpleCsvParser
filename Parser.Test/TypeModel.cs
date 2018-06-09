using System;

namespace SimpleCsvParser.Test
{
    public class TypeModel
    {
        [CsvProperty("boolean")]
        public bool Boolean { get; set; }
        [CsvProperty("char")]
        public char Char { get; set; }
        [CsvProperty("sbyte")]
        public sbyte SByte { get; set; }
        [CsvProperty("byte")]
        public byte Byte { get; set; }
        [CsvProperty("int16")]
        public short Int16 { get; set; }
        [CsvProperty("uint16")]
        public ushort UInt16 { get; set; }
        [CsvProperty("int32")]
        public int Int32 { get; set; }
        [CsvProperty("uint32")]
        public uint UInt32 { get; set; }
        [CsvProperty("int64")]
        public long Int64 { get; set; }
        [CsvProperty("uint64")]
        public ulong UInt64 { get; set; }
        [CsvProperty("single")]
        public Single Single { get; set; }
        [CsvProperty("double")]
        public double Double { get; set; }
        [CsvProperty("decimal")]
        public decimal Decimal { get; set; }
        [CsvProperty("datetime")]
        public DateTime DateTime { get; set; }
        [CsvProperty("string")]
        public string String { get; set; }
    }
}