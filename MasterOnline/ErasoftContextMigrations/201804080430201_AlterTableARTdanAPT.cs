namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterTableARTdanAPT : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.APT03A", "RecNum", c => c.Int(nullable: false, identity: true));
            AddColumn("dbo.ART03A", "RecNum", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.APT03B", "NO", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.ART03B", "NO", c => c.Int(nullable: false, identity: true));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ART03B", "NO", c => c.Int(nullable: false));
            AlterColumn("dbo.APT03B", "NO", c => c.Int(nullable: false));
            DropColumn("dbo.ART03A", "RecNum");
            DropColumn("dbo.APT03A", "RecNum");
        }
    }
}
