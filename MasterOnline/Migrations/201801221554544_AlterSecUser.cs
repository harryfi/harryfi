namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterSecUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SecUser", "FormId", c => c.Int(nullable: false));
            DropColumn("dbo.SecUser", "Form");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SecUser", "Form", c => c.String(nullable: false, maxLength: 50));
            DropColumn("dbo.SecUser", "FormId");
        }
    }
}
