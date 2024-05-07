using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MainCore.Infrasturecture.AutoRegisterDi
{
    /// <summary>
    /// Extensions for <see cref="Type"/>
    /// </summary>
    internal static class TypeExtensions
    {
        public static bool HasAttribute(this Type type)
        {
            return type
                .GetCustomAttributes<RegisterWithLifetimeAttribute>(true)
                .Any();
        }

        public static ServiceLifetime GetLifetimeForClass(this Type type)
        {
            return type
                .GetCustomAttribute<RegisterWithLifetimeAttribute>(true)
                .RequiredLifetime;
        }

        public static bool WithoutInterface(this Type type)
        {
            return type
                .GetCustomAttribute<RegisterWithLifetimeAttribute>(true)
                .WithoutInterface;
        }
    }
}