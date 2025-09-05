using Microsoft.Extensions.Hosting;

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
    }
}
