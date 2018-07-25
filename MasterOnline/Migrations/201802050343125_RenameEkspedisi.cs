namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameEkspedisi : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Ekspedisis", newName: "Ekspedisi");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.Ekspedisi", newName: "Ekspedisis");
        }
    }
}
