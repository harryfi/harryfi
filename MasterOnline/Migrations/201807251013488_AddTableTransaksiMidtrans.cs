namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTableTransaksiMidtrans : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TABEL_TRANSAKSI_MIDTRANS",
                c => new
                    {
                        NO_TRANSAKSI = c.String(nullable: false, maxLength: 50),
                        ACCOUNT_ID = c.Long(nullable: false),
                        VALUE = c.Double(nullable: false),
                        TYPE = c.Int(nullable: false),
                        TGL_INPUT = c.DateTime(nullable: false, storeType: "date"),
                        RECNUM = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.NO_TRANSAKSI);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.TABEL_TRANSAKSI_MIDTRANS");
        }
    }
}
