# Simple Csv Parser
A csv parser that is meant to convert csv files into lists of objects.

## Usage

```csharp
    public class TestModel
    {
        [CsvProperty("name")]
        public string Name { get; set; }
        [CsvProperty("type")]
        public TestType Type { get; set; }
        [CsvProperty("cost")]
        public int Cost { get; set; }
        [CsvProperty("id")]
        public int Id { get; set; }
    }

    public enum TestType
    {
        Attachment,
        Effect,
        Spell,
        Structure
    }

    ...
    var parser = new CsvParser(new CsvParserOptions() { RemoveEmptyEntries = true });
    var list = parser.ParseFile<TestModel>("test.csv");
    // List<TestModel>
    //      { Name = "Claws", Type = TestType.Attachment, Cost = 10, Id = 1 }
    //      { Name = "Double Edge Sword", Type = TestType.Attachment, Cost = 10, Id = 2 }
    //      { Name = "Gauntlet", Type = TestType.Attachment, Cost = 10, Id = 3 }
    //      { Name = "Leather Armour", Type = TestType.Attachment, Cost = 15, Id = 4 }
    //      ...
    ...
```

test.csv
```csv
name,type,cost,id,final
Claws,Attachment,10,"1",
Double Edge Sword,Attachment,10,2,
Gauntlet,Attachment,10,3,"Final, Test"
Leather Armour,Attachment,15,4,
Wings,Attachment,15,5,
Control,Effect,10,6,
,,,,
Ensnare,Effect,15,7,
Healing Spring,Effect,15,8,
Katee's Inferno,Effect,20,9,
Tornado,Effect,25,10,
Destroy Attachment,Spell,10,11,
Fireball,Spell,5,12,
,,,,
Heal,Spell,5,13,
Lightning Bolt,Spell,10,14,
Teleport,Spell,10,15,
Armory,Structure,15,16,
Blacksmith,Structure,15,17,
"""Magma"" Fissure",Structure,15,18,
"Medical,Healing Camp",Structure,15,19,
Spire,Structure,5,20,
,,,,
,,,,
```