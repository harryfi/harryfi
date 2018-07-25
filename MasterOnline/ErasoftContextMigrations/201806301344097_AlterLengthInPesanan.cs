namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterLengthInPesanan : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.SOT01A", "PEMESAN", c => c.String(maxLength: 10, unicode: false));
            AlterColumn("dbo.SOT01A", "NAMAPEMESAN", c => c.String(nullable: false, maxLength: 30, unicode: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SOT01A", "NAMAPEMESAN", c => c.String(nullable: false, maxLength: 20, unicode: false));
            AlterColumn("dbo.SOT01A", "PEMESAN", c => c.String(maxLength: 30, unicode: false));
        }
    }
}
