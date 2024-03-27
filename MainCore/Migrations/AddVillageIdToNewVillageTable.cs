using FluentMigrator;

namespace MainCore.Migrations
{
    [Migration(202403271931)]
    public class AddVillageIdToNewVillageTable : Migration
    {
        public override void Up()
        {
            Alter.Table("NewVillages")
                .AddColumn("VillageId").AsInt32().WithDefaultValue(0);
        }

        public override void Down()
        {
            Execute.Sql("ALTER TABLE 'NewVillages' DROP COLUMN 'VillageId';");
        }
    }
}