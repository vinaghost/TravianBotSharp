using FluentMigrator;

namespace MainCore.Infrasturecture.Migrations
{
    [Migration(202506261500)]
    public class HeroFarmTargets : Migration
    {
        public override void Up()
        {
            Create.Table("HeroFarmTargets")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("X").AsInt32().NotNullable()
                .WithColumn("Y").AsInt32().NotNullable()
                .WithColumn("OasisType").AsString().NotNullable().WithDefaultValue("Unknown")
                .WithColumn("Animal").AsString().NotNullable().WithDefaultValue("Unknown")
                .WithColumn("Resource").AsInt32().NotNullable()
                .WithColumn("LastSend").AsDateTime().NotNullable().WithDefaultValue("0001-01-01T00:00:00")
                .WithColumn("AccountId").AsInt32().NotNullable().ForeignKey("Accounts", "Id");
        }

        public override void Down()
        {
            Delete.Table("HeroFarmTargets");
        }
    }
}