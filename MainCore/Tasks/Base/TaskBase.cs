using FluentResults;
using MainCore.Commands;
using MainCore.Common.Enums;
using MainCore.Repositories;
using MediatR;

namespace MainCore.Tasks.Base
{
    public abstract class TaskBase
    {
        protected readonly UnitOfCommand _unitOfCommand;
        protected readonly UnitOfRepository _unitOfRepository;
        protected readonly IMediator _mediator;

        protected TaskBase(UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator)
        {
            
            _unitOfRepository = unitOfRepository;
            _mediator = mediator;
        }

        public StageEnums Stage { get; set; }
        public DateTime ExecuteAt { get; set; }
        public CancellationToken CancellationToken { get; set; }

        public async Task<Result> Handle()
        {
            Result result;
            result = await PreExecute();
            if (result.IsFailed) return result;
            result = await Execute();
            if (result.IsFailed) return result;
            result = await PostExecute();
            if (result.IsFailed) return result;
            return Result.Ok();
        }

        protected abstract Task<Result> Execute();

        protected virtual async Task<Result> PreExecute()
        {
            await Task.CompletedTask;
            return Result.Ok();
        }

        protected virtual async Task<Result> PostExecute()
        {
            await Task.CompletedTask;
            return Result.Ok();
        }

        public string GetName()
        {
            if (string.IsNullOrWhiteSpace(_name))
            {
                SetName();
            }
            return _name;
        }

        protected string _name;

        protected abstract void SetName();
    }
}