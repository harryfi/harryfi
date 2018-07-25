namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterGLFTRAN12 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GLFTRAN1", "RecNum", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.GLFTRAN2", "no", c => c.Int(nullable: false, identity: true));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.GLFTRAN2", "no", c => c.Int(nullable: false));
            DropColumn("dbo.GLFTRAN1", "RecNum");
        }
    }
}
