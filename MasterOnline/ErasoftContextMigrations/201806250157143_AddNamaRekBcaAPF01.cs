namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNamaRekBcaAPF01 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.APF01", "NamaRekBca", c => c.String(maxLength: 30));
        }
        
        public override void Down()
        {
            DropColumn("dbo.APF01", "NamaRekBca");
        }
    }
}
