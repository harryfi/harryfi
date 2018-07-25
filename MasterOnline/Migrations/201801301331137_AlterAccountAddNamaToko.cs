namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterAccountAddNamaToko : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Account", "NamaTokoOnline", c => c.String(nullable: false, maxLength: 100));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Account", "NamaTokoOnline");
        }
    }
}
