using MainCore.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;

namespace MainCore.Test
{
    public class HostingTest
    {
        [Fact]
        public void HostShouldNotBeNull()
        {
            var host = AppMixins
                .GetHostBuilder()
                .UseDefaultServiceProvider((hostContext, config) =>
                {
                    config.ValidateOnBuild = true;
                    config.ValidateScopes = true;
                })
                .Build();

            host.ShouldNotBeNull();
        }

        [Fact]
        public void ReturnSameInstanceForSingleton()
        {
            var host = AppMixins
                .GetHostBuilder()
                .Build();

            host.Services.UseMicrosoftDependencyResolver();
            var serviceProvider = host.Services;

            serviceProvider.ShouldNotBeNull();

            var userAgentManager1 = serviceProvider.GetRequiredService<IUseragentManager>();
            var userAgentManager2 = Locator.Current.GetService<IUseragentManager>();

            userAgentManager1.ShouldBeSameAs(userAgentManager2);
        }
    }
}