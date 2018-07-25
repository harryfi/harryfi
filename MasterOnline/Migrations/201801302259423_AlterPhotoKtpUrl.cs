namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterPhotoKtpUrl : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Account", "PhotoKtpUrl", c => c.String(maxLength: 255));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Account", "PhotoKtpUrl", c => c.String(nullable: false, maxLength: 255));
        }
    }
}
