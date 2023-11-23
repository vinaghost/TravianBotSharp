using MainCore.Common.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace MainCore.Infrasturecture.AutoRegisterDi
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterWithLifetimeAttribute : Attribute
    {
        public RegisterWithLifetimeAttribute(ServiceLifetime requiredLifetime, ServerEnums requiredServer, bool withoutInterface)
        {
            RequiredLifetime = requiredLifetime;
            RequiredServer = requiredServer;
            WithoutInterface = withoutInterface;
        }

        public ServiceLifetime RequiredLifetime { get; }
        public ServerEnums RequiredServer { get; }
        public bool WithoutInterface { get; }
    }

    public class RegisterAsScopedAttribute : RegisterWithLifetimeAttribute
    {
        public RegisterAsScopedAttribute(ServerEnums requiredServer = ServerEnums.NONE, bool withoutInterface = false) : base(ServiceLifetime.Scoped, requiredServer, withoutInterface)
        {
        }
    }

    public class RegisterAsTransientAttribute : RegisterWithLifetimeAttribute
    {
        public RegisterAsTransientAttribute(ServerEnums requiredServer = ServerEnums.NONE, bool withoutInterface = false) : base(ServiceLifetime.Transient, requiredServer, withoutInterface)
        {
        }
    }

    public class RegisterAsSingletonAttribute : RegisterWithLifetimeAttribute
    {
        public RegisterAsSingletonAttribute(ServerEnums requiredServer = ServerEnums.NONE, bool withoutInterface = false) : base(ServiceLifetime.Singleton, requiredServer, withoutInterface)
        {
        }
    }

    /// <summary>
    /// Attribute for marking classes which no need to register in container
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DoNotAutoRegisterAttribute : Attribute
    {
    }
}