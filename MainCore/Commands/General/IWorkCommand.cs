using FluentResults;
using MainCore.DTO;
using MainCore.Entities;

namespace MainCore.Commands.General
{
    public interface IWorkCommand
    {
        Result Execute(AccountId accountId, AccessDto access);
    }
}