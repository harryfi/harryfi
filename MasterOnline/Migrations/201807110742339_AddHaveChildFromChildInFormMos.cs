namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddHaveChildFromChildInFormMos : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FormMos", "HaveChildFromChild", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FormMos", "HaveChildFromChild");
        }
    }
}
