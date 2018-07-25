namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Alter25Mei2018 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.STF02H", "BRG_MP", c => c.String(maxLength: 50));
            AlterColumn("dbo.ARF01C", "AL", c => c.String(nullable: false, unicode: false, storeType: "text"));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ARF01C", "AL", c => c.String(nullable: false, maxLength: 30));
            DropColumn("dbo.STF02H", "BRG_MP");
        }
    }
}
