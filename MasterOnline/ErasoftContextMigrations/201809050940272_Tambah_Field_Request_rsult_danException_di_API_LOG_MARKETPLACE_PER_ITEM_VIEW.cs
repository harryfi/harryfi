namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Tambah_Field_Request_rsult_danException_di_API_LOG_MARKETPLACE_PER_ITEM_VIEW : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.API_LOG_MARKETPLACE_PER_ITEM", "REQUEST_RESULT", c => c.String());
            AddColumn("dbo.API_LOG_MARKETPLACE_PER_ITEM", "REQUEST_EXCEPTION", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.API_LOG_MARKETPLACE_PER_ITEM", "REQUEST_EXCEPTION");
            DropColumn("dbo.API_LOG_MARKETPLACE_PER_ITEM", "REQUEST_RESULT");
        }
    }
}
