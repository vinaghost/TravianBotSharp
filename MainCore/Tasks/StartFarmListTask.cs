using FluentResults;
using MainCore.Commands;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;
using MainCore.Tasks.Base;
using MediatR;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class StartFarmListTask : AccountTask
    {
        public StartFarmListTask(IUnitOfCommand unitOfCommand, IUnitOfRepository unitOfRepository, IMediator mediator) : base(unitOfCommand, unitOfRepository, mediator)
        {
        }

        protected override async Task<Result> Execute()
        {
            if (CancellationToken.IsCancellationRequested) return new Cancel();
            await Task.CompletedTask;
            //result = await _mediator.Send(new ToFarmListPageCommand(AccountId));
            //if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            //result = await _mediator.Send(new StartFarmListCommand(AccountId));
            //if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            SetNextExecute();
            return Result.Ok();
        }

        private void SetNextExecute()
        {
            var seconds = _unitOfRepository.AccountSettingRepository.GetByName(AccountId, AccountSettingEnums.FarmIntervalMin, AccountSettingEnums.FarmIntervalMax);
            ExecuteAt = DateTime.Now.AddSeconds(seconds);
        }

        protected override void SetName()
        {
            _name = "Start farm list";
        }
    }
}