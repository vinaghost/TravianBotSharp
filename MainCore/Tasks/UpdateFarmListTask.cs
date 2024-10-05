using MainCore.Commands.Features.StartFarmList;
using MainCore.Tasks.Base;
using Microsoft.Extensions.DependencyInjection;

namespace MainCore.Tasks
{
    [RegisterTransient(Registration = RegistrationStrategy.Self)]
    public class UpdateFarmListTask : AccountTask
    {
        protected override async Task<Result> Execute(IServiceScope scoped, CancellationToken cancellationToken)
        {
            Result result;

            var toFarmListPageCommand = scoped.ServiceProvider.GetRequiredService<ToFarmListPageCommand>();
            result = await toFarmListPageCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var updateFarmlistCommand = scoped.ServiceProvider.GetRequiredService<UpdateFarmlistCommand>();
            result = await updateFarmlistCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        protected override string TaskName => "Update farm list";
    }
}