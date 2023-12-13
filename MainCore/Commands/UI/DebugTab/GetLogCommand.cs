using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Services;
using MediatR;
using Serilog.Core;
using Serilog.Formatting.Display;
using System.Text;

namespace MainCore.Commands.UI.DebugTab
{
    public class GetLogCommand : ByAccountIdBase, IRequest<string>
    {
        public GetLogCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class GetLogCommandHandler : IRequestHandler<GetLogCommand, string>
    {
        private static readonly MessageTemplateTextFormatter _formatter = new("{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}");
        private readonly LogSink _logSink;

        public GetLogCommandHandler(ILogEventSink logSink)
        {
            _logSink = logSink as LogSink;
        }

        public async Task<string> Handle(GetLogCommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var logs = _logSink.GetLogs(request.AccountId);
            var buffer = new StringWriter(new StringBuilder());

            foreach (var log in logs)
            {
                _formatter.Format(log, buffer);
            }
            return buffer.ToString();
        }
    }
}