using Polly;
using Polly.Retry;
using RestSharp;

namespace MainCore.Commands.Checks
{
    public class CheckProxyCommand
    {
        private readonly IRestClientManager _restClientManager;

        private static readonly AsyncRetryPolicy _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: times => TimeSpan.FromSeconds(10 * times));

        public CheckProxyCommand(IRestClientManager restClientManager = null)
        {
            _restClientManager = restClientManager ?? Locator.Current.GetService<IRestClientManager>();
        }

        private async Task Validate(AccessDto access)
        {
            var request = new RestRequest
            {
                Method = Method.Get,
            };
            var client = _restClientManager.Get(access);
            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful) throw new Exception("Proxy failed");
        }

        public async Task<bool> Execute(AccessDto access)
        {
            var poliResult = await _retryPolicy
                    .ExecuteAndCaptureAsync(() => Validate(access));

            if (poliResult.FinalException is not null) return false;
            return true;
        }
    }
}