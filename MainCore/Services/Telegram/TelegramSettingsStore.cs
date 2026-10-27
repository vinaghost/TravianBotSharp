using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MainCore.Services.Telegram;

public interface ITelegramSettingsStore
{
    TelegramOptions Load(AccountId accountId);
    string ReadToken(AccountId accountId, TelegramOptions options);
    TelegramOptions Save(AccountId accountId, TelegramOptions options, string token);
    string Describe(AccountId accountId, VillageId? villageId);
}

[RegisterSingleton<ITelegramSettingsStore>]
public sealed class TelegramSettingsStore(IDbContextFactory<AppDbContext> factory) : ITelegramSettingsStore
{
    private readonly object _gate = new();
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static string DirectoryPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TravianBotSharp", "Telegram");

    private string Identity(AccountId accountId)
    {
        using var db = factory.CreateDbContext();
        var account = db.Accounts.AsNoTracking().SingleOrDefault(x => x.Id == accountId.Value)
            ?? throw new InvalidOperationException("The selected account no longer exists.");
        var identity = $"{Path.GetFullPath(db.Database.GetDbConnection().DataSource)}\n{account.Id}\n{account.Server}\n{account.Username}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(identity)));
    }

    private static string Target(string identity, string revision) => $"TravianBotSharp/Telegram/{identity}/{revision}";
    private static TelegramOptions Read(string identity)
    {
        var path = Path.Combine(DirectoryPath, identity + ".json");
        if (!File.Exists(path)) return new();
        try
        {
            var options = JsonSerializer.Deserialize<TelegramOptions>(File.ReadAllText(path), JsonOptions) ?? new();
            options.Validate();
            return options;
        }
        catch (JsonException) { throw new InvalidOperationException("Telegram settings are unreadable. Disable and save them again."); }
    }
    public TelegramOptions Load(AccountId accountId) { lock (_gate) return Read(Identity(accountId)); }
    public string ReadToken(AccountId accountId, TelegramOptions options)
    {
        lock (_gate) return string.IsNullOrEmpty(options.Revision) ? "" : TelegramCredentials.Read(Target(Identity(accountId), options.Revision));
    }
    public TelegramOptions Save(AccountId accountId, TelegramOptions options, string token)
    {
        options.Validate();
        token = token.Trim();
        if (options.Enabled || token.Length > 0) TelegramApiClient.ValidateToken(token);
        lock (_gate)
        {
            var identity = Identity(accountId);
            TelegramOptions previous;
            try { previous = Read(identity); } catch (InvalidOperationException) { previous = new(); }
            var saved = options with { Revision = Guid.NewGuid().ToString("N"), ChatId = options.ChatId.Trim() };
            var target = Target(identity, saved.Revision);
            Directory.CreateDirectory(DirectoryPath);
            var path = Path.Combine(DirectoryPath, identity + ".json");
            var temp = path + ".tmp";
            TelegramCredentials.Write(target, token);
            try
            {
                File.WriteAllText(temp, JsonSerializer.Serialize(saved, JsonOptions));
                File.Move(temp, path, true);
            }
            catch
            {
                TelegramCredentials.Delete(target);
                throw;
            }
            // Saving the new revision is atomic; queued deliveries from previous revisions are discarded.
            if (!string.IsNullOrEmpty(previous.Revision)) TelegramCredentials.Delete(Target(identity, previous.Revision));
            return saved;
        }
    }
    public string Describe(AccountId accountId, VillageId? villageId)
    {
        using var db = factory.CreateDbContext();
        var account = db.Accounts.AsNoTracking().SingleOrDefault(x => x.Id == accountId.Value)
            ?? throw new InvalidOperationException("The selected account no longer exists.");
        var host = Uri.TryCreate(account.Server, UriKind.Absolute, out var uri) ? uri.Host : "Travian";
        var label = $"{account.Username} · {host}";
        if (villageId is { } id)
        {
            var village = db.Villages.AsNoTracking().SingleOrDefault(x => x.Id == id.Value && x.AccountId == accountId.Value)
                ?? throw new InvalidOperationException("The selected village no longer belongs to this account.");
            label += $"\nVillage: {village.Name}";
        }
        return label.Length > 400 ? label[..400] : label;
    }
}

// Bot tokens never enter settings files, exported account templates, HTTP diagnostics, or notifications.
internal static class TelegramCredentials
{
    public static string Read(string target)
    {
        if (!CredRead(target, 1, 0, out var pointer))
        {
            if (Marshal.GetLastWin32Error() == 1168) return "";
            throw new InvalidOperationException("Windows Credential Manager could not read the Telegram token.");
        }
        try
        {
            var credential = Marshal.PtrToStructure<Credential>(pointer);
            return Marshal.PtrToStringUni(credential.Blob, (int)credential.BlobSize / 2) ?? "";
        }
        finally { CredFree(pointer); }
    }
    public static void Write(string target, string token)
    {
        if (token.Length == 0) return;
        var pointer = Marshal.StringToCoTaskMemUni(token);
        try
        {
            var credential = new Credential { Type = 1, TargetName = target, BlobSize = (uint)Encoding.Unicode.GetByteCount(token), Blob = pointer, Persist = 2, UserName = "TravianBotSharp" };
            if (!CredWrite(ref credential, 0)) throw new InvalidOperationException("Windows Credential Manager could not save the Telegram token.");
        }
        finally { Marshal.ZeroFreeCoTaskMemUnicode(pointer); }
    }
    public static void Delete(string target)
    {
        if (!CredDelete(target, 1, 0) && Marshal.GetLastWin32Error() != 1168)
            throw new InvalidOperationException("Windows could not remove an old Telegram token. Remove it from Windows Credential Manager.");
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct Credential
    {
        public uint Flags, Type;
        public string TargetName;
        public string? Comment;
        public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
        public uint BlobSize;
        public IntPtr Blob;
        public uint Persist, AttributeCount;
        public IntPtr Attributes;
        public string? TargetAlias;
        public string UserName;
    }
    [DllImport("advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)] private static extern bool CredRead(string target, uint type, uint flags, out IntPtr credential);
    [DllImport("advapi32.dll", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)] private static extern bool CredWrite(ref Credential credential, uint flags);
    [DllImport("advapi32.dll", EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)] private static extern bool CredDelete(string target, uint type, uint flags);
    [DllImport("advapi32.dll")] private static extern void CredFree(IntPtr pointer);
}
