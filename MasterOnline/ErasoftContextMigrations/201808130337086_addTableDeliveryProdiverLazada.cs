namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addTableDeliveryProdiverLazada : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DELIVERY_PROVIDER_LAZADA",
                c => new
                    {
                        CUST = c.String(nullable: false, maxLength: 10),
                        NAME = c.String(nullable: false, maxLength: 250),
                        RECNUM = c.Int(nullable: false),
                        COD = c.String(maxLength: 1),
                    })
                .PrimaryKey(t => new { t.CUST, t.NAME });
            
        }
        
        public override void Down()
        {
            DropTable("dbo.DELIVERY_PROVIDER_LAZADA");
        }
    }
}
