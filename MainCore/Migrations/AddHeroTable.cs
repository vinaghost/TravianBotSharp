using FluentMigrator;
using System.Data;

namespace MainCore.Migrations
{
    [Migration(202303071943)]
    public class AddHeroTable : Migration
    {
        public override void Up()
        {
            Create.Table("Heroes")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("AccountId").AsInt32().ForeignKey("Accounts", "Id").OnDelete(Rule.Cascade)
                .WithColumn("Health").AsInt32()
                .WithColumn("Status").AsInt32();
        }

        public override void Down()
        {
            Delete.Table("Heroes");
        }
    }
}