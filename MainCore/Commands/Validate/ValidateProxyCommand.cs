using MainCore.Commands.Base;
using MainCore.DTO;
using Polly;
using Polly.Retry;
using RestSharp;

namespace MainCore.Commands.Validate
{
    public class ValidateProxyCommand : ICommand<bool>
    {
        public AccessDto Access { get; }

        public ValidateProxyCommand(AccessDto access)
        {
            Access = access;
        }
    }

    [RegisterAsTransient]
    public class ValidateProxyCommandHandler : ICommandHandler<ValidateProxyCommand, bool>
    {
        private readonly IRestClientManager _restClientManager;

        public ValidateProxyCommandHandler(IRestClientManager restClientManager)
        {
            _restClientManager = restClientManager;
        }

        private readonly AsyncRetryPolicy<bool> _retryPolicy = Policy<bool>
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: times => TimeSpan.FromSeconds(10 * times));

        public bool Value { get; private set; }

        private async Task<bool> Validate(AccessDto access)
        {
            var request = new RestRequest
            {
                Method = Method.Get,
            };
            var client = _restClientManager.Get(access);
            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful) throw new Exception("Proxy failed");
            return true;
        }

        public async Task<Result> Handle(ValidateProxyCommand command, CancellationToken cancellationToken)
        {
            var poliResult = await _retryPolicy
                    .ExecuteAndCaptureAsync(() => Validate(command.Access));
            Value = poliResult.Result;
            return Result.Ok();
        }
    }
}