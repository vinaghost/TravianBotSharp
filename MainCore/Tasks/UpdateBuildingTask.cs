using MainCore.Tasks.Base;
using Microsoft.Extensions.DependencyInjection;

namespace MainCore.Tasks
{
    [RegisterTransient(Registration = RegistrationStrategy.Self)]
    public class UpdateBuildingTask : VillageTask
    {
        protected override async Task<Result> Execute(IServiceScope scoped, CancellationToken cancellationToken)
        {
            var dataService = scoped.ServiceProvider.GetRequiredService<DataService>();
            var chromeBrowser = dataService.ChromeBrowser;
            var url = chromeBrowser.CurrentUrl;
            Result result;

            var updateBuildingCommand = scoped.ServiceProvider.GetRequiredService<UpdateBuildingCommand>();
            var toDorfCommand = scoped.ServiceProvider.GetRequiredService<ToDorfCommand>();

            if (url.Contains("dorf1"))
            {
                result = await updateBuildingCommand.Execute(cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                result = await toDorfCommand.Execute(2, cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                result = await updateBuildingCommand.Execute(cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }
            else if (url.Contains("dorf2"))
            {
                result = await updateBuildingCommand.Execute(cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                result = await toDorfCommand.Execute(1, cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                result = await updateBuildingCommand.Execute(cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }
            else
            {
                result = await toDorfCommand.Execute(2, cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                result = await updateBuildingCommand.Execute(cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                result = await toDorfCommand.Execute(1, cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                result = await updateBuildingCommand.Execute(cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }

            return Result.Ok();
        }

        protected override string TaskName => "Update building";
    }
}