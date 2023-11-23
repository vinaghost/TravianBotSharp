using FluentAssertions;
using MainCore;
using MainCore.Common.Enums;

namespace TestProject
{
    [TestClass]
    public class ConfigureServicesTest
    {
        [TestMethod]
        [DataRow(ServerEnums.TravianOfficial)]
        [DataRow(ServerEnums.TTWars)]
        public void ConfigureServicesTest_ShouldNotThrow(ServerEnums server)
        {
            var setup = () => DependencyInjection.Setup(server);
            setup.Should().NotThrow();
        }
    }
}