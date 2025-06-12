namespace MainCore.Services
{
    public interface ITelegramService
    {
        Task SendText(string message, AccountId accountId);
    }
}
