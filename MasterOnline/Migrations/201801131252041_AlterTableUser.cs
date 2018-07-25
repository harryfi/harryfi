namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterTableUser : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.User");
            AlterColumn("dbo.User", "UserId", c => c.Long(nullable: false, identity: true));
            AlterColumn("dbo.User", "AccountId", c => c.Long(nullable: false));
            AddPrimaryKey("dbo.User", "UserId");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.User");
            AlterColumn("dbo.User", "AccountId", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.User", "UserId", c => c.String(nullable: false, maxLength: 20));
            AddPrimaryKey("dbo.User", "UserId");
        }
    }
}
