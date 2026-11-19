using ArchUnitNET.Domain;
using ArchUnitNET.Loader;

namespace MainCore.Test.ArchitectureTests
{
    public abstract class BaseTest
    {
        protected static readonly Architecture Architecture = new ArchLoader()
            .LoadAssemblies(typeof(AppMixins).Assembly)
            .Build();
    }
}