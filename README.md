# Simple Csv Parser
A Simple CSV Parser that is meant to convert csv files into lists of objects and create csv files.

## Features
- Delimited file parsing
- Conversion into usable C# objects
- Binding to headers
- Large file processing

## Installation
#### Package Manager
```bash
    Install-Package SimpleCsvParser.Core
```
#### .NET CLI
```bash
    dotnet add package SimpleCsvParser.Core
```
#### Paket CLI
```bash
    paket add SimpleCsvParser.Core
```

## Options
### Delimiter *(char) = ','
The delimiter the parser should use for each row in the file.
### RowDelimiter *(string) = "\r\n"
The row delimiter that is used to parse the different rows.
### Wrapper *(char) = '"'
The wrapper tag that is used if the delimiter exists in the data of the column.
### AllowDefaults *(bool) = true
Determine if the parser should allow defaults when creating the object.
### ParseHeaders *(bool) = true
Determine if the parser should handle the first line in the delimited stream as a header.
### RemoveEmptyEntries *(bool) = false
Determine if the parser should remove empty entries from the list before using them.
### WriteHeaders *(bool) = true
Determine if the parser needs to write the headers to the file as well.

## Examples
### Basic Usage
This is shorthand to not worry about creating the stream reader.
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
    var list = CsvParser.ParseFile<TestModel>("test.csv", new CsvStreamOptions() { RemoveEmptyEntries = true });
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

### Stream Usage
Basic Usage for stream objects
```csharp
    using (var reader = new CsvStreamReader<TestModel>(stream))
    {
        foreach (var item in reader.AsEnumerable())
        {
            ... Do stuff with item
        }
    }
```

### Large Data Files
Large data files need to be processed differently because some of them will not be able to be held in memory very easily.  Be careful with large data sets because they could cause major issues with the program by slowing it down to a crawl or eating up all the memory in the system.
```csharp
    using (var reader = new CsvStreamReader<LargeModel>("large.csv", new CsvStreamOptions() { ParseHeaders = false }))
    {
        foreach (var item in reader.AsEnumerable())
        {
            ... Do stuff with item
        }

        Parallel.ForEach(reader.AsEnumerable(), (x, state, index) => {
            ... Process the iterator in different threads
        });
    }
```
### Writing Files
```csharp
    var list = new List<TestModel>();
    ... fill up the list that we want to save to a file.

    CsvParser.SaveFile("example.csv", list);
```

### Change Log
All notable changes to this project will be added here until it gets to long and a file is needed.

#### [1.1.2] - 2018-12-28
- Reworked the data stream to build out the rows and columns.
- Added in new option (RowDelimiter) which will allow for different row types.
- - This is a change that could break the previous version because before it was checking for '\r', '\n' and '\r\n' all at once.  And now is only checking for '\r\n' or whatever is in the RowDelimiter option.
- Adding in the CsvStreamWriter this will allow for the writing of objects to a file or stream.
- Various speed impovements.
- Added in new option (WriteHeaders) which will handle if the parser needs to print the headers to the file or stream.

### License
SimpleCsvParser.Core is licensed under the [Microsoft Public License](https://opensource.org/licenses/MS-PL)