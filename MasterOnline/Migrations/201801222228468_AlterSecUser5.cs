namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterSecUser5 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SecUser", "HasParent", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SecUser", "HasParent");
        }
    }
}
