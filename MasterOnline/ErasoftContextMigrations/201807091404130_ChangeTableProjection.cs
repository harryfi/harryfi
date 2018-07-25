namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeTableProjection : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.DeliveryTemplateElevenias", newName: "DeliveryTemplateElevenia");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.DeliveryTemplateElevenia", newName: "DeliveryTemplateElevenias");
        }
    }
}
