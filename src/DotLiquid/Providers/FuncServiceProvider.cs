using System;

namespace DotLiquid.Providers
{
    public class FuncServiceProvider : IServiceProvider
    {
        private readonly Func<Type, object> _serviceProvider;

        public FuncServiceProvider(Func<Type,object> serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public object GetService(Type serviceType)
        {
            return _serviceProvider(serviceType);
        }
    }
}