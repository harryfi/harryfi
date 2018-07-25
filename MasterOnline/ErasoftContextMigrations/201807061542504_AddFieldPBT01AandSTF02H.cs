namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFieldPBT01AandSTF02H : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PBT01A", "KODE_REF_PESANAN", c => c.String(maxLength: 15));
            AddColumn("dbo.PBT01A", "DROPSHIPPER", c => c.Boolean(nullable: false));
            //AddColumn("dbo.STF02H", "DeliveryTempElevenia", c => c.String(maxLength: 10));
        }
        
        public override void Down()
        {
            //DropColumn("dbo.STF02H", "DeliveryTempElevenia");
            DropColumn("dbo.PBT01A", "DROPSHIPPER");
            DropColumn("dbo.PBT01A", "KODE_REF_PESANAN");
        }
    }
}
