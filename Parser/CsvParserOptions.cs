namespace Parser
{
    public class CsvParserOptions
    {
        /// <summary>
        /// The delimiter that is used to separte data values.
        /// </summary>
        public char Delimiter { get; set; } = ',';
        /// <summary>
        /// The data wrapper that is used when the delimiter is part of the data value.
        /// </summary>
        public char Wrapper { get; set; } = '"';
        /// <summary>
        /// If empty entries need to be removed from the list.
        /// </summary>
        public bool RemoveEmptyEntries { get; set; } = false;
        /// <summary>
        /// If defaults should be used when building out the objects.
        /// </summary>
        public bool AllowDefaults { get; set; } = true;
        /// <summary>
        /// If we want to parse the headers out of the csv file.
        /// </summary>
        public bool ParseHeaders { get; set; } = true;
    }
}