namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_urlImage_tempBrgMp : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TEMP_BRG_MP", "IMAGE", c => c.String());
            AddColumn("dbo.TEMP_BRG_MP", "IMAGE2", c => c.String());
            AddColumn("dbo.TEMP_BRG_MP", "IMAGE3", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TEMP_BRG_MP", "IMAGE3");
            DropColumn("dbo.TEMP_BRG_MP", "IMAGE2");
            DropColumn("dbo.TEMP_BRG_MP", "IMAGE");
        }
    }
}
