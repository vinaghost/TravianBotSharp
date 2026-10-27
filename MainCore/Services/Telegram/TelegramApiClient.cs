using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MainCore.Services.Telegram;

public interface ITelegramTransport
{
    Task SendAsync(string token, string chatId, string text, bool silent, CancellationToken cancellationToken);
    Task<TelegramChat[]> FindChatsAsync(string token, CancellationToken cancellationToken);
}
public sealed class TelegramDeliveryException(string message, int retryAfter = 0) : Exception(message)
{
    public int RetryAfter { get; } = retryAfter;
}

public sealed class TelegramApiClient(HttpClient? client = null) : ITelegramTransport
{
    private static readonly HttpClient SharedClient = new(new HttpClientHandler { AllowAutoRedirect = false }) { Timeout = TimeSpan.FromSeconds(25) };
    private readonly HttpClient _client = client ?? SharedClient;

    public static void ValidateToken(string token)
    {
        if (!Regex.IsMatch(token, @"\A[0-9]{5,20}:[A-Za-z0-9_-]{20,200}\z", RegexOptions.None, TimeSpan.FromSeconds(1)))
            throw new InvalidOperationException("Enter the bot token supplied by @BotFather.");
    }

    public async Task SendAsync(string token, string chatId, string text, bool silent, CancellationToken cancellationToken)
    {
        var id = TelegramOptions.ValidateChat(chatId);
        if (string.IsNullOrWhiteSpace(text)) throw new InvalidOperationException("A notification cannot be empty.");
        // Plain text avoids interpreting player-supplied names as Telegram markup.
        using var result = await CallAsync(token, "sendMessage", new
        {
            chat_id = id, text = LimitText(text), disable_notification = silent,
            link_preview_options = new { is_disabled = true }, allow_paid_broadcast = false
        }, cancellationToken);
        if (!result.RootElement.TryGetProperty("result", out var message) || !message.TryGetProperty("message_id", out _))
            throw new TelegramDeliveryException("Telegram did not confirm delivery. Check the chat before retrying.");
    }

    public static string LimitText(string text)
    {
        if (text.Length <= 4000) return text;
        var end = char.IsHighSurrogate(text[3998]) ? 3998 : 3999;
        return text[..end] + "…";
    }

    public async Task<TelegramChat[]> FindChatsAsync(string token, CancellationToken cancellationToken)
    {
        // No offset: discovering a destination must not acknowledge another bot consumer's updates.
        using var result = await CallAsync(token, "getUpdates", new { timeout = 0, limit = 100 }, cancellationToken);
        var chats = new Dictionary<long, TelegramChat>();
        foreach (var update in result.RootElement.GetProperty("result").EnumerateArray())
        {
            JsonElement message;
            if (!update.TryGetProperty("message", out message) && !update.TryGetProperty("channel_post", out message)) continue;
            if (!message.TryGetProperty("chat", out var chat) || !chat.TryGetProperty("id", out var idValue) || !idValue.TryGetInt64(out var id)) continue;
            var name = chat.TryGetProperty("title", out var title) ? title.GetString()
                : chat.TryGetProperty("first_name", out var first) ? first.GetString() : "Telegram chat";
            chats[id] = new(id.ToString(System.Globalization.CultureInfo.InvariantCulture), name ?? "Telegram chat");
        }
        return [.. chats.Values.OrderBy(x => x.Name)];
    }

    private async Task<JsonDocument> CallAsync(string token, string method, object body, CancellationToken cancellationToken)
    {
        token = token.Trim();
        ValidateToken(token);
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.telegram.org/bot{token}/{method}")
            { Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json") };
            using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var buffer = new MemoryStream();
            var bytes = new byte[8192];
            int count;
            while ((count = await stream.ReadAsync(bytes, cancellationToken)) > 0)
            {
                if (buffer.Length + count > 2_000_000) throw new TelegramDeliveryException("Telegram returned an oversized response.");
                buffer.Write(bytes, 0, count);
            }
            var document = JsonDocument.Parse(buffer.ToArray());
            var root = document.RootElement;
            if (response.IsSuccessStatusCode && root.TryGetProperty("ok", out var ok) && ok.ValueKind == JsonValueKind.True) return document;
            var code = root.TryGetProperty("error_code", out var error) && error.TryGetInt32(out var errorCode) ? errorCode : (int)response.StatusCode;
            var retryAfter = 0;
            if (root.TryGetProperty("parameters", out var parameters) && parameters.TryGetProperty("retry_after", out var retry)) retry.TryGetInt32(out retryAfter);
            document.Dispose();
            throw new TelegramDeliveryException(code switch
            {
                401 or 404 => "Telegram rejected the bot token. Check the token from @BotFather.",
                403 => "The bot cannot message this chat. Start or unblock the bot and check its group permissions.",
                400 => "Telegram rejected the destination or message. Check the chat ID; groups may receive a new ID after migration.",
                409 => "This bot has a webhook or another updates consumer. Enter the chat ID manually or use a dedicated bot.",
                429 => "Telegram rate limit reached. Delivery will wait when the retry delay is within five minutes.",
                _ => "Telegram is unavailable. The message was not confirmed; check the chat before retrying."
            }, code == 429 ? Math.Max(1, retryAfter) : 0);
        }
        // Never propagate HTTP exceptions: their URL contains the secret bot token.
        catch (HttpRequestException) { throw new TelegramDeliveryException("Cannot reach Telegram. Delivery is unconfirmed; check your connection and chat."); }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        { throw new TelegramDeliveryException("Telegram timed out. Delivery is unconfirmed; check the chat before retrying."); }
        catch (JsonException) { throw new TelegramDeliveryException("Telegram returned an unreadable response. Delivery is unconfirmed."); }
    }
}
