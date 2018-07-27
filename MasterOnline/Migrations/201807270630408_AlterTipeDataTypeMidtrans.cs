namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterTipeDataTypeMidtrans : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.TABEL_TRANSAKSI_MIDTRANS", "TYPE", c => c.String(nullable: false, maxLength: 2));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.TABEL_TRANSAKSI_MIDTRANS", "TYPE", c => c.Int(nullable: false));
        }
    }
}
