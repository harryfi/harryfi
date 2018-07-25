namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add2FieldsInARF01 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ARF01", "API_CLIENT_U", c => c.String(maxLength: 50));
            AddColumn("dbo.ARF01", "API_CLIENT_P", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ARF01", "API_CLIENT_P");
            DropColumn("dbo.ARF01", "API_CLIENT_U");
        }
    }
}
