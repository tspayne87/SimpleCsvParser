using System;

namespace SimpleCsvParser.Test
{
    public class TestModel
    {
        [CsvProperty("name", 1)]
        public string Name { get; set; }
        [CsvProperty("type", 2)]
        public TestType Type { get; set; }
        [CsvProperty("cost", 3)]
        public int? Cost { get; set; }
        [CsvProperty("id", 4)]
        public Double Id { get; set; }
        [CsvProperty("date", 5)]
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