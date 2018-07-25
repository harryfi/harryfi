namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTableEkspedisi : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Ekspedisis",
                c => new
                    {
                        RecNum = c.Int(nullable: false, identity: true),
                        NamaEkspedisi = c.String(nullable: false, maxLength: 25),
                        Status = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.RecNum);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Ekspedisis");
        }
    }
}
