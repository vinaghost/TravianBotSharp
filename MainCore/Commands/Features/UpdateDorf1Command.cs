using FluentResults;
using MainCore.Common.Errors;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Services;
using MediatR;

namespace MainCore.Commands.Special
{
    public class UpdateDorf1Command : ByAccountVillageIdBase, IRequest<Result>
    {
        public UpdateDorf1Command(AccountId accountId, VillageId villageId) : base(accountId, villageId)
        {
        }
    }

    public class UpdateDorf1CommandHandler : IRequestHandler<UpdateDorf1Command, Result>
    {
        private readonly IChromeManager _chromeManager;
        private readonly IUnitOfCommand _unitOfCommand;

        public UpdateDorf1CommandHandler(IChromeManager chromeManager, IUnitOfCommand unitOfCommand)
        {
            _chromeManager = chromeManager;
            _unitOfCommand = unitOfCommand;
        }

        public async Task<Result> Handle(UpdateDorf1Command request, CancellationToken cancellationToken)
        {
            return await Execute(request.AccountId, request.VillageId, cancellationToken);
        }

        public async Task<Result> Execute(AccountId accountId, VillageId villageId, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(accountId);
            var url = chromeBrowser.CurrentUrl;
            Result result;
            await chromeBrowser.Refresh(cancellationToken);
            if (url.Contains("dorf1"))
            {
                result = await _unitOfCommand.UpdateDorfCommand.Execute(accountId, villageId);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            }
            else if (url.Contains("dorf2"))
            {
                result = await _unitOfCommand.UpdateDorfCommand.Execute(accountId, villageId);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
                result = await _unitOfCommand.ToDorfCommand.Execute(accountId, 1, cancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
                result = await _unitOfCommand.UpdateDorfCommand.Execute(accountId, villageId);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            }
            else
            {
                result = await _unitOfCommand.ToDorfCommand.Execute(accountId, 1, cancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
                result = await _unitOfCommand.UpdateDorfCommand.Execute(accountId, villageId);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            }

            return Result.Ok();
        }
    }
}