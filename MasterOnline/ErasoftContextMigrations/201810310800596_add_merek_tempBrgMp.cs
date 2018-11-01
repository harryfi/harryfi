namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_merek_tempBrgMp : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TEMP_BRG_MP", "MEREK", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TEMP_BRG_MP", "MEREK");
        }
    }
}
