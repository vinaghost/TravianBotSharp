using FluentResults;
using MainCore.Commands.Base;
using MainCore.Infrasturecture.AutoRegisterDi;

namespace MainCore.Commands.Navigate.ToQuestPageCommandHandler
{
    [RegisterAsTransient(Common.Enums.ServerEnums.TTWars)]
    public class TTWars : ICommandHandler<ToQuestPageCommand>
    {
        public async Task<Result> Handle(ToQuestPageCommand command, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return Result.Ok();
        }
    }
}