namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddStatusSetuju : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Partner", "StatusSetuju", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Partner", "StatusSetuju");
        }
    }
}
