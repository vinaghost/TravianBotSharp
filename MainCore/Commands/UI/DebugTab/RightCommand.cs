using MediatR;
using System.Diagnostics;

namespace MainCore.Commands.UI.DebugTab
{
    public class RightCommand : IRequest
    {
    }

    public class RightCommandHandler : IRequestHandler<RightCommand>
    {
        public async Task Handle(RightCommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(AppContext.BaseDirectory, "logs"),
                UseShellExecute = true
            });
        }
    }
}