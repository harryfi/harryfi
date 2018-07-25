namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddColumnShowToFormMos : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FormMos", "Show", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FormMos", "Show");
        }
    }
}
