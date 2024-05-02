using MainCore.Infrasturecture.Persistence;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MainCore.Infrasturecture.AutoRegisterDi
{
    public static class AutoRegisterHelpers
    {
        public static IServiceCollection AutoRegister(this IServiceCollection services)
        {
            var foundServices = GetAutoRegistered();
            foreach (var service in foundServices)
            {
                services.Add(new ServiceDescriptor(service.Interface, service.Class, service.Lifetime));
            }
            return services;
        }

        public static IEnumerable<AutoRegisteredResult> GetAutoRegistered()
        {
            var assembly = typeof(AppDbContext).Assembly;
            var allPublicTypes = assembly.GetExportedTypes()
                .Where(y => y.IsClass && !y.IsAbstract && !y.IsGenericType && !y.IsNested);

            foreach (var classType in allPublicTypes)
            {
                if (!classType.HasAttribute()) continue;

                var lifetimeForClass = classType.GetLifetimeForClass();

                var withoutInterface = classType.WithoutInterface();
                if (withoutInterface)
                {
                    yield return new AutoRegisteredResult(classType, classType, lifetimeForClass);
                }
                else
                {
                    var interfaces = classType.GetTypeInfo().ImplementedInterfaces;
                    var filteredInterfaces = interfaces.Where(i => i.IsPublic && !i.IsNested);
                    foreach (var infc in filteredInterfaces)
                    {
                        yield return new AutoRegisteredResult(classType, infc, lifetimeForClass);
                    }
                }
            }
        }
    }
}