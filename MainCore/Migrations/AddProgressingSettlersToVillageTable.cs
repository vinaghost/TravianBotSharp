using FluentMigrator;

namespace MainCore.Migrations
{
    [Migration(202403272006)]
    public class AddProgressingSettlersToVillageTable : Migration
    {
        public override void Up()
        {
            Alter.Table("Villages")
                .AddColumn("ProgressingSettlers").AsInt32().WithDefaultValue(0);
        }

        public override void Down()
        {
            Execute.Sql("ALTER TABLE 'Villages' DROP COLUMN 'ProgressingSettlers';");
        }
    }
}