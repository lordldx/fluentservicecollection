using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;

namespace fluentservicecollection
{
    public class SelectedTypesContext
    {
        private readonly IServiceCollection _services;

        private Func<Type, Type> _getServiceType;

        public IList<Type> Types { get; } = new List<Type>();

        public SelectedTypesContext(IServiceCollection services, Type interfaceType)
        {
            _services = services;

            _getServiceType = type => interfaceType;
        }

        public IServiceCollection AsSingleton()
        {
            foreach (var type in Types)
            {
                var serviceType = _getServiceType(type);
                if (serviceType != null && serviceType.IsAssignableFrom(type))
                    _services.AddSingleton(serviceType, type);
            }

            return _services;
        }

        public IServiceCollection Scoped()
        {
            foreach (var type in Types)
            {
                var serviceType = _getServiceType(type);
                if (serviceType != null && serviceType.IsAssignableFrom(type))
                    _services.AddScoped(serviceType, type);
            }

            return _services;
        }

        public SelectedTypesContext WithServiceFirstInterface()
        {
            _getServiceType = classType => classType.GetInterfaces().FirstOrDefault();
            return this;
        }

        public SelectedTypesContext WithServiceLastInterface()
        {
            _getServiceType = classType => classType.GetInterfaces().LastOrDefault();
            return this;
        }

        public SelectedTypesContext WithServiceDefaultInterface()
        {
            _getServiceType = classType => classType.GetInterfaces().FirstOrDefault(i => i.Name == $"I{classType.Name}");
            return this;
        }

        public SelectedTypesContext WithService<T>()
        {
            return WithService(typeof(T));
        }

        public SelectedTypesContext WithService(Type typedef)
        {
            _getServiceType = classType => typedef;
            return this;
        }

        public SelectedTypesContext AsSelf()
        {
            _getServiceType = classType => classType;
            return this;
        }

        public SelectedTypesContext Except<T>()
        {
            return Except(typeof(T));
        }

        public SelectedTypesContext Except(Type t)
        {
            Types.Remove(t);
            return this;
        }
    }
}