namespace SimpleCsvParser.Options
{
   /// <summary>
   /// Options for the csv stream reader that will load a basic csv file
   /// </summary>
   public class CsvStreamReaderOptions
   {
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
      /// If empty entries need to be removed from the list.
      /// </summary>
      public bool RemoveEmptyEntries { get; set; } = false;

      /// <summary>
      /// The row index where the data start in the file.
      /// </summary>
      /// <value></value>
      public int StartRow { get; set; } = 0;
   }
}