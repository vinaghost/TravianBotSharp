namespace MainCore.Commands.Base
{
    public interface ICommand : IBaseCommand
    { }

    public interface ICommand<out TResponse> : IBaseCommand
    { }

    public interface IBaseCommand
    { }
}