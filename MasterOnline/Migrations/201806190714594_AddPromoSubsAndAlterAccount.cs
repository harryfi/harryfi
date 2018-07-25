namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPromoSubsAndAlterAccount : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Promo",
                c => new
                    {
                        EMAIL_REF = c.String(nullable: false, maxLength: 50),
                        HP_REF = c.String(maxLength: 20),
                        EMAIL = c.String(maxLength: 50),
                        HP = c.String(maxLength: 20),
                        TGL = c.DateTime(storeType: "date"),
                        TGL_REPLY = c.DateTime(storeType: "date"),
                        PAID = c.Boolean(nullable: false),
                        RecNum = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.EMAIL_REF);
            
            CreateTable(
                "dbo.Subscription",
                c => new
                    {
                        KODE = c.String(nullable: false, maxLength: 2),
                        KETERANGAN = c.String(maxLength: 30),
                        JUMLAH_MP = c.Short(nullable: false),
                        JUMLAH_PESANAN = c.Int(nullable: false),
                        HARGA = c.Double(nullable: false),
                        RecNum = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.KODE);
            
            AddColumn("dbo.Account", "KODE_SUBSCRIPTION", c => c.String(maxLength: 2));
            AddColumn("dbo.Account", "TGL_SUBSCRIPTION", c => c.DateTime(storeType: "date"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Account", "TGL_SUBSCRIPTION");
            DropColumn("dbo.Account", "KODE_SUBSCRIPTION");
            DropTable("dbo.Subscription");
            DropTable("dbo.Promo");
        }
    }
}
