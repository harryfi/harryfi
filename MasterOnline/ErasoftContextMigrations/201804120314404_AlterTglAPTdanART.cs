namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterTglAPTdanART : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.APT01A", "TGL", c => c.DateTime(storeType: "smalldatetime"));
            AlterColumn("dbo.APT01A", "JTGL", c => c.DateTime(storeType: "smalldatetime"));
            AlterColumn("dbo.APT01A", "TGL_PAJAK", c => c.DateTime(storeType: "smalldatetime"));
            AlterColumn("dbo.APT01A", "TGL_INV_2", c => c.DateTime(storeType: "smalldatetime"));
            AlterColumn("dbo.APT01A", "TGLINPUT", c => c.DateTime(storeType: "smalldatetime"));
            AlterColumn("dbo.ART01A", "TGL", c => c.DateTime(storeType: "smalldatetime"));
            AlterColumn("dbo.ART01A", "TGL_PAJAK", c => c.DateTime(storeType: "smalldatetime"));
            AlterColumn("dbo.ART01A", "JTGL", c => c.DateTime(storeType: "smalldatetime"));
            AlterColumn("dbo.ART01A", "TGLINPUT", c => c.DateTime(storeType: "smalldatetime"));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ART01A", "TGLINPUT", c => c.String(maxLength: 10));
            AlterColumn("dbo.ART01A", "JTGL", c => c.String(maxLength: 10));
            AlterColumn("dbo.ART01A", "TGL_PAJAK", c => c.String(maxLength: 10));
            AlterColumn("dbo.ART01A", "TGL", c => c.String(maxLength: 10));
            AlterColumn("dbo.APT01A", "TGLINPUT", c => c.String(maxLength: 10));
            AlterColumn("dbo.APT01A", "TGL_INV_2", c => c.String(maxLength: 10));
            AlterColumn("dbo.APT01A", "TGL_PAJAK", c => c.String(maxLength: 10));
            AlterColumn("dbo.APT01A", "JTGL", c => c.String(maxLength: 10));
            AlterColumn("dbo.APT01A", "TGL", c => c.String(maxLength: 10));
        }
    }
}
