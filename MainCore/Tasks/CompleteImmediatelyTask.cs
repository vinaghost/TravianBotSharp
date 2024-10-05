using MainCore.Commands.Features.CompleteImmediately;
using MainCore.Tasks.Base;
using Microsoft.Extensions.DependencyInjection;

namespace MainCore.Tasks
{
    [RegisterTransient(Registration = RegistrationStrategy.Self)]
    public class CompleteImmediatelyTask : VillageTask
    {
        private readonly GetVillageName _getVillageName;

        public CompleteImmediatelyTask(GetVillageName getVillageName)
        {
            _getVillageName = getVillageName;
        }

        protected override async Task<Result> Execute(IServiceScope scoped, CancellationToken cancellationToken)
        {
            Result result;
            var toDorfCommand = scoped.ServiceProvider.GetRequiredService<ToDorfCommand>();
            result = await toDorfCommand.Execute(0, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var completeImmediatelyCommand = scoped.ServiceProvider.GetRequiredService<CompleteImmediatelyCommand>();
            result = await completeImmediatelyCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var updateBuildingCommand = scoped.ServiceProvider.GetRequiredService<UpdateBuildingCommand>();
            result = await updateBuildingCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        protected override void SetName()
        {
            var villageName = _getVillageName.Execute(VillageId);
            _name = $"Complete immediately in {villageName}";
        }
    }
}