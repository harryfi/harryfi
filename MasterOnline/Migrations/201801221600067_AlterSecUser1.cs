namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterSecUser1 : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.SecUser");
            AlterColumn("dbo.SecUser", "UserId", c => c.Long(nullable: false, identity: true));
            AddPrimaryKey("dbo.SecUser", "UserId");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.SecUser");
            AlterColumn("dbo.SecUser", "UserId", c => c.String(nullable: false, maxLength: 20));
            AddPrimaryKey("dbo.SecUser", "UserId");
        }
    }
}
