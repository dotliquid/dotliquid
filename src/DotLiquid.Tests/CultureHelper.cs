using System;
using System.Globalization;

namespace DotLiquid
{
    internal static class CultureHelper
    {
        public static IDisposable SetCulture(string name) => SetCulture(new CultureInfo(name));

        public static IDisposable SetCulture(CultureInfo culture)
        {
            var scope = new CultureScope(CultureInfo.CurrentCulture);

#if CORE
            CultureInfo.CurrentCulture = culture;
#else
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
#endif
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
#if CORE
                CultureInfo.CurrentCulture = this.culture;
#else
                System.Threading.Thread.CurrentThread.CurrentCulture =  this.culture;
#endif
            }
        }
    }
}