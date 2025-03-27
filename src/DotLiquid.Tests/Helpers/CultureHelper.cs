using System;
using System.Globalization;

namespace DotLiquid.Tests.Helpers
{
    public static class CultureHelper
    {
        public static IDisposable SetCulture(string name)
        {
            var scope = new CultureScope(CultureInfo.CurrentCulture);
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(name);
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
                System.Threading.Thread.CurrentThread.CurrentCulture =  culture;
            }
        }
    }
}