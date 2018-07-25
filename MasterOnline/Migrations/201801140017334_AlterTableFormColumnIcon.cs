namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterTableFormColumnIcon : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FormMos", "Icon", c => c.String(maxLength: 20));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FormMos", "Icon");
        }
    }
}
