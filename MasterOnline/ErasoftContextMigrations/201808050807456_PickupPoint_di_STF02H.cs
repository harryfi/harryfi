namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PickupPoint_di_STF02H : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PICKUP_POINT_BLIBLI",
                c => new
                    {
                        Recnum = c.Int(nullable: false, identity: true),
                        KODE = c.String(maxLength: 10),
                        KETERANGAN = c.String(maxLength: 250),
                        MERCHANT_CODE = c.String(maxLength: 30),
                    })
                .PrimaryKey(t => t.Recnum);
            
            AddColumn("dbo.STF02H", "PICKUP_POINT", c => c.String(maxLength: 30));
        }
        
        public override void Down()
        {
            DropColumn("dbo.STF02H", "PICKUP_POINT");
            DropTable("dbo.PICKUP_POINT_BLIBLI");
        }
    }
}
