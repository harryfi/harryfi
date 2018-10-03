namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class max_length_Account_user_id_20_50 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Account", "UserId", c => c.String(nullable: false, maxLength: 50));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Account", "UserId", c => c.String(nullable: false, maxLength: 20));
        }
    }
}
