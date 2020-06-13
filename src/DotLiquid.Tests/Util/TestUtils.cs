using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace DotLiquid.Tests.Util
{
    public class TestUtils
    {
        public static void AssertRender(object data, string template, string expected)
        {
            var renderParameters = new RenderParameters(CultureInfo.InvariantCulture) {
                LocalVariables = ToHash(data)
            };

            var actual = Template.Parse(template).Render(renderParameters);

            Assert.That(actual, new TemplateEqualConstraint(expected));
        }

        private static Hash ToHash(object o) => (Hash) Convert(o);

        private static object Convert(object any)
        {
            if (any.GetType().IsArray)
            {
                var ary = (IEnumerable) any;
                return ary.Cast<object>().Select(Convert).ToArray();
            }

            if (any.GetType().Name.Contains("AnonymousType"))
            {
                var hash = new Hash();
                foreach (PropertyDescriptor p in TypeDescriptor.GetProperties(any))
                {
                    hash.Add(p.Name, Convert(p.GetValue(any)));
                }

                return hash;
            }

            return any;
        }

        public class TemplateEqualConstraint : Constraint
        {
            private readonly string _cleanExpected;

            public TemplateEqualConstraint(string expected) => _cleanExpected = Clean(expected);

            public override string Description
            {
                get => string.Concat("equal to ", '"', _cleanExpected, '"');
                protected set { }
            }

            public override ConstraintResult ApplyTo<TActual>(TActual actual)
            {
                var cleanActual = Clean(actual);
                var isSuccess = cleanActual == _cleanExpected;
                return new ConstraintResult(this, cleanActual, isSuccess);
            }

            private static string Clean(object str) => Regex.Replace(str?.ToString() ?? "", @"\s*[\r\n]+\s*", "\n");
        }

    }
}
