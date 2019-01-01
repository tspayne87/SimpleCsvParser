using System;
using System.Diagnostics;

namespace SimpleCsvParser.Performance
{
    class Program
    {
        private static string file = "large.csv";

        static void Main(string[] args)
        {
            // WriteSmallModel();
            // WriteLargeModel(1000000);
            ReadFile<LargeModel>();
            // ReadFile<TestModel>();
        }

        static void ReadFile<TModel>()
            where TModel: class, new()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            using (var reader = new CsvStreamReader<TModel>(file))
            {
                foreach (var model in reader.AsParallel()) { }
            }
            stopwatch.Stop();
            Console.WriteLine($"Total Elapsed Time: {stopwatch.Elapsed}");
        }

        static void WriteLargeModel(int iterations = 100)
        {
            long min = long.MaxValue;
            long max = 0;
            long total = 0;
            using (var writer = new CsvStreamWriter<LargeModel>(file))
            {
                var stopwatch = new Stopwatch();
                for (var i = 0; i < iterations; ++i) {
                    Console.WriteLine("Parsing line: " + i);
                    stopwatch.Restart();
                    writer.WriteLine(GenerateModel());
                    stopwatch.Stop();

                    var elapsed = stopwatch.ElapsedMilliseconds;
                    min = Math.Min(elapsed, min);
                    max = Math.Max(max, elapsed);
                    total += elapsed;
                }
            }
            long average = total / iterations;
            Console.WriteLine($"Total Elapsed Time: {total}, Min: {min}, Max: {max}, Average: {average}");
        }

        static void WriteSmallModel(int iterations = 1000000)
        {
            long min = long.MaxValue;
            long max = 0;
            long total = 0;
            using (var writer = new CsvStreamWriter<TestModel>(file))
            {
                var stopwatch = new Stopwatch();
                for (var i = 0; i < iterations; ++i) {
                    Console.WriteLine("Parsing line: " + i);
                    stopwatch.Restart();
                    writer.WriteLine(GenerateSmallModel());
                    stopwatch.Stop();

                    var elapsed = stopwatch.ElapsedMilliseconds;
                    min = Math.Min(elapsed, min);
                    max = Math.Max(max, elapsed);
                    total += elapsed;
                }
            }
            long average = total / iterations;
            Console.WriteLine($"Total Elapsed Time: {total}, Min: {min}, Max: {max}, Average: {average}");
        }

        private static TestModel GenerateSmallModel()
        {
            return new TestModel() {
                Name = $"Test",
                Type = TestType.Attachment,
                Cost = 10 ,
                Id = 1,
                Date = DateTime.Now
            };
        }

        private static LargeModel GenerateModel()
        {
            return new LargeModel() {
                Row1 = "Row1",
                Row2 = "Row2",
                Row3 = "Row3",
                Row4 = "Row4",
                Row5 = "Row5",
                Row6 = "Row6",
                Row7 = "Row7",
                Row8 = "Row8",
                Row9 = "Row9",
                Row10 = "Row10",
                Row11 = "Row11",
                Row12 = "Row12",
                Row13 = "Row13",
                Row14 = "Row14",
                Row15 = "Row15",
                Row16 = "Row16",
                Row17 = "Row17",
                Row18 = "Row18",
                Row19 = "Row19",
                Row20 = "Row20",
                Row21 = "Row21",
                Row22 = "Row22",
                Row23 = "Row23",
                Row24 = "Row24",
                Row25 = "Row25",
                Row26 = "Row26",
                Row27 = "Row27",
                Row28 = "Row28",
                Row29 = "Row29",
                Row30 = "Row30",
                Row31 = "Row31",
                Row32 = "Row32",
                Row33 = "Row33",
                Row34 = "Row34",
                Row35 = "Row35",
                Row36 = "Row36",
                Row37 = "Row37",
                Row38 = "Row38",
                Row39 = "Row39",
                Row40 = "Row40",
                Row41 = "Row41",
                Row42 = "Row42",
                Row43 = "Row43",
                Row44 = "Row44",
                Row45 = "Row45",
                Row46 = "Row46",
                Row47 = "Row47",
                Row48 = "Row48",
                Row49 = "Row49",
                Row50 = "Row50",
                Row51 = "Row51",
                Row52 = "Row52",
                Row53 = "Row53",
                Row54 = "Row54",
                Row55 = "Row55",
                Row56 = "Row56",
                Row57 = "Row57",
                Row58 = "Row58",
                Row59 = "Row59",
                Row60 = "Row60",
                Row61 = "Row61",
                Row62 = "Row62",
                Row63 = "Row63",
                Row64 = "Row64",
                Row65 = "Row65",
                Row66 = "Row66",
                Row67 = "Row67",
                Row68 = "Row68",
                Row69 = "Row69",
                Row70 = "Row70",
                Row71 = "Row71",
                Row72 = "Row72",
                Row73 = "Row73",
                Row74 = "Row74",
                Row75 = "Row75",
                Row76 = "Row76",
                Row77 = "Row77",
                Row78 = "Row78",
                Row79 = "Row79",
                Row80 = "Row80",
                Row81 = "Row81",
                Row82 = "Row82",
                Row83 = "Row83",
                Row84 = "Row84",
                Row85 = "Row85",
                Row86 = "Row86",
                Row87 = "Row87",
                Row88 = "Row88",
                Row89 = "Row89",
                Row90 = "Row90",
                Row91 = "Row91",
                Row92 = "Row92",
                Row93 = "Row93",
                Row94 = "Row94",
                Row95 = "Row95",
                Row96 = "Row96",
                Row97 = "Row97",
                Row98 = "Row98",
                Row99 = "Row99",
                Row100 = "Row100"
            };
        }
    }
}
