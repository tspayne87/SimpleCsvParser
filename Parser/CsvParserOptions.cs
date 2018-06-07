namespace Parser
{
    public class CsvParserOptions
    {
        public char Delimiter { get; set; } = ',';
        public char Wrapper { get; set; } = '"';
        public bool RemoveEmptyEntries { get; set; } = false;
        public bool AllowDefaults { get; set; } = true;
        public bool ParseHeaders { get; set; } = true;
    }
}