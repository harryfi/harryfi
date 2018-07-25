namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterLengthPemesan : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.SOT01A", "PEMESAN", c => c.String(maxLength: 30, unicode: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SOT01A", "PEMESAN", c => c.String(maxLength: 10, unicode: false));
        }
    }
}
