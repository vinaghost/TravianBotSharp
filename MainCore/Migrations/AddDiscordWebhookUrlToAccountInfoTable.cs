using FluentMigrator;

namespace MainCore.Migrations
{
    [Migration(202404101706)]
    public class AddDiscordWebhookUrlToAccountInfoTable : Migration
    {
        public override void Up()
        {
            Alter.Table("AccountsInfo")
                .AddColumn("DiscordWebhookUrl").AsString().WithDefaultValue("");
        }

        public override void Down()
        {
            Execute.Sql("ALTER TABLE 'AccountsInfo' DROP COLUMN 'DiscordWebhookUrl';");
        }
    }
}