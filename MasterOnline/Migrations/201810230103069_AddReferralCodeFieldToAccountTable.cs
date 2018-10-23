namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddReferralCodeFieldToAccountTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Account", "ReferralCode", c => c.String(maxLength: 100));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Account", "ReferralCode");
        }
    }
}
