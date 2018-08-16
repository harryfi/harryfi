namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSomething : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Account", "TOKEN_CC");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Account", "TOKEN_CC", c => c.String(maxLength: 100));
        }
    }
}
