namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterSecUserParentId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SecUser", "ParentId", c => c.Int(nullable: false));
            DropColumn("dbo.SecUser", "HasParent");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SecUser", "HasParent", c => c.Boolean(nullable: false));
            DropColumn("dbo.SecUser", "ParentId");
        }
    }
}
