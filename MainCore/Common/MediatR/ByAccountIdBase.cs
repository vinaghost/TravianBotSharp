namespace MainCore.Common.MediatR
{
    public class ByAccountIdBase
    {
        public AccountId AccountId { get; }

        public ByAccountIdBase(AccountId accountId)
        {
            AccountId = accountId;
        }
    }
}