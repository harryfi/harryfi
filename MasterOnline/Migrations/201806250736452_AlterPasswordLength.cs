namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterPasswordLength : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Account", "Password", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Account", "Password", c => c.String(nullable: false, maxLength: 20));
        }
    }
}
