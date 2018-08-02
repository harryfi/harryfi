namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFieldPhotoKtpBase64 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Account", "PhotoKtpBase64", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Account", "PhotoKtpBase64");
        }
    }
}
