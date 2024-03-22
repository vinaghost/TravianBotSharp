using FluentMigrator;

namespace MainCore.Migrations
{
    [Migration(202422032353)]
    public class AddSettlerToVillageTabel : Migration
    {
        public override void Up()
        {
            Alter.Table("Villages")
                .AddColumn("Settlers").AsInt32().WithDefaultValue(0);
        }

        public override void Down()
        {
            Execute.Sql("ALTER TABLE 'Villages' DROP COLUMN 'Settlers';");
        }
    }
}