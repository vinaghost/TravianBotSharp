namespace MainCore.Test
{
    public class ServiceProviderTest
    {
        [Fact]
        public void ServiceProviderShouldNotBeNull()
        {
            var serviceProvider = DependencyInjection.Setup();
            serviceProvider.ShouldNotBeNull();
        }
    }
}