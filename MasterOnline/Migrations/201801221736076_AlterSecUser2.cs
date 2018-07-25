namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterSecUser2 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.SecUser", "AccountId", c => c.Long(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SecUser", "AccountId", c => c.String(nullable: false, maxLength: 50));
        }
    }
}
