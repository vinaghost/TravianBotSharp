using System.Security.Cryptography;
using System.Text;
using MainCore.Entities;
using MainCore.Infrasturecture.Persistence;
using MainCore.Services.Telegram;
using Microsoft.EntityFrameworkCore;

namespace MainCore.Test.Telegram;

public class TelegramSettingsTests
{
    [Fact]
    public void SavesPreferencesSeparatelyFromCredentialsAndForgetsTheOldToken()
    {
        // Unique test identity; no existing account settings or credentials are read or changed.
        var id = Guid.NewGuid().ToString("N");
        var database = Path.Combine(Path.GetTempPath(), $"tbs-telegram-{id}.db");
        var username = "telegram-test-" + id;
        const string server = "https://example.invalid";
        const string token = "123456:abcdefghijklmnopqrstuvwxyz";
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite($"Data Source={database};Pooling=False").Options;
        var factory = Substitute.For<IDbContextFactory<AppDbContext>>();
        factory.CreateDbContext().Returns(_ => new AppDbContext(options));
        var identity = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{Path.GetFullPath(database)}\n1\n{server}\n{username}")));
        var preferences = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TravianBotSharp", "Telegram", identity + ".json");
        var store = new TelegramSettingsStore(factory);
        var created = false;
        try
        {
            using (var db = factory.CreateDbContext())
            {
                db.Database.EnsureCreated();
                db.Accounts.Add(new Account { Id = 1, Username = username, Server = server });
                db.Accounts.Add(new Account { Id = 2, Username = "other-" + username, Server = server });
                db.SaveChanges();
            }
            created = true;
            var saved = store.Save(new(1), new() { Enabled = true, ChatId = "42", Events = TelegramEvents.IncomingAttack | TelegramEvents.AiPlanApplied }, token);
            store.Load(new(1)).ShouldBe(saved);
            store.ReadToken(new(1), saved).ShouldBe(token);
            File.ReadAllText(preferences).ShouldNotContain(token);
            store.Load(new(2)).Enabled.ShouldBeFalse();
            var disabled = store.Save(new(1), saved with { Enabled = false }, "");
            store.ReadToken(new(1), disabled).ShouldBeEmpty();
            store.ReadToken(new(1), saved).ShouldBeEmpty();
            disabled.Revision.ShouldNotBe(saved.Revision);
        }
        finally
        {
            try { if (created) store.Save(new(1), new(), ""); }
            finally
            {
                File.Delete(preferences);
                File.Delete(database);
                File.Delete(database + "-wal");
                File.Delete(database + "-shm");
            }
        }
    }
}
