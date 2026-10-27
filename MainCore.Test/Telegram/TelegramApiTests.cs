using System.Net;
using System.Text.Json;
using MainCore.Services.Telegram;

namespace MainCore.Test.Telegram;

public class TelegramApiTests
{
    private const string Token = "123456:abcdefghijklmnopqrstuvwxyz";
    private sealed class Handler(Func<HttpRequestMessage, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => send(request);
    }
    private static HttpResponseMessage Json(string json, int status = 200) => new((HttpStatusCode)status) { Content = new StringContent(json) };

    [Fact]
    public async Task SendsPlainTextToTheExactChatWithNoPaidBroadcastOrLinkPreview()
    {
        using var http = new HttpClient(new Handler(async request =>
        {
            request.RequestUri!.Host.ShouldBe("api.telegram.org");
            request.RequestUri.AbsolutePath.ShouldBe($"/bot{Token}/sendMessage");
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync());
            body.RootElement.GetProperty("chat_id").GetInt64().ShouldBe(-1001234567890L);
            body.RootElement.GetProperty("text").GetString().ShouldBe("Village <b>name</b>");
            body.RootElement.TryGetProperty("parse_mode", out _).ShouldBeFalse();
            body.RootElement.GetProperty("disable_notification").GetBoolean().ShouldBeTrue();
            body.RootElement.GetProperty("allow_paid_broadcast").GetBoolean().ShouldBeFalse();
            body.RootElement.GetProperty("link_preview_options").GetProperty("is_disabled").GetBoolean().ShouldBeTrue();
            return Json("{\"ok\":true,\"result\":{\"message_id\":42}}");
        }));
        await new TelegramApiClient(http).SendAsync(Token, "-1001234567890", "Village <b>name</b>", true, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(400)] [InlineData(401)] [InlineData(403)] [InlineData(409)] [InlineData(500)]
    public async Task ApiErrorsNeverEchoTheTokenOrRawTelegramDescription(int status)
    {
        using var http = new HttpClient(new Handler(_ => Task.FromResult(Json($"{{\"ok\":false,\"error_code\":{status},\"description\":\"{Token}\"}}", status))));
        var error = await Should.ThrowAsync<TelegramDeliveryException>(() => new TelegramApiClient(http).SendAsync(Token, "42", "test", false, TestContext.Current.CancellationToken));
        error.ToString().ShouldNotContain(Token);
        error.RetryAfter.ShouldBe(0);
    }

    [Fact]
    public async Task NetworkErrorsDoNotLeakTheTokenInTheRequestUrl()
    {
        using var http = new HttpClient(new Handler(_ => throw new HttpRequestException($"Request failed at https://api.telegram.org/bot{Token}/sendMessage")));
        var error = await Should.ThrowAsync<TelegramDeliveryException>(() => new TelegramApiClient(http).SendAsync(Token, "42", "test", false, TestContext.Current.CancellationToken));
        error.ToString().ShouldNotContain(Token);
        error.InnerException.ShouldBeNull();
    }

    [Fact]
    public async Task ExposesTelegramRateLimitDelayWithoutRetryingInsideHttpClient()
    {
        var calls = 0;
        using var http = new HttpClient(new Handler(_ => { calls++; return Task.FromResult(Json("{\"ok\":false,\"error_code\":429,\"parameters\":{\"retry_after\":12}}", 429)); }));
        var error = await Should.ThrowAsync<TelegramDeliveryException>(() => new TelegramApiClient(http).SendAsync(Token, "42", "test", false, TestContext.Current.CancellationToken));
        error.RetryAfter.ShouldBe(12);
        calls.ShouldBe(1);
    }

    [Fact]
    public async Task ChatDiscoveryDoesNotAcknowledgeUpdatesAndReturnsOnlyChatMetadata()
    {
        using var http = new HttpClient(new Handler(async request =>
        {
            request.RequestUri!.AbsolutePath.ShouldEndWith("/getUpdates");
            var body = await request.Content!.ReadAsStringAsync();
            body.ShouldNotContain("offset");
            return Json("""{"ok":true,"result":[{"message":{"chat":{"id":42,"first_name":"Player"},"text":"private message"}},{"message":{"chat":{"id":42,"first_name":"Player"}}},{"message":{"chat":{"id":-1001234567890,"title":"My group"}}}]}""");
        }));
        var chats = await new TelegramApiClient(http).FindChatsAsync(Token, TestContext.Current.CancellationToken);
        chats.Length.ShouldBe(2);
        JsonSerializer.Serialize(chats).ShouldNotContain("private message");
    }

    [Fact]
    public void MessageLengthLimitPreservesSurrogatePairs()
    {
        var text = new string('a', 3998) + "😀" + new string('b', 50);
        var limited = TelegramApiClient.LimitText(text);
        limited.Length.ShouldBeLessThanOrEqualTo(4000);
        limited.ShouldEndWith("…");
        limited.ShouldNotContain('\ud83d');
    }
}
