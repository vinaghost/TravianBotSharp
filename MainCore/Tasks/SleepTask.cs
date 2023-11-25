using FluentResults;
using MainCore.Commands.General;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class SleepTask : AccountTask
    {
        private readonly IChooseAccessCommand _chooseAccessCommand;
        private readonly ISleepCommand _sleepCommand;
        private readonly IWorkCommand _workCommand;
        private readonly IUnitOfRepository _unitOfRepository;

        public SleepTask(IChooseAccessCommand chooseAccessCommand, ISleepCommand sleepCommand, IWorkCommand workCommand, IUnitOfRepository unitOfRepository)
        {
            _chooseAccessCommand = chooseAccessCommand;
            _sleepCommand = sleepCommand;
            _workCommand = workCommand;
            _unitOfRepository = unitOfRepository;
        }

        protected override  async Task<Result> Execute()
        {
            if (CancellationToken.IsCancellationRequested) return new Cancel();

            var sleepTimeMinutes = _unitOfRepository.AccountSettingRepository.GetByName(AccountId, AccountSettingEnums.SleepTimeMin, AccountSettingEnums.SleepTimeMax);
            Result result;
            result = await _sleepCommand.Execute(AccountId, TimeSpan.FromMinutes(sleepTimeMinutes), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _chooseAccessCommand.Execute(AccountId, false);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            var access = _chooseAccessCommand.Value;
            result = _workCommand.Execute(AccountId, access);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            SetNextExecute();

            return Result.Ok();
        }

        private void SetNextExecute()
        {
            var workTime = _unitOfRepository.AccountSettingRepository.GetByName(AccountId, AccountSettingEnums.WorkTimeMin, AccountSettingEnums.WorkTimeMax);
            ExecuteAt = DateTime.Now.AddMinutes(workTime);
        }

        protected override void SetName()
        {
            _name = "Sleep task";
        }
    }
}