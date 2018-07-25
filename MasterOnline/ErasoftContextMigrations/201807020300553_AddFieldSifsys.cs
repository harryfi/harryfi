namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFieldSifsys : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SIFSYS", "BCA_API_KEY", c => c.String(maxLength: 50));
            AddColumn("dbo.SIFSYS", "BCA_API_SECRET", c => c.String(maxLength: 50));
            AddColumn("dbo.SIFSYS", "BCA_CLIENT_ID", c => c.String(maxLength: 50));
            AddColumn("dbo.SIFSYS", "BCA_CLIENT_SECRET", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SIFSYS", "BCA_CLIENT_SECRET");
            DropColumn("dbo.SIFSYS", "BCA_CLIENT_ID");
            DropColumn("dbo.SIFSYS", "BCA_API_SECRET");
            DropColumn("dbo.SIFSYS", "BCA_API_KEY");
        }
    }
}
