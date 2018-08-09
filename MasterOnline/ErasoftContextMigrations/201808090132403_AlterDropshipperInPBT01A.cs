namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterDropshipperInPBT01A : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.PBT01A", "DROPSHIPPER", c => c.Boolean());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.PBT01A", "DROPSHIPPER", c => c.Boolean(nullable: false));
        }
    }
}
