using MainCore.DTO;

namespace MainCore.Commands.Validate
{
    public interface IValidateProxyCommand
    {
        Task<bool> Execute(AccessDto access);
    }
}