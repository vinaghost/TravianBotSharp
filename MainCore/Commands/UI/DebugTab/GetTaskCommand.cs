using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Services;
using MainCore.UI.Models.Output;
using MediatR;

namespace MainCore.Commands.UI.Debug
{
    public class GetTaskCommand : ByAccountIdBase, IRequest<List<TaskItem>>
    {
        public GetTaskCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class GetTaskCommandHandler : IRequestHandler<GetTaskCommand, List<TaskItem>>
    {
        private readonly ITaskManager _taskManager;

        public GetTaskCommandHandler(ITaskManager taskManager)
        {
            _taskManager = taskManager;
        }

        public async Task<List<TaskItem>> Handle(GetTaskCommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var tasks = _taskManager.GetTaskList(request.AccountId);

            return tasks
                .Select(x => new TaskItem(x))
                .ToList();
        }
    }
}