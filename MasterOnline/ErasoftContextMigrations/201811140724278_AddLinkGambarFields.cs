namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLinkGambarFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.STF02", "LINK_GAMBAR_1", c => c.String());
            AddColumn("dbo.STF02", "LINK_GAMBAR_2", c => c.String());
            AddColumn("dbo.STF02", "LINK_GAMBAR_3", c => c.String());
            AlterColumn("dbo.ARF01C", "EMAIL", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ARF01C", "EMAIL", c => c.String(nullable: false, maxLength: 50));
            DropColumn("dbo.STF02", "LINK_GAMBAR_3");
            DropColumn("dbo.STF02", "LINK_GAMBAR_2");
            DropColumn("dbo.STF02", "LINK_GAMBAR_1");
        }
    }
}
