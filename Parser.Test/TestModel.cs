using System;

namespace Parser.Test
{
    public class TestModel
    {
        [CsvProperty("name")]
        public string Name { get; set; }
        [CsvProperty("type")]
        public TestType Type { get; set; }
        [CsvProperty("cost")]
        public int Cost { get; set; }
        [CsvProperty("id")]
        public Double Id { get; set; }
        [CsvProperty("date")]
        public DateTime Date { get; set; }
    }

    public class SecondTestModel
    {
        [CsvProperty(0)]
        public string Name { get; set; }
        [CsvProperty(1)]
        public TestType Type { get; set; }
        [CsvProperty(2)]
        public int Cost { get; set; }
        [CsvProperty(3)]
        public double Id { get; set; }
        [CsvProperty(4)]
        public DateTime Date { get; set; }
    }

    public enum TestType
    {
        Attachment,
        Effect,
        Spell,
        Structure
    }

    public class ImportCSVContainer
   {
       [CsvProperty("container_id")]
       public string ContainerId { get; set; }

       [CsvProperty("site")]
       public string Site { get; set; }

       [CsvProperty("subject")]
       public string Subject { get; set; }

       [CsvProperty("when_collected")]
       public string WhenCollected { get; set; }

       [CsvProperty("when_shipped")]
       public string WhenShipped { get; set; }

       [CsvProperty("storage_container_id")]
       public string StorageContainerId { get; set; }

       [CsvProperty("row")]
       public string Row { get; set; }

       [CsvProperty("column")]
       public string Column { get; set; }

       [CsvProperty("sample_type")]
       public string SampleType { get; set; }

       [CsvProperty("volume")]
       public string Volume { get; set; }

       [CsvProperty("volume_units")]
       public string VolumeUnits { get; set; }

       [CsvProperty("concentration")]
       public string Concentration { get; set; }

       [CsvProperty("specific_concentration")]
       public string SpecificConcentration { get; set; }

       [CsvProperty("concentration_type")]
       public string ConcentrationType { get; set; }

       [CsvProperty("parent_container_id")]
       public string ParentContainerId { get; set; }

       [CsvProperty("when_extracted")]
       public string WhenExtracted { get; set; }
   }
}