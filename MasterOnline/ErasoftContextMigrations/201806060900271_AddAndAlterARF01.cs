namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAndAlterARF01 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ARF01", "REFRESH_TOKEN", c => c.String(maxLength: 50));
            AlterColumn("dbo.ARF01", "AL", c => c.String(nullable: false, unicode: false, storeType: "text"));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ARF01", "AL", c => c.String(nullable: false, maxLength: 30, unicode: false));
            DropColumn("dbo.ARF01", "REFRESH_TOKEN");
        }
    }
}
