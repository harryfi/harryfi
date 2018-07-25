namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterAccountAddFotoKtp : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Account", "PhotoKtpUrl", c => c.String(nullable: false, maxLength: 255));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Account", "PhotoKtpUrl");
        }
    }
}
