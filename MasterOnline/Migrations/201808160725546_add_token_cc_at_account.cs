namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_token_cc_at_account : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Account", "TOKEN_CC", c => c.String(maxLength: 100));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Account", "TOKEN_CC");
        }
    }
}
