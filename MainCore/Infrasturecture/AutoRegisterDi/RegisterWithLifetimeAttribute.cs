using Microsoft.Extensions.DependencyInjection;

namespace MainCore.Infrasturecture.AutoRegisterDi
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterWithLifetimeAttribute : Attribute
    {
        public RegisterWithLifetimeAttribute(ServiceLifetime requiredLifetime, bool withoutInterface)
        {
            RequiredLifetime = requiredLifetime;
            WithoutInterface = withoutInterface;
        }

        public ServiceLifetime RequiredLifetime { get; }
        public bool WithoutInterface { get; }
    }

    public class RegisterAsScopedAttribute : RegisterWithLifetimeAttribute
    {
        public RegisterAsScopedAttribute(bool withoutInterface = false) : base(ServiceLifetime.Scoped, withoutInterface)
        {
        }
    }

    public class RegisterAsTransientAttribute : RegisterWithLifetimeAttribute
    {
        public RegisterAsTransientAttribute(bool withoutInterface = false) : base(ServiceLifetime.Transient, withoutInterface)
        {
        }
    }

    public class RegisterAsSingletonAttribute : RegisterWithLifetimeAttribute
    {
        public RegisterAsSingletonAttribute(bool withoutInterface = false) : base(ServiceLifetime.Singleton, withoutInterface)
        {
        }
    }

    public class RegisterAsServiceAttribute : RegisterAsSingletonAttribute
    {
        public RegisterAsServiceAttribute() : base(false)
        {
        }
    }

    public class RegisterAsTaskAttribute : RegisterAsTransientAttribute
    {
        public RegisterAsTaskAttribute() : base(true)
        {
        }
    }

    public class RegisterAsViewModelAttribute : RegisterAsSingletonAttribute
    {
        public RegisterAsViewModelAttribute() : base(true)
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