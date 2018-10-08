namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addStatusAPI_ARF01 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ARF01", "STATUS_API", c => c.String(maxLength: 1));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ARF01", "STATUS_API");
        }
    }
}
