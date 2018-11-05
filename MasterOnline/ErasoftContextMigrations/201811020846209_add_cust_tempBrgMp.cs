namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_cust_tempBrgMp : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TEMP_BRG_MP", "CUST", c => c.String(maxLength: 10));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TEMP_BRG_MP", "CUST");
        }
    }
}
