using System;
using System.IO;
using System.Linq;

namespace DotLiquid.PerformanceTest
{
    using System.Collections.Generic;
    using System.Diagnostics;

    class Program
    {
        public class User : Drop
        {
            public string Name { get; set; }
            public IEnumerable<Item> Items { get; set; }
        }

        public class Item : Drop
        {
            public string Description { get; set; }
            public double Cost { get; set; }
        }
        
        static void Main()
        {
            // Warm up
            Console.WriteLine("Warm up");
            RunTest(100);

            // Real tests
            Console.WriteLine("Real tests");
            RunTest(2);
            RunTest(1000);
            RunTest(10000);
            
            Console.ReadKey();
        }

        static void RunTest(int iterations)
        {
            var template = Template.Parse(TestTemplates.AdvancedTemplate);

            var stopwatch = new Stopwatch();
            var timings = new List<double>();
            
            for (var i = 0; i < iterations; ++i)
            {
                var hash = Hash.FromAnonymousObject(GetFreshTestObject());

                stopwatch.Reset();
                stopwatch.Start();

                template.Render(hash);

                stopwatch.Stop();

                timings.Add(stopwatch.Elapsed.TotalMilliseconds);
            }

            Console.WriteLine(@"Iterations: {0}", iterations);
            Console.WriteLine(@"   Minimum: {0:0.00000}ms", timings.Min());
            Console.WriteLine(@"   Maximum: {0:0.00000}ms", timings.Max());
            Console.WriteLine(@"     Range: {0:0.00000}ms", timings.Max() - timings.Min());
            Console.WriteLine(@"   Average: {0:0.00000}ms", timings.Average());
            Console.WriteLine(@"   Std Dev: {0:0.00000}ms", CalculateStdDev(timings));
            Console.WriteLine();

            //WriteToFile(template.Render(Hash.FromAnonymousObject(GetFreshTestObject())));
        }

        private static void WriteToFile(string render)
        {
            using (var file = new StreamWriter(@"C:\templatetest.html"))
            {
                file.Write(render);
            }
        }

        private static double CalculateStdDev(IList<double> values)
        {
            var avg = values.Average();   
            var sum = values.Sum(d => Math.Pow(d - avg, 2));
            return Math.Sqrt((sum) / (values.Count() - 1));
        }

        static object GetFreshTestObject()
        {
            return new
            {
                user = new User
                {
                    Name = "Steve Lillis",
                    Items = new List<Item>
		            {
		                new Item { Description = "First Item", Cost = 52.2 },
		                new Item { Description = "Code comments", Cost = 112.2 },
                        new Item { Description = "Random", Cost = 552.26 },
		                new Item { Description = "Code Something", Cost = 1422.2 },
                        new Item { Description = "Something Else", Cost = 523.22 },
		                new Item { Description = "Test Data", Cost = 182.28 },
                        new Item { Description = "Getting Fancy", Cost = 552.26 },
		                new Item { Description = "Other Examples", Cost = 212.26 },
                        new Item { Description = "Placeholder", Cost = 512.72 },
		                new Item { Description = "Clever Words", Cost = 412.42 },
                        new Item { Description = "Shopping", Cost = 52.17 },
		                new Item { Description = "More Test Data", Cost = 122.42 },
		                new Item { Description = "Documentation", Cost = 52.2 },
		                new Item { Description = "Code comments", Cost = 112.2 },
                        new Item { Description = "Random", Cost = 552.26 },
		                new Item { Description = "Code Something", Cost = 1422.2 },
                        new Item { Description = "Something Else", Cost = 523.22 },
		                new Item { Description = "Test Data", Cost = 182.28 },
                        new Item { Description = "Getting Fancy", Cost = 552.26 },
		                new Item { Description = "Other Examples", Cost = 212.26 },
                        new Item { Description = "Placeholder", Cost = 512.72 },
		                new Item { Description = "Clever Words", Cost = 412.42 },
                        new Item { Description = "Shopping", Cost = 52.17 },
		                new Item { Description = "More Test Data", Cost = 122.42 },
		            }
                }
            };
        }
    }
}
