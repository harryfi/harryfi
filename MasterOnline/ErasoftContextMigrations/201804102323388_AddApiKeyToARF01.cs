namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddApiKeyToARF01 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ARF01", "API_KEY", c => c.String(maxLength: 100));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ARF01", "API_KEY");
        }
    }
}
