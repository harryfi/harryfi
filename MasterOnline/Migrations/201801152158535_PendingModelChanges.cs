namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PendingModelChanges : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Marketplace", "Status", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Marketplace", "Status");
        }
    }
}
