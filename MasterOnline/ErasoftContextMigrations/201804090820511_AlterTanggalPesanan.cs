namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterTanggalPesanan : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.SOT01A", "TGL", c => c.DateTime(nullable: false));
            AlterColumn("dbo.SOT01A", "TGL_KIRIM", c => c.DateTime());
            AlterColumn("dbo.SOT01A", "TGL_INPUT", c => c.DateTime());
            AlterColumn("dbo.SOT01A", "TGL_JTH_TEMPO", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SOT01A", "TGL_JTH_TEMPO", c => c.String(nullable: false));
            AlterColumn("dbo.SOT01A", "TGL_INPUT", c => c.String());
            AlterColumn("dbo.SOT01A", "TGL_KIRIM", c => c.String());
            AlterColumn("dbo.SOT01A", "TGL", c => c.String(nullable: false));
        }
    }
}
