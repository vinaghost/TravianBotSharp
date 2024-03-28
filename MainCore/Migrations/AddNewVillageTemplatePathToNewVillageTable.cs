using FluentMigrator;

namespace MainCore.Migrations
{
    [Migration(202403282055)]
    public class AddNewVillageTemplatePathToNewVillageTable : Migration
    {
        public override void Up()
        {
            Alter.Table("NewVillages")
                .AddColumn("NewVillageTemplatePath").AsString().WithDefaultValue("");
        }

        public override void Down()
        {
            Execute.Sql("ALTER TABLE 'NewVillages' DROP COLUMN 'NewVillageTemplatePath';");
        }
    }
}