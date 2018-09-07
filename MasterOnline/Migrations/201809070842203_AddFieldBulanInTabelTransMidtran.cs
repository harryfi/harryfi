namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFieldBulanInTabelTransMidtran : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TABEL_TRANSAKSI_MIDTRANS", "BULAN", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TABEL_TRANSAKSI_MIDTRANS", "BULAN");
        }
    }
}
