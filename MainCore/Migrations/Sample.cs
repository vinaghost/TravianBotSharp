using FluentMigrator;

namespace MainCore.Migrations
{
    //[Migration(202408201529)]
    public class Sample : Migration
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