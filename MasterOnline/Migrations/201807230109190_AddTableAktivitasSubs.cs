namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTableAktivitasSubs : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AktivitasSubscription",
                c => new
                    {
                        RecNum = c.Int(nullable: false, identity: true),
                        Account = c.String(maxLength: 30),
                        Email = c.String(maxLength: 50),
                        TipeSubs = c.String(maxLength: 2),
                        TanggalBayar = c.DateTime(storeType: "date"),
                        Nilai = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.RecNum);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.AktivitasSubscription");
        }
    }
}
