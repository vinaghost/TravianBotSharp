using FluentMigrator;
using System.Data;

namespace MainCore.Migrations
{
    [Migration(202403252026)]
    public class AddNewVillagesTable : Migration
    {
        public override void Up()
        {
            Create.Table("NewVillages")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("AccountId").AsInt32().ForeignKey("Accounts", "Id").OnDelete(Rule.Cascade)
                .WithColumn("X").AsInt32()
                .WithColumn("Y").AsInt32();
        }

        public override void Down()
        {
            Delete.Table("NewVillages");
        }
    }
}