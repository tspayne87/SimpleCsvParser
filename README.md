# Simple Csv Parser
A Simple CSV Parser that is meant to convert csv files into lists of objects.

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
### Delimiter *(string) = "\r\n"
The The delimiter that is used to seperate data values.
### Wrapper *(char) = '"'
The wrapper tag that is used if the delimiter exists in the data of the column.
### RowDelimiter *(string) = "\r\n"
The row delimiter that is used to parse the different rows.
### RemoveEmptyEntries *(bool) = false
Determine if the parser should remove empty entries from the list before using them.
### StartRow *(int) = 0
The starting row the data should start in

### HeaderDelimiter *(string) = "\r\n"
The The delimiter that is used to seperate data values.
### HeaderWrapper *(char) = '"'
The wrapper tag that is used if the delimiter exists in the data of the column.
### HeaderRowDelimiter *(string) = "\r\n"
The row delimiter that is used to parse the different rows.
### HeaderRemoveEmptyEntries *(bool) = false
Determine if the parser should remove empty entries from the list before using them.
### HeaderStartRow *(int) = 0
The starting row the data should start in

### IgnoreHeaders *(bool) = false
Determine if the parser should handle the first line in the delimited stream as a header.


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
    var list = CsvParser.ParseFile<TestModel>("test.csv", new CsvStreamReaderWithHeaderOptions() { RemoveEmptyEntries = true, StartRow = 1 });
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
    using var reader = new CsvStreamModelReader<TestModel>(stream);
    reader.LoadHeaders();
    foreach (var item in reader.Parse())
    {
        ... Do stuff with item
    }
```

### Streams
- CsvStreamModelReader<TModel>
  - A standard reader that will mainly used by this framework to build out objects from the delimited data.
- CsvStreamReader
  - A basic stream reader that parses a 2D list of items

### Change Log
All notable changes to this project will be added here until it gets to long and a file is needed.


#### [2.0.1] - 2022-05-25
- Minor speed increases
- All for null or empty delimiters

#### [2.0.0] - 2022-05-25
- A major overhaul has been done, thus the major version release.
  - Speed improvements have been included into this version
  - Less memory usage by the main processor
  - Writing is no long support but may revist it later once the speed has been improved more

#### [1.2.4] - 2020-03-30
- Bug Fix: Fixing an issue when trying to use CsvDictionaryStreamReader when ParseHeaders is false, now will create a dictionary based on the empty lamda function passed into the AsEnumerable for this stream.

#### [1.2.3] - 2019-08-20
- Bug Fix: Changed ToDictionary Stream to deal with seperating items better.

#### [1.2.2] - 2019-08-13
- Change Allow for null to be passed to the Wrapper option.

#### [1.2.1] - 2019-08-08
- Moved Streams into the 'SimpleCsvParser.Streams' namespace.
- Added: CsvRowStreamReader to allow for custom parsers.

#### [1.2.0] - 2019-08-06
- The main purpose of these changes is to allow for parsing on files where the header names are unknown until it is parsed, and Dictionary is how it is done.
- Reworked internals so that dictionaries could be used.
- Added new options: All delimiters got changed from char to string.
  - HeaderRowDelimiter: The header row delimiter should be used.
  - HeaderDelimiter: The header delimiter that should be used.
  - HeaderRow: The index where the header should start when the row delimiter is applied.
  - DataRow: The index where the data should start when the row delimiter is applied.
- Added CsvDictionaryStreamReader to deal with reading dictionary results that do not have all their headers defined.
- Malformed Exception is not thrown or row parsing now due to the data could have comments and the DataRow/HeaderRow gives more control to skip these rows.

#### [1.1.3] - 2018-12-31
- Reworked data streams to allow for better parallelization and enumeration.
- Added AsParallel to the reader to handle large file processing.
- Various speed improvements.

#### [1.1.2] - 2018-12-28
- Reworked the data stream to build out the rows and columns.
- Added in new option (RowDelimiter) which will allow for different row types.
  - This is a change that could break the previous version because before it was checking for '\r', '\n' and '\r\n' all at once.  And now is only checking for '\r\n' or whatever is in the RowDelimiter option.
- Adding in the CsvStreamWriter this will allow for the writing of objects to a file or stream.
- Various speed impovements.
- Added in new option (WriteHeaders) which will handle if the parser needs to print the headers to the file or stream.

### License
SimpleCsvParser.Core is licensed under the [Microsoft Public License](https://opensource.org/licenses/MS-PL)