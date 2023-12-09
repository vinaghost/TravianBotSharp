using FluentResults;
using MainCore.Commands.Base;
using MainCore.Common.MediatR;
using MainCore.Common.Models;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MediatR;

namespace MainCore.Commands.Features.Step.UpgradeBuilding
{
    public class AddCroplandCommand : ByAccountVillageIdBase, ICommand
    {
        public AddCroplandCommand(AccountId accountId, VillageId villageId) : base(accountId, villageId)
        {
        }
    }

    [RegisterAsTransient]
    public class AddCroplandCommandHandler : ICommandHandler<AddCroplandCommand>
    {
        private readonly UnitOfRepository _unitOfRepository;
        private readonly IMediator _mediator;

        public AddCroplandCommandHandler(UnitOfRepository unitOfRepository, IMediator mediator)
        {
            _unitOfRepository = unitOfRepository;
            _mediator = mediator;
        }

        public async Task<Result> Handle(AddCroplandCommand command, CancellationToken cancellationToken)
        {
            var cropland = _unitOfRepository.BuildingRepository.GetCropland(command.VillageId);

            var plan = new NormalBuildPlan()
            {
                Location = cropland.Location,
                Type = cropland.Type,
                Level = cropland.Level + 1,
            };

            _unitOfRepository.JobRepository.AddToTop(command.VillageId, plan);
            await _mediator.Publish(new JobUpdated(command.AccountId, command.VillageId), cancellationToken);
            return Result.Ok();
        }
    }
}