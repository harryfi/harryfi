namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_nama3_tempBrgMp : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TEMP_BRG_MP", "NAMA3", c => c.String(maxLength: 30));
            AlterColumn("dbo.ARF01C", "EMAIL", c => c.String(nullable: false, maxLength: 50));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ARF01C", "EMAIL", c => c.String(maxLength: 50));
            DropColumn("dbo.TEMP_BRG_MP", "NAMA3");
        }
    }
}
