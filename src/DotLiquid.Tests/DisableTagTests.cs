using System.Collections.Generic;
using System.Globalization;
using System.IO;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    [TestFixture]
    public class DisableTagTests
    {
        internal class CustomTag : Tag
        {
            public override void Render(Context context, TextWriter result) => result.Write(TagName);
        }
        internal class Custom2Tag : Tag
        {
            public override void Render(Context context, TextWriter result) => result.Write(TagName);
        }

        internal class NoNameTag : Tag
        {
            public override void Initialize(string tagName, string markup, List<string> tokens)
            {
                // ignore base, so tag name is not be set
            }

            public override void Render(Context context, TextWriter result) => result.Write($"from no name: {TagName}");
        }

        [Test]
        public void TestDisableTag()
        {
            Helper.WithCustomTag<CustomTag>("custom", () =>
            {
                var context = new Context(CultureInfo.InvariantCulture);
                context.WithDisabledTags(new[] { "custom" }, () =>
                {
                    Assert.That(Template.Parse("{% custom %}").Render(RenderParameters.FromContext(context, context.FormatProvider)),
                        Is.EqualTo("Liquid error: custom usage is not allowed in this context"));
                });
            });
        }

        [Test]
        public void TestDisableNestedTag()
        {
            Helper.WithCustomTag<CustomTag>("custom", () =>
            {
                Helper.WithCustomTag<Custom2Tag>("custom2", () =>
                {
                    var context = new Context(CultureInfo.InvariantCulture);
                    context.WithDisabledTags(new[] { "custom" }, () =>
                    {
                        Assert.That(Template.Parse("{% custom %};{% custom2 %}").Render(RenderParameters.FromContext(context, context.FormatProvider)),
                            Is.EqualTo("Liquid error: custom usage is not allowed in this context;custom2"));
                    });
                });
            });
        }

        [Test]
        public void TestDisableMultipleNestedTag()
        {
            Helper.WithCustomTag<CustomTag>("custom", () =>
            {
                Helper.WithCustomTag<Custom2Tag>("custom2", () =>
                {
                    var context = new Context(CultureInfo.InvariantCulture);
                    context.WithDisabledTags(new[] { "custom", "custom2" }, () =>
                    {
                        Assert.That(Template.Parse("{% custom %};{% custom2 %}").Render(RenderParameters.FromContext(context, context.FormatProvider)),
                            Is.EqualTo("Liquid error: custom usage is not allowed in this context;Liquid error: custom2 usage is not allowed in this context"));
                    });
                });
            });
        }

        [Test]
        public void TestIgnoreTagWithoutTagName()
        {
            Helper.WithCustomTag<NoNameTag>("no_name", () =>
            {
                var context = new Context(CultureInfo.InvariantCulture);
                context.WithDisabledTags(new[] { "no_name" }, () =>
                {
                    Assert.That(Template.Parse("{% no_name %}").Render(RenderParameters.FromContext(context, context.FormatProvider)),
                        Is.EqualTo("from no name: "));
                });
            });
        }
    }
}
