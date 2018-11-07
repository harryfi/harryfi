namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ARF01C_field_EMAIL_Not_Required : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ARF01C", "EMAIL", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ARF01C", "EMAIL", c => c.String(nullable: false, maxLength: 50));
        }
    }
}
