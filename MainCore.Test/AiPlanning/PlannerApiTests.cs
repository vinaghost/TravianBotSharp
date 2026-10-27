using System.Net;
using System.Text;
using System.Text.Json;
using MainCore.Services.AiPlanning;

namespace MainCore.Test.AiPlanning
{
    public class PlannerApiTests
    {
        private sealed class FakeHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> send) : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => send(request);
        }
        private static HttpResponseMessage Json(object value) => new(HttpStatusCode.OK) { Content = new StringContent(PlannerJson.Write(value), Encoding.UTF8, "application/json") };

        [Fact]
        public async Task FreeCatalogAuthenticatesAndExcludesPaidOrUnknownPricing()
        {
            var paths = new List<string>();
            using var http = new HttpClient(new FakeHandler(request =>
            {
                paths.Add(request.RequestUri!.AbsolutePath);
                request.Headers.Authorization!.Parameter.ShouldBe("test-key");
                if (request.RequestUri.AbsolutePath.EndsWith("/key")) return Task.FromResult(Json(new { data = new { } }));
                return Task.FromResult(Json(new { data = new[]
                {
                    new { id = "vendor/model:free", pricing = new { prompt = "0", completion = "0", request = "0" }, supported_parameters = new[] { "structured_outputs", "response_format" } },
                    new { id = "vendor/paid", pricing = new { prompt = "0.01", completion = "0.01", request = "0" }, supported_parameters = Array.Empty<string>() },
                    new { id = "vendor/notfree:free", pricing = new { prompt = "0", completion = "0", request = "1" }, supported_parameters = Array.Empty<string>() }
                }}));
            }));
            var models = await new PlannerApiClient(http).GetModelsAsync(AiProvider.OpenRouter, "test-key", true, TestContext.Current.CancellationToken);
            paths.ShouldBe(new[] { "/api/v1/key", "/api/v1/models" });
            models.Select(x => x.Id).ShouldBe(new[] { "openrouter/free", "vendor/model:free" });
            models.ShouldAllBe(x => x.IsFree);
        }

        [Theory]
        [InlineData(AiProvider.OpenRouter)]
        [InlineData(AiProvider.OpenAI)]
        public async Task SendsCorrectEndpointAndParsesCompletedPlan(AiProvider provider)
        {
            var options = new PlannerOptions { Provider = provider, Model = provider == AiProvider.OpenRouter ? "openrouter/free" : "test-openai" };
            using var http = new HttpClient(new FakeHandler(async request =>
            {
                request.RequestUri!.Host.ShouldBe(provider == AiProvider.OpenRouter ? "openrouter.ai" : "api.openai.com");
                request.RequestUri.AbsolutePath.ShouldEndWith(provider == AiProvider.OpenRouter ? "/chat/completions" : "/responses");
                var body = await request.Content!.ReadAsStringAsync();
                body.ShouldNotContain("test-key");
                using var parsed = JsonDocument.Parse(body);
                if (provider == AiProvider.OpenRouter)
                {
                    parsed.RootElement.GetProperty("provider").GetProperty("max_price").GetProperty("prompt").GetInt32().ShouldBe(0);
                    parsed.RootElement.TryGetProperty("models", out _).ShouldBeFalse();
                    parsed.RootElement.GetProperty("response_format").GetProperty("type").GetString().ShouldBe("json_schema");
                }
                else
                {
                    parsed.RootElement.GetProperty("store").GetBoolean().ShouldBeFalse();
                    parsed.RootElement.GetProperty("text").GetProperty("format").GetProperty("strict").GetBoolean().ShouldBeTrue();
                }
                var proposal = PlannerJson.Write(PlannerFixture.Proposal(PlannerFixture.Step()));
                return provider == AiProvider.OpenRouter
                    ? Json(new { model = options.Model, choices = new[] { new { finish_reason = "stop", message = new { content = proposal } } }, usage = new { prompt_tokens = 123, completion_tokens = 45 } })
                    : Json(new { model = options.Model, status = "completed", output = new[] { new { type = "message", content = new[] { new { type = "output_text", text = proposal } } } }, usage = new { input_tokens = 123, output_tokens = 45 } });
            }));
            var result = await new PlannerApiClient(http).GenerateAsync(options, "test-key", new(options.Model, provider == AiProvider.OpenRouter, true, true), "village context", TestContext.Current.CancellationToken);
            result.Proposal.Steps.Length.ShouldBe(1);
            result.InputTokens.ShouldBe(123);
            result.OutputTokens.ShouldBe(45);
        }

        [Fact]
        public async Task FreeOnlyRejectsPaidModelBeforeSending()
        {
            using var http = new HttpClient(new FakeHandler(_ => throw new Exception("Must not send")));
            await Should.ThrowAsync<InvalidOperationException>(() => new PlannerApiClient(http).GenerateAsync(new() { Model = "paid" }, "test", new("paid", false, true, true), "", TestContext.Current.CancellationToken));
        }

        [Theory]
        [InlineData(401)]
        [InlineData(429)]
        [InlineData(500)]
        public async Task ErrorsDoNotEchoSecretsOrRetryPaidRequests(int status)
        {
            var calls = 0;
            using var http = new HttpClient(new FakeHandler(_ =>
            {
                calls++;
                return Task.FromResult(new HttpResponseMessage((HttpStatusCode)status) { Content = new StringContent("test-secret-key") });
            }));
            var error = await Should.ThrowAsync<InvalidOperationException>(() => new PlannerApiClient(http).GenerateAsync(new(), "test-secret-key", new("openrouter/free", true, true, true), "", TestContext.Current.CancellationToken));
            error.Message.ShouldNotContain("test-secret-key");
            calls.ShouldBe(1);
        }

        [Theory]
        [InlineData("length", "{}")]
        [InlineData("stop", "not JSON")]
        [InlineData("stop", "{\"assessment\":\"Test\",\"steps\":[],\"followUpActions\":[],\"assumptions\":[],\"execute\":\"delete\"}")]
        public void RejectsIncompleteMalformedOrUnexpectedOutput(string finish, string text)
        {
            using var doc = JsonDocument.Parse(PlannerJson.Write(new { choices = new[] { new { finish_reason = finish, message = new { content = text } } } }));
            Should.Throw<InvalidOperationException>(() => PlannerApiClient.ReadResponse(AiProvider.OpenRouter, "test", doc.RootElement));
        }

        [Fact]
        public void HandlesOpenAiRefusal()
        {
            using var doc = JsonDocument.Parse("""{"status":"completed","output":[{"content":[{"type":"refusal","refusal":"declined"}]}]}""");
            Should.Throw<InvalidOperationException>(() => PlannerApiClient.ReadResponse(AiProvider.OpenAI, "test", doc.RootElement));
        }

        [Fact]
        public void NonSchemaModelUsesJsonModeOrPromptOnlyWithoutPretendingSchemaSupport()
        {
            using var json = JsonDocument.Parse(PlannerJson.Write(PlannerApiClient.BuildRequest(new(), new("openrouter/free", true, false, true), "context")));
            json.RootElement.GetProperty("response_format").GetProperty("type").GetString().ShouldBe("json_object");
            using var plain = JsonDocument.Parse(PlannerJson.Write(PlannerApiClient.BuildRequest(new(), new("openrouter/free", true, false, false), "context")));
            plain.RootElement.TryGetProperty("response_format", out _).ShouldBeFalse();
        }
    }
}
