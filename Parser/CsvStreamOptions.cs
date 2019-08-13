namespace SimpleCsvParser
{
    public class CsvStreamOptions
    {
        /// <summary>
        /// The delimiter that is used to seperate the header rows.
        /// </summary>
        /// <value></value>
        public string HeaderRowDelimiter { get; set; } = "\r\n";
        /// <summary>
        /// The delimiter that is used to seperate the columns on the header.
        /// </summary>
        /// <value></value>
        public string HeaderDelimiter { get; set; } = ",";
        /// <summary>
        /// The delimiter that is used to seperate data values.
        /// </summary>
        public string Delimiter { get; set; } = ",";
        /// <summary>
        /// The data wrapper that is used when the delimiter is part of the data value.
        /// </summary>
        public char? Wrapper { get; set; } = '"';
        /// <summary>
        /// The row delimiter that should be used when breaking up the rows.
        /// </summary>
        /// <value></value>
        public string RowDelimiter { get; set; } = "\r\n";
        /// <summary>
        /// If defaults should be used when building out the objects.
        /// </summary>
        public bool AllowDefaults { get; set; } = true;
        /// <summary>
        /// If we want to parse the headers out of the csv file.
        /// </summary>
        public bool ParseHeaders { get; set; } = true;
        /// <summary>
        /// If empty entries need to be removed from the list.
        /// </summary>
        public bool RemoveEmptyEntries { get; set; } = false;
        /// <summary>
        /// To determine if we need to write the headers to the stream, is only used in the csv parser static class.
        /// </summary>
        /// <value></value>
        public bool WriteHeaders { get; set; } = true;
        /// <summary>
        /// The row index where the header row exists in the file after being parsed.
        /// </summary>
        /// <value></value>
        public int HeaderRow { get; set; } = 0;
        /// <summary>
        /// The row index where the data start in the file.
        /// </summary>
        /// <value></value>
        public int DataRow { get; set; } = 1;
    }
}