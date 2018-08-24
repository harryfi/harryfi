namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FIELD_EMAIL_SIFSYS_TAMBAHAN_LENGTH : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.SIFSYS_TAMBAHAN", "EMAIL", c => c.String(nullable: false, maxLength: 50));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SIFSYS_TAMBAHAN", "EMAIL", c => c.String(nullable: false, maxLength: 30));
        }
    }
}
