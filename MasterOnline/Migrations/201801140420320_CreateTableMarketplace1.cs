namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateTableMarketplace1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Marketplace",
                c => new
                    {
                        IdMarket = c.Int(nullable: false, identity: true),
                        NamaMarket = c.String(nullable: false, maxLength: 50),
                        LokasiLogo = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.IdMarket, clustered: false);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Marketplace");
        }
    }
}
