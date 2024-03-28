using FluentMigrator;

namespace MainCore.Migrations
{
    [Migration(202403232304)]
    public class AddMaximumVillageToAccountInfoTable : Migration
    {
        public override void Up()
        {
            Alter.Table("AccountsInfo")
                .AddColumn("MaximumVillage").AsInt32().WithDefaultValue(0);
        }

        public override void Down()
        {
            Execute.Sql("ALTER TABLE 'AccountsInfo' DROP COLUMN 'MaximumVillage';");
        }
    }
}