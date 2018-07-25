namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterSecUser4 : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.SecUser");
            AddPrimaryKey("dbo.SecUser", "RecNum");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.SecUser");
            AddPrimaryKey("dbo.SecUser", "UserId");
        }
    }
}
