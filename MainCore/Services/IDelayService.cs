namespace MainCore.Services
{
    public interface IDelayService
    {
        Task DelayClick(CancellationToken cancellationToken = default);

        Task DelayTask(CancellationToken cancellationToken = default);
    }
}
