using CsvHelper.Configuration.Attributes;
using SimpleCsvParser;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Performance
{
    public class DataModel
    {
        [CsvProperty(0)]
        [Index(0)]
        public Guid Column1 { get; set; }
        [CsvProperty(1)]
        [Index(1)]
        public DateTime Column2 { get; set; }
        [CsvProperty(2)]
        [Index(2)]
        public string Column3 { get; set; }
        [CsvProperty(3)]
        [Index(3)]
        public string Column4 { get; set; }
        [CsvProperty(4)]
        [Index(4)]
        public DateTime Column5 { get; set; }
        [CsvProperty(5)]
        [Index(5)]
        public string Column6 { get; set; }
        [CsvProperty(6)]
        [Index(6)]
        public string Column7 { get; set; }
        [CsvProperty(7)]
        [Index(7)]
        public string Column8 { get; set; }
        [CsvProperty(8)]
        [Index(8)]
        public string Column9 { get; set; }
        [CsvProperty(9)]
        [Index(9)]
        public string Column10 { get; set; }
        [CsvProperty(10)]
        [Index(10)]
        public string Column11 { get; set; }
        [CsvProperty(11)]
        [Index(11)]
        public string Column12 { get; set; }
        [CsvProperty(12)]
        [Index(12)]
        public string Column13 { get; set; }
        [CsvProperty(13)]
        [Index(13)]
        public string Column14 { get; set; }
        [CsvProperty(14)]
        [Index(14)]
        public string Column15 { get; set; }
        [CsvProperty(15)]
        [Index(15)]
        public string Column16 { get; set; }
        [CsvProperty(16)]
        [Index(16)]
        public string Column17 { get; set; }
        [CsvProperty(17)]
        [Index(17)]
        public string Column18 { get; set; }
        [CsvProperty(18)]
        [Index(18)]
        public string Column19 { get; set; }
        [CsvProperty(19)]
        [Index(19)]
        public string Column20 { get; set; }
        [CsvProperty(20)]
        [Index(20)]
        public string Column21 { get; set; }
        [CsvProperty(21)]
        [Index(21)]
        public string Column22 { get; set; }
        [CsvProperty(22)]
        [Index(22)]
        public string Column23 { get; set; }
        [CsvProperty(23)]
        [Index(23)]
        public string Column24 { get; set; }
        [CsvProperty(24)]
        [Index(24)]
        public string Column25 { get; set; }
    }
}
