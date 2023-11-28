using FluentResults;
using MainCore.Commands.Step.StartFarmlist;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Repositories;
using MediatR;

namespace MainCore.Commands.Special
{
    public class StartFarmListCommand : ByAccountIdBase, IRequest<Result>
    {
        public StartFarmListCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class StartFarmListCommandHandler : IRequestHandler<StartFarmListCommand, Result>
    {
        private readonly IStartSingleFarmListCommand _startSingleFarmListCommand;
        private readonly IStartAllFarmListCommand _startAllFarmListCommand;
        private readonly IUnitOfRepository _unitOfRepository;
        private readonly IUnitOfCommand _unitOfCommand;

        public StartFarmListCommandHandler(IStartSingleFarmListCommand startSingleFarmListCommand, IStartAllFarmListCommand startAllFarmListCommand, IUnitOfRepository unitOfRepository, IUnitOfCommand unitOfCommand)
        {
            _startSingleFarmListCommand = startSingleFarmListCommand;
            _startAllFarmListCommand = startAllFarmListCommand;
            _unitOfRepository = unitOfRepository;
            _unitOfCommand = unitOfCommand;
        }

        public async Task<Result> Handle(StartFarmListCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            Result result;
            var useStartAllButton = _unitOfRepository.AccountSettingRepository.GetBooleanByName(accountId, AccountSettingEnums.UseStartAllButton);
            if (useStartAllButton)
            {
                result = await _startAllFarmListCommand.Execute(accountId);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            }
            else
            {
                var farmLists = _unitOfRepository.FarmRepository.GetActive(accountId);
                if (farmLists.Count == 0) return Result.Fail(new Skip("No farmlist is active"));

                foreach (var farmList in farmLists)
                {
                    result = await _startSingleFarmListCommand.Execute(accountId, farmList);
                    if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

                    await _unitOfCommand.DelayClickCommand.Execute(accountId);
                }
            }
            return Result.Ok();
        }
    }
}