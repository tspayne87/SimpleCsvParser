namespace SimpleCsvParser.Options
{
   /// <summary>
   /// Options for the csv stream reader that will load a basic csv file
   /// </summary>
   public class CsvStreamReaderWithHeaderOptions : CsvStreamReaderOptions
   {
      /// <summary>
      /// The delimiter that is used to seperate data values.
      /// </summary>
      public string HeaderDelimiter { get; set; } = ",";

      /// <summary>
      /// The data wrapper that is used when the delimiter is part of the data value.
      /// </summary>
      public char? HeaderWrapper { get; set; } = '"';

      /// <summary>
      /// The row delimiter that should be used when breaking up the rows.
      /// </summary>
      /// <value></value>
      public string HeaderRowDelimiter { get; set; } = "\r\n";

      /// <summary>
      /// If empty entries need to be removed from the list.
      /// </summary>
      public bool HeaderRemoveEmptyEntries { get; set; } = false;

      /// <summary>
      /// The row index where the data start in the file.
      /// </summary>
      public int HeaderStartRow { get; set; } = 0;

      /// <summary>
      /// If we need to parse the headers.
      /// </summary>
      public bool IgnoreHeaders { get; set; } = false;
   }
}