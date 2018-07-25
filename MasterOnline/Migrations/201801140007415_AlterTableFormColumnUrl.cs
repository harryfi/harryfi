namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterTableFormColumnUrl : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.FormMos", "Url", c => c.String(maxLength: 150));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.FormMos", "Url", c => c.String(nullable: false, maxLength: 150));
        }
    }
}
