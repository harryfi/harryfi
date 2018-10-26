namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCustomReferalCode : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Partner", "KodeRefPilihan", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Partner", "KodeRefPilihan");
        }
    }
}
