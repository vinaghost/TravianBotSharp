using FluentMigrator;
using System.Data;

namespace MainCore.Migrations
{
    [Migration(202303072023)]
    public class AddAdventureTable : Migration
    {
        public override void Up()
        {
            Create.Table("Adventures")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("AccountId").AsInt32().ForeignKey("Accounts", "Id").OnDelete(Rule.Cascade)
                .WithColumn("X").AsInt32()
                .WithColumn("Y").AsInt32()
                .WithColumn("Difficulty").AsInt32();
        }

        public override void Down()
        {
            Delete.Table("Adventures");
        }
    }
}