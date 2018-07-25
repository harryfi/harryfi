namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterSTF02H : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.STF02H", "DISPLAY", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.STF02H", "DISPLAY");
        }
    }
}
