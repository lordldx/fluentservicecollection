using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace fluentservicecollection
{
    public static class ServiceCollectionExtensions
    {
        public static SelectedTypesContext AddImplementationsOfType<T>(this IServiceCollection services)
        {
            return AddImplementationsOfType(services, typeof(T));
        }

        public static SelectedTypesContext AddImplementationsOfType(this IServiceCollection services, Type typedef)
        {
            var ctx = new SelectedTypesContext(services, typedef);
            if (typedef.IsGenericType)
                WithAllAssemblies(assembly => WithClassesOfGenericType(assembly, typedef, type => ctx.Types.Add(type)));
            else 
                WithAllAssemblies(assembly => WithClassesOfType(assembly, typedef, type => ctx.Types.Add(type)));

            return ctx;
        }

        private static void WithAllAssemblies(Action<Assembly> action)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                action(assembly);
            }
        }

        private static void WithClassesOfType(Assembly assembly, Type typedef, Action<Type> action)
        {
            foreach (var type in assembly.GetTypes()
                .Where(t => typedef.IsAssignableFrom(t) &&
                            t.IsClass &&
                            !t.IsAbstract))
            {
                action(type);
            }
        }

        private static void WithClassesOfGenericType(Assembly assembly, Type typedef, Action<Type> action)
        {
            foreach (var type in assembly.GetTypes()
                .Where(t => t.IsAssignableToGenericType(typedef) &&
                            t.IsClass &&
                            !t.IsAbstract))
            {
                action(type);
            }
        }
    }
}