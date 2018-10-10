namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class namapemesan_sit01a_20_to_30 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.SIT01A", "NAMAPEMESAN", c => c.String(maxLength: 30, unicode: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SIT01A", "NAMAPEMESAN", c => c.String(maxLength: 20, unicode: false));
        }
    }
}
