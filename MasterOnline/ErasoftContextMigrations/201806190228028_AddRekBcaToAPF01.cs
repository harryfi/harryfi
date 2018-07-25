namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRekBcaToAPF01 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.APF01", "RekBca", c => c.String(maxLength: 12));
        }
        
        public override void Down()
        {
            DropColumn("dbo.APF01", "RekBca");
        }
    }
}
