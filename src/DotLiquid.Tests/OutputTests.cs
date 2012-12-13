using NUnit.Framework;

namespace DotLiquid.Tests
{
	[TestFixture]
	public class OutputTests
	{
		private static class FunnyFilter
		{
			public static string MakeFunny(string input)
			{
				return "LOL";
			}

			public static string CiteFunny(string input)
			{
				return "LOL: " + input;
			}

#if NET35
            public static string AddSmiley(string input)
            {
                return AddSmiley(input, ":-)");
            }

            public static string AddSmiley(string input, string smiley)
#else
			public static string AddSmiley(string input, string smiley = ":-)")
#endif
			{
				return input + " " + smiley;
			}

#if NET35
            public static string AddTag(string input)
            {
                return AddTag(input, "p", "foo");
            }

            public static string AddTag(string input, string tag, string id)
#else
			public static string AddTag(string input, string tag = "p", string id = "foo")
#endif
			{
				return string.Format("<{0} id=\"{1}\">{2}</{0}>", tag, id, input);
			}

			public static string Paragraph(string input)
			{
				return string.Format("<p>{0}</p>", input);
			}

			public static string LinkTo(string name, string url)
			{
				return string.Format("<a href=\"{0}\">{1}</a>", url, name);
			}
		}

		private Hash _assigns;

		[TestFixtureSetUp]
		public void SetUp()
		{
			_assigns = Hash.FromAnonymousObject(new
			{
				best_cars = "bmw",
				car = Hash.FromAnonymousObject(new { bmw = "good", gm = "bad" })
			});
		}

		[Test]
		public void TestVariable()
		{
			Assert.AreEqual(" bmw ", Template.Parse(" {{best_cars}} ").Render(_assigns));
		}

		[Test]
		public void TestVariableTraversing()
		{
			Assert.AreEqual(" good bad good ", Template.Parse(" {{car.bmw}} {{car.gm}} {{car.bmw}} ").Render(_assigns));
		}

		[Test]
		public void TestVariablePiping()
		{
			Assert.AreEqual(" LOL ", Template.Parse(" {{ car.gm | make_funny }} ").Render(new RenderParameters { LocalVariables = _assigns, Filters = new[] { typeof(FunnyFilter) } }));
		}

		[Test]
		public void TestVariablePipingWithInput()
		{
			Assert.AreEqual(" LOL: bad ", Template.Parse(" {{ car.gm | cite_funny }} ").Render(new RenderParameters { LocalVariables = _assigns, Filters = new[] { typeof(FunnyFilter) } }));
		}

		[Test]
		public void TestVariablePipingWithArgs()
		{
			Assert.AreEqual(" bad :-( ", Template.Parse(" {{ car.gm | add_smiley : ':-(' }} ").Render(new RenderParameters { LocalVariables = _assigns, Filters = new[] { typeof(FunnyFilter) } }));
		}

		[Test]
		public void TestVariablePipingWithNoArgs()
		{
			Assert.AreEqual(" bad :-) ", Template.Parse(" {{ car.gm | add_smiley }} ").Render(new RenderParameters { LocalVariables = _assigns, Filters = new[] { typeof(FunnyFilter) } }));
		}

		[Test]
		public void TestMultipleVariablePipingWithArgs()
		{
			Assert.AreEqual(" bad :-( :-( ", Template.Parse(" {{ car.gm | add_smiley : ':-(' | add_smiley : ':-(' }} ").Render(new RenderParameters { LocalVariables = _assigns, Filters = new[] { typeof(FunnyFilter) } }));
		}

		[Test]
		public void TestVariablePipingWithArgs2()
		{
			Assert.AreEqual(" <span id=\"bar\">bad</span> ", Template.Parse(" {{ car.gm | add_tag : 'span', 'bar' }} ").Render(new RenderParameters { LocalVariables = _assigns, Filters = new[] { typeof(FunnyFilter) } }));
		}

		[Test]
		public void TestVariablePipingWithWithVariableArgs()
		{
			Assert.AreEqual(" <span id=\"good\">bad</span> ", Template.Parse(" {{ car.gm | add_tag : 'span', car.bmw }} ").Render(new RenderParameters { LocalVariables = _assigns, Filters = new[] { typeof(FunnyFilter) } }));
		}

		[Test]
		public void TestMultiplePipings()
		{
			Assert.AreEqual(" <p>LOL: bmw</p> ", Template.Parse(" {{ best_cars | cite_funny | paragraph }} ").Render(new RenderParameters { LocalVariables = _assigns, Filters = new[] { typeof(FunnyFilter) } }));
		}

		[Test]
		public void TestLinkTo()
		{
			Assert.AreEqual(" <a href=\"http://typo.leetsoft.com\">Typo</a> ", Template.Parse(" {{ 'Typo' | link_to: 'http://typo.leetsoft.com' }} ").Render(new RenderParameters { LocalVariables = _assigns, Filters = new[] { typeof(FunnyFilter) } }));
		}
	}
}