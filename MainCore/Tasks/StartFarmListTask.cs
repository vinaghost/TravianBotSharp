using FluentResults;
using MainCore.Commands;
using MainCore.Commands.Special;
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
        private readonly IUnitOfRepository _unitOfRepository;
        private readonly IUnitOfCommand _unitOfCommand;
        private readonly IMediator _mediator;

        public StartFarmListTask(IMediator mediator, IUnitOfCommand unitOfCommand, IUnitOfRepository unitOfRepository)
        {
            _mediator = mediator;
            _unitOfCommand = unitOfCommand;
            _unitOfRepository = unitOfRepository;
        }

        public override async Task<Result> Execute()
        {
            if (CancellationToken.IsCancellationRequested) return new Cancel();
            Result result;
            result = await _mediator.Send(new ToFarmListPageCommand(AccountId));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            result = await _mediator.Send(new StartFarmListCommand(AccountId));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            SetNextExecute();
            return Result.Ok();
        }

        private void SetNextExecute()
        {
            var workTime = _unitOfRepository.AccountSettingRepository.GetByName(AccountId, AccountSettingEnums.FarmIntervalMin, AccountSettingEnums.FarmIntervalMax);
            ExecuteAt = DateTime.Now.AddMinutes(workTime);
        }

        protected override void SetName()
        {
            _name = "Start farm list";
        }
    }
}