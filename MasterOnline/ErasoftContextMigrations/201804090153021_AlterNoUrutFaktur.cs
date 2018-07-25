namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterNoUrutFaktur : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.SIT01B", "NO_URUT", c => c.Int(nullable: false, identity: true));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SIT01B", "NO_URUT", c => c.Int(nullable: false));
        }
    }
}
