namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_table_log_Upload_faktur : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LOG_IMPORT_FAKTUR",
                c => new
                    {
                        RECNUM = c.Int(nullable: false, identity: true),
                        UPLOADER = c.String(maxLength: 100),
                        LAST_FAKTUR_UPLOADED = c.String(maxLength: 100),
                        UPLOAD_DATETIME = c.DateTime(nullable: false),
                        LAST_FAKTUR_UPLOADED_DATETIME = c.DateTime(nullable: false),
                        CUST = c.String(),
                        LOG_FILE = c.String(),
                    })
                .PrimaryKey(t => t.RECNUM);
            
            AlterColumn("dbo.SIT01A", "NO_REF", c => c.String(maxLength: 100, unicode: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SIT01A", "NO_REF", c => c.String(maxLength: 15, unicode: false));
            DropTable("dbo.LOG_IMPORT_FAKTUR");
        }
    }
}
