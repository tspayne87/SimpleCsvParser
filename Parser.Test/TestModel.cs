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

    public enum TestType
    {
        Attachment,
        Effect,
        Spell,
        Structure
    }
}