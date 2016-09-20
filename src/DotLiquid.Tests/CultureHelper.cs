using System;
using System.Globalization;

namespace DotLiquid
{
    internal static class CultureHelper
    {
        public static IDisposable SetCulture(string name)
        {
            var scope = new CultureScope(CultureInfo.CurrentCulture);
            CultureInfo.CurrentCulture = new CultureInfo(name);
            return scope;
        }

        private class CultureScope : IDisposable
        {
            private readonly CultureInfo culture;

            public CultureScope(CultureInfo culture)
            {
                this.culture = culture;
            }

            public void Dispose()
            {
                CultureInfo.CurrentCulture = this.culture;
            }
        }
    }
}