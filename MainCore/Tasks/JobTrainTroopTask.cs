using FluentResults;
using MainCore.Commands;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;
using MainCore.Tasks.Base;
using MediatR;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class JobTrainTroopTask : VillageTask
    {
        public JobTrainTroopTask(UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator) : base(unitOfCommand, unitOfRepository, mediator)
        {
        }

        protected override async Task<Result> Execute()
        {
            await Task.CompletedTask;
            return Result.Ok();
        }

        protected override void SetName()
        {
            var name = _unitOfRepository.VillageRepository.GetVillageName(VillageId);
            _name = $"Job training troop in {name}";
        }
    }
}