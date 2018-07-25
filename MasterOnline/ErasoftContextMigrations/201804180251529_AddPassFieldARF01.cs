namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPassFieldARF01 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ARF01", "PASSWORD", c => c.String(maxLength: 20));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ARF01", "PASSWORD");
        }
    }
}
