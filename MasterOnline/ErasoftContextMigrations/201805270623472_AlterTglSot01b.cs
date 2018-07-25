namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterTglSot01b : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.SOT01B", "TGL_INPUT", c => c.DateTime(storeType: "date"));
            AlterColumn("dbo.SOT01B", "TGL_KIRIM", c => c.DateTime(storeType: "date"));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SOT01B", "TGL_KIRIM", c => c.String());
            AlterColumn("dbo.SOT01B", "TGL_INPUT", c => c.String());
        }
    }
}
