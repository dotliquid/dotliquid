using System;
using System.Globalization;
using System.IO;
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

            public static string AddSmiley(string input, string smiley = ":-)")
            {
                return input + " " + smiley;
            }

            public static string AddTag(string input, string tag = "p", string id = "foo")
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

        [OneTimeSetUp]
        public void SetUp()
        {
            _assigns = Hash.FromAnonymousObject(new
            {
                best_cars = "bmw",
                car = Hash.FromAnonymousObject(new { bmw = "good", gm = "bad" }),
                number = 3.145
            });
        }

        [Test]
        public void TestVariable()
        {
            Assert.AreEqual(" bmw ", Template.Parse(" {{best_cars}} ").Render(_assigns));
        }


        string Render(CultureInfo culture)
        {

            var renderParams = new RenderParameters(culture)
                               {
                                   LocalVariables = _assigns 
                               };
            return Template.Parse("{{number}}").Render(renderParams);
        }

        [Test]
 	    public void TestSeperator_Comma()
        {

            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ",";
            var c = new CultureInfo("en-US")
            {
                NumberFormat = nfi
            };
            Assert.AreEqual("3,145", Render(c) );

        }
 	    [Test]
 	    public void TestSeperator_Decimal()
        {
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";
            var c = new CultureInfo("en-US")
            {
                NumberFormat = nfi
            };
            Assert.AreEqual("3.145",Render(c) );

        }

        private class ActionDisposable : IDisposable
        {
            private readonly Action _Action;

            public ActionDisposable(Action action) => _Action = action;

            public void Dispose() => _Action();
        }
        IDisposable SetCulture(CultureInfo ci)
        {
            var old = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = ci;
            return new ActionDisposable( ()=>CultureInfo.CurrentCulture = old );
        }


        [Test]
        public void ParsingWithCommaDecimalSeparatorShouldWorkWhenPassedCultureIsDifferentToCurrentCulture()
        {
            var ci = new CultureInfo(CultureInfo.CurrentCulture.Name)
            {
                NumberFormat =
                      {
                          NumberDecimalSeparator = ","
                          , NumberGroupSeparator = "."
                      }
            };
            using (SetCulture( ci ))
            {
                var t = Template.Parse( "{{2.5}}" );
                var result = t.Render( new Hash(), CultureInfo.InvariantCulture );

                Assert.AreEqual( result, "2.5" );
            }
        }

        [Test]
        public void ParsingWithInvariantCultureShouldWork()
        {
            var ci = new CultureInfo(CultureInfo.CurrentCulture.Name)
                              {
                                  NumberFormat =
                                  {
                                      NumberDecimalSeparator = ","
                                      , NumberGroupSeparator = "."
                                  }
                              };
            using (SetCulture( ci ))
            {
                float.TryParse("2.5", NumberStyles.Number, CultureInfo.InvariantCulture, out var result);

                Assert.AreEqual(2.5,result);
            }
        }

        [Test]
        public void ParsingWithExplicitCultureShouldWork()
        {
            var ci = new CultureInfo( CultureInfo.CurrentCulture.Name )
                                 {
                                     NumberFormat =
                                     {
                                         NumberDecimalSeparator = ","
                                         , NumberGroupSeparator = "."
                                     }
                                 };
            using (SetCulture( ci ))
            {
                CultureInfo.CurrentCulture = ci;
                float.TryParse("2.5", NumberStyles.Number, ci, out var result);

                Assert.AreEqual(25, result);
            }
        }


        [Test]
        public void ParsingWithDefaultCultureShouldWork()
        {
            var ci = new CultureInfo(CultureInfo.CurrentCulture.Name)
                              {
                                  NumberFormat =
                                  {
                                      NumberDecimalSeparator = ","
                                      , NumberGroupSeparator = "."
                                  }
                              };
            using(SetCulture( ci))
            {
                float.TryParse( "2.5", out var result);
                Assert.AreEqual(25,result);
            }
        }

        [Test]
        public void TestVariableTraversing()
        {
            Assert.AreEqual(" good bad good ", Template.Parse(" {{car.bmw}} {{car.gm}} {{car.bmw}} ").Render(_assigns));
        }

        [Test]
        public void TestVariablePiping()
        {
            Assert.AreEqual(" LOL ", Template.Parse(" {{ car.gm | make_funny }} ").Render(new RenderParameters(CultureInfo.InvariantCulture) { LocalVariables = _assigns, Filters = new[] { typeof(FunnyFilter) } }));
        }

        [Test]
        public void TestVariablePipingWithInput()
        {
            Assert.AreEqual(" LOL: bad ", Template.Parse(" {{ car.gm | cite_funny }} ").Render(new RenderParameters(CultureInfo.InvariantCulture) { LocalVariables = _assigns, Filters = new[] { typeof(FunnyFilter) } }));
        }

        [Test]
        public void TestVariablePipingWithArgs()
        {
            Assert.AreEqual(" bad :-( ", Template.Parse(" {{ car.gm | add_smiley : ':-(' }} ").Render(new RenderParameters(CultureInfo.InvariantCulture) { LocalVariables = _assigns, Filters = new[] { typeof(FunnyFilter) } }));
        }

        [Test]
        public void TestVariablePipingWithNoArgs()
        {
            Assert.AreEqual(" bad :-) ", Template.Parse(" {{ car.gm | add_smiley }} ").Render(new RenderParameters(CultureInfo.InvariantCulture) { LocalVariables = _assigns, Filters = new[] { typeof(FunnyFilter) } }));
        }

        [Test]
        public void TestMultipleVariablePipingWithArgs()
        {
            Assert.AreEqual(" bad :-( :-( ", Template.Parse(" {{ car.gm | add_smiley : ':-(' | add_smiley : ':-(' }} ").Render(new RenderParameters(CultureInfo.InvariantCulture) { LocalVariables = _assigns, Filters = new[] { typeof(FunnyFilter) } }));
        }

        [Test]
        public void TestVariablePipingWithArgs2()
        {
            Assert.AreEqual(" <span id=\"bar\">bad</span> ", Template.Parse(" {{ car.gm | add_tag : 'span', 'bar' }} ").Render(new RenderParameters(CultureInfo.InvariantCulture) { LocalVariables = _assigns, Filters = new[] { typeof(FunnyFilter) } }));
        }

        [Test]
        public void TestVariablePipingWithWithVariableArgs()
        {
            Assert.AreEqual(" <span id=\"good\">bad</span> ", Template.Parse(" {{ car.gm | add_tag : 'span', car.bmw }} ").Render(new RenderParameters(CultureInfo.InvariantCulture) { LocalVariables = _assigns, Filters = new[] { typeof(FunnyFilter) } }));
        }

        [Test]
        public void TestMultiplePipings()
        {
            Assert.AreEqual(" <p>LOL: bmw</p> ", Template.Parse(" {{ best_cars | cite_funny | paragraph }} ").Render(new RenderParameters(CultureInfo.InvariantCulture) { LocalVariables = _assigns, Filters = new[] { typeof(FunnyFilter) } }));
        }

        [Test]
        public void TestLinkTo()
        {
            Assert.AreEqual(" <a href=\"http://typo.leetsoft.com\">Typo</a> ", Template.Parse(" {{ 'Typo' | link_to: 'http://typo.leetsoft.com' }} ").Render(new RenderParameters(CultureInfo.InvariantCulture) { LocalVariables = _assigns, Filters = new[] { typeof(FunnyFilter) } }));
        }
    }
}
