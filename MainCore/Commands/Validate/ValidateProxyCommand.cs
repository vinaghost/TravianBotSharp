using MainCore.DTO;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Services;
using Polly;
using Polly.Retry;
using RestSharp;

namespace MainCore.Commands.Validate
{
    [RegisterAsTransient]
    public class ValidateProxyCommand : IValidateProxyCommand
    {
        private readonly IRestClientManager _restClientManager;

        public ValidateProxyCommand(IRestClientManager restClientManager)
        {
            _restClientManager = restClientManager;
        }

        private readonly AsyncRetryPolicy<bool> _retryPolicy = Policy<bool>
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: _ => TimeSpan.FromSeconds(10));

        public async Task<bool> Execute(AccessDto access)
        {
            var poliResult = await _retryPolicy
                .ExecuteAndCaptureAsync(() => Validate(access));

            if (!poliResult.Result) return false;
            return true;
        }

        private async Task<bool> Validate(AccessDto access)
        {
            var request = new RestRequest
            {
                Method = Method.Get,
            };
            var client = _restClientManager.Get(access);
            var response = await client.ExecuteAsync(request);
            return response.IsSuccessful;
        }
    }
}