using FluentMigrator;
using System.Data;

namespace MainCore.Migrations
{
    [Migration(202403151103)]
    public class AddTroopTable : Migration
    {
        public override void Up()
        {
            Create.Table("Troops")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("VillageId").AsInt32().ForeignKey("Villages", "Id").OnDelete(Rule.Cascade)
                .WithColumn("Type").AsInt32()
                .WithColumn("Level").AsInt32().WithDefaultValue(-1);
        }

        public override void Down()
        {
            Delete.Table("Troops");
        }
    }
}