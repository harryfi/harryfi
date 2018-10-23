namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeColumnNameReferral : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Account", "KODE_REFERRAL", c => c.String(maxLength: 100));
            DropColumn("dbo.Account", "ReferralCode");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Account", "ReferralCode", c => c.String(maxLength: 100));
            DropColumn("dbo.Account", "KODE_REFERRAL");
        }
    }
}
