using FluentResults;
using MainCore.Common.Models;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MediatR;

namespace MainCore.Commands.Step.UpgradeBuilding
{
    [RegisterAsTransient]
    public class AddCroplandCommand : IAddCroplandCommand
    {
        private readonly IUnitOfRepository _unitOfRepository;
        private readonly IMediator _mediator;

        public AddCroplandCommand(IUnitOfRepository unitOfRepository, IMediator mediator)
        {
            _unitOfRepository = unitOfRepository;
            _mediator = mediator;
        }

        public async Task<Result> Execute(AccountId accountId, VillageId villageId)
        {
            var cropland = _unitOfRepository.BuildingRepository.GetCropland(villageId);

            var plan = new NormalBuildPlan()
            {
                Location = cropland.Location,
                Type = cropland.Type,
                Level = cropland.Level + 1,
            };

            _unitOfRepository.JobRepository.AddToTop(villageId, plan);
            await _mediator.Publish(new JobUpdated(accountId, villageId));
            return Result.Ok();
        }
    }
}