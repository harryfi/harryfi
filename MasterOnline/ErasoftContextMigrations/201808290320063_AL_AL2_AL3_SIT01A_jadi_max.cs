namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AL_AL2_AL3_SIT01A_jadi_max : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.SIT01A", "AL3", c => c.String(unicode: false));
            AlterColumn("dbo.SIT01A", "AL2", c => c.String(unicode: false));
            AlterColumn("dbo.SIT01A", "AL1", c => c.String(unicode: false));
            AlterColumn("dbo.SIT01A", "AL", c => c.String(unicode: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SIT01A", "AL", c => c.String(maxLength: 40, unicode: false));
            AlterColumn("dbo.SIT01A", "AL1", c => c.String(maxLength: 40, unicode: false));
            AlterColumn("dbo.SIT01A", "AL2", c => c.String(maxLength: 40, unicode: false));
            AlterColumn("dbo.SIT01A", "AL3", c => c.String(maxLength: 40, unicode: false));
        }
    }
}
