using MediatR;
using System.Diagnostics;

namespace MainCore.Commands.UI.DebugTab
{
    public class LeftCommand : IRequest
    {
    }

    public class LeftCommandHandler : IRequestHandler<LeftCommand>
    {
        private const string _url = "https://ko-fi.com/vinaghost";

        public async Task Handle(LeftCommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            Process.Start(new ProcessStartInfo
            {
                FileName = _url,
                UseShellExecute = true
            });
        }
    }
}