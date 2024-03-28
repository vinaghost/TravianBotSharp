using FluentMigrator;
using System.Data;

namespace MainCore.Migrations
{
    [Migration(202403221509)]
    public class AddExpansionSlotTable : Migration
    {
        public override void Up()
        {
            Create.Table("ExpansionSlots")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("VillageId").AsInt32().ForeignKey("Villages", "Id").OnDelete(Rule.Cascade)
                .WithColumn("Status").AsInt32()
                .WithColumn("Content").AsString();
        }

        public override void Down()
        {
            Delete.Table("ExpansionSlots");
        }
    }
}