namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterTableFormColumnHaveChild : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FormMos", "HaveChild", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FormMos", "HaveChild");
        }
    }
}
