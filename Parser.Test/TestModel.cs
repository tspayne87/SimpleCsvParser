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
}