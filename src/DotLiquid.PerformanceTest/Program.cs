using System;
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

        private const string TemplateCode = @"
<div>
<p><b>
{% if user.name == 'Steve Lillis' -%}
  Welcome back
{% else -%}
  I don't know you!
{% endif -%}
</b></p>
{% unless user.name == 'Steve Thompson' -%}
  <i>Unless example</i>
{% endunless -%}
{% comment %}A comment for comments sake{% endcomment %}
<ul>
<li>This entry and something about baked goods</li>
<li>
{% assign handle = 'cake' -%}
{% case handle -%}
  {% when 'cake' -%}
     This is a cake
  {% when 'cookie' -%}
     This is a cookie
  {% else -%}
     This is not a cake nor a cookie
{% endcase -%}
</li>
</ul>
</div>
<p>{{ user.name | upcase }} has the following items:</p>
<table>
{% for item in user.items -%}
  <tr>
     <td>
        {% cycle 'one', 'two', 'three' %}
     </td>
     <td>
        {{ item.description }} 
        {% assign handle = 'cake' -%}
        {% case handle -%}
          {% when 'cake' -%}
             This is a cake
          {% when 'cookie' -%}
             This is a cookie
          {% else -%}
             This is not a cake nor a cookie
        {% endcase -%}
     </td>
     <td>
        {{ item.cost }}
     </td>
  </tr>
{% endfor -%}
{% for item in user.items reversed -%}
  <tr>
     <td>e
        {% cycle 'one', 'two', 'three' %}
     </td>
     <td>
        {% if item.description == 'First Item' -%}
            {{ item.description | upcase }}
        {% else %}
            {{ item.description }}
        {% endif %}
     </td>
     <td>
        {{ item.cost }}
     </td>
  </tr>
{% endfor -%}
</table>";

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
    var template = Template.Parse(TemplateCode);

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
            
    //Console.WriteLine(template.Render(Hash.FromAnonymousObject(GetFreshTestObject())));
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
