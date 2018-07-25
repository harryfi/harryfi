namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterTableAccountColumnStatus : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Account", "Status", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Account", "Status", c => c.Int(nullable: false));
        }
    }
}
