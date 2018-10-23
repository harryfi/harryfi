namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTipePartner : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Partner", "TipePartner", c => c.Int(nullable: false));
            AddColumn("dbo.Partner", "NamaTipe", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Partner", "NamaTipe");
            DropColumn("dbo.Partner", "TipePartner");
        }
    }
}
