namespace MainCore.Services
{
    public interface IUseragentManager : IDisposable
    {
        string Get();

        Task Load();
    }
}