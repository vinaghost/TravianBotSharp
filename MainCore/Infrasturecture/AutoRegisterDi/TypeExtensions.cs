using MainCore.Common.Enums;
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

        public static bool IsServerCorrect(this Type type,
            ServerEnums correctServer)
        {
            var server = type
                .GetCustomAttribute<RegisterWithLifetimeAttribute>(true)
                .RequiredServer;
            if (server == ServerEnums.NONE) return true;
            return correctServer == server;
        }

        public static bool WithoutInterface(this Type type)
        {
            return type
                .GetCustomAttribute<RegisterWithLifetimeAttribute>(true)
                .WithoutInterface;
        }
    }
}