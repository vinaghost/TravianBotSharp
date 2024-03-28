using FluentMigrator;

namespace MainCore.Migrations
{
    [Migration(202403252104)]
    public class AddNewVillageTemplatePathToAccountInfoTable : Migration
    {
        public override void Up()
        {
            Alter.Table("AccountsInfo")
                .AddColumn("NewVillageTemplatePath").AsString().WithDefaultValue("");
        }

        public override void Down()
        {
            Execute.Sql("ALTER TABLE 'AccountsInfo' DROP COLUMN 'NewVillageTemplatePath';");
        }
    }
}