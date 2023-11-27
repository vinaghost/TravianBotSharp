using FluentResults;
using MainCore.DTO;
using MainCore.Entities;

namespace MainCore.Commands.General
{
    public interface IOpenBrowserCommand
    {
        Task<Result> Execute(AccountId accountId, AccessDto access);
    }
}