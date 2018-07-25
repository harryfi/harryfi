namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterTglSTT01AdanB : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.STT01A", "Tgl", c => c.DateTime());
            AlterColumn("dbo.STT01A", "TglInput", c => c.DateTime());
            AlterColumn("dbo.STT01A", "TGL_TERIMA_CUST", c => c.DateTime());
            AlterColumn("dbo.STT01A", "TGL_RETUR", c => c.DateTime());
            AlterColumn("dbo.STT01A", "TGL_STPD", c => c.DateTime());
            AlterColumn("dbo.STT01B", "TglInput", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.STT01B", "TglInput", c => c.String());
            AlterColumn("dbo.STT01A", "TGL_STPD", c => c.String());
            AlterColumn("dbo.STT01A", "TGL_RETUR", c => c.String());
            AlterColumn("dbo.STT01A", "TGL_TERIMA_CUST", c => c.String());
            AlterColumn("dbo.STT01A", "TglInput", c => c.String());
            AlterColumn("dbo.STT01A", "Tgl", c => c.String());
        }
    }
}
