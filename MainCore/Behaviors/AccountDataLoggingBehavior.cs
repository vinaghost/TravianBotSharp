using MainCore.Infrasturecture.Extensions;
using MainCore.Tasks.Base;

using Serilog.Context;

namespace MainCore.Behaviors
{
    public sealed class AccountDataLoggingBehavior<TRequest, TResponse>(IDbContextFactory<AppDbContext> contextFactory)
       : Behavior<TRequest, TResponse>
           where TRequest : AccountTask
    {
        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            using (LogContext.PushProperty("AccountId", request.AccountId))
            using (LogContext.PushProperty("Account", GetAccountData(request.AccountId)))
            {
                return await Next(request, cancellationToken);
            }
        }

        private string GetAccountData(AccountId accountId)
        {
            using var context = contextFactory.CreateDbContext();
            return context.GetAccountData(accountId);
        }
    }
}