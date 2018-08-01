namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTableMidtransData : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MIDTRANS_DATA",
                c => new
                    {
                        RecNum = c.Int(nullable: false, identity: true),
                        BANK = c.String(maxLength: 50),
                        TRANSACTION_TIME = c.String(maxLength: 50),
                        GROSS_AMOUNT = c.String(maxLength: 50),
                        ORDER_ID = c.String(maxLength: 50),
                        PAYMENT_TYPE = c.String(maxLength: 50),
                        SIGNATURE_KEY = c.String(maxLength: 250),
                        STATUS_CODE = c.String(maxLength: 50),
                        TRANSACTION_ID = c.String(maxLength: 50),
                        TRANSACTION_STATUS = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.RecNum);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.MIDTRANS_DATA");
        }
    }
}
