namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterSecUser3 : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.SecUser");
            AddColumn("dbo.SecUser", "RecNum", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.SecUser", "UserId", c => c.Long(nullable: false));
            AddPrimaryKey("dbo.SecUser", "UserId");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.SecUser");
            AlterColumn("dbo.SecUser", "UserId", c => c.Long(nullable: false, identity: true));
            DropColumn("dbo.SecUser", "RecNum");
            AddPrimaryKey("dbo.SecUser", "UserId");
        }
    }
}
