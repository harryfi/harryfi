namespace MasterOnline.ErasoftContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class change_avalue1_tempbrgmp_stf02h : DbMigration
    {
        public override void Up()
        {
            //CreateTable(
            //    "dbo.TEMP_SHOPEE_ORDERS",
            //    c => new
            //        {
            //            ordersn = c.String(nullable: false, maxLength: 128),
            //            note = c.String(),
            //            estimated_shipping_fee = c.String(),
            //            payment_method = c.String(),
            //            escrow_amount = c.String(),
            //            message_to_seller = c.String(),
            //            shipping_carrier = c.String(),
            //            currency = c.String(),
            //            create_time = c.DateTime(nullable: false),
            //            pay_time = c.DateTime(nullable: false),
            //            Recipient_Address_town = c.String(),
            //            Recipient_Address_city = c.String(),
            //            Recipient_Address_name = c.String(),
            //            Recipient_Address_district = c.String(),
            //            Recipient_Address_country = c.String(),
            //            Recipient_Address_zipcode = c.String(),
            //            Recipient_Address_full_address = c.String(),
            //            Recipient_Address_phone = c.String(),
            //            Recipient_Address_state = c.String(),
            //            days_to_ship = c.Int(nullable: false),
            //            tracking_no = c.String(),
            //            order_status = c.String(),
            //            note_update_time = c.DateTime(nullable: false),
            //            update_time = c.DateTime(nullable: false),
            //            goods_to_declare = c.Boolean(nullable: false),
            //            total_amount = c.String(),
            //            service_code = c.String(),
            //            country = c.String(),
            //            actual_shipping_cost = c.String(),
            //            cod = c.Boolean(nullable: false),
            //            dropshipper = c.String(),
            //            buyer_username = c.String(),
            //            CUST = c.String(),
            //            NAMA_CUST = c.String(),
            //            CONN_ID = c.String(),
            //        })
            //    .PrimaryKey(t => t.ordersn);

            //CreateTable(
            //    "dbo.TEMP_SHOPEE_ORDERS_ITEM",
            //    c => new
            //        {
            //            ordersn = c.String(nullable: false, maxLength: 128),
            //            item_id = c.Int(nullable: false),
            //            variation_id = c.Long(nullable: false),
            //            weight = c.Single(nullable: false),
            //            item_name = c.String(),
            //            is_wholesale = c.Boolean(nullable: false),
            //            item_sku = c.String(),
            //            variation_discounted_price = c.String(),
            //            variation_name = c.String(),
            //            variation_quantity_purchased = c.Int(nullable: false),
            //            variation_sku = c.String(),
            //            variation_original_price = c.String(),
            //            pay_time = c.DateTime(nullable: false),
            //            CUST = c.String(),
            //            NAMA_CUST = c.String(),
            //            CONN_ID = c.String(),
            //        })
            //    .PrimaryKey(t => new { t.ordersn, t.item_id, t.variation_id });

            //CreateTable(
            //    "dbo.TEMP_TOKPED_ORDERS",
            //    c => new
            //        {
            //            order_id = c.String(nullable: false, maxLength: 128),
            //            product_id = c.Int(nullable: false),
            //            conn_id = c.String(nullable: false, maxLength: 128),
            //            fs_id = c.String(),
            //            accept_partial = c.Boolean(),
            //            invoice_ref_num = c.String(),
            //            product_name = c.String(),
            //            product_quantity = c.Int(),
            //            product_notes = c.String(),
            //            product_weight = c.Double(),
            //            product_total_weight = c.Double(),
            //            product_price = c.Int(),
            //            product_total_price = c.Int(),
            //            product_currency = c.String(),
            //            product_sku = c.String(),
            //            products_fulfilled_product_id = c.Int(),
            //            products_fulfilled_quantity_deliver = c.Int(),
            //            products_fulfilled_quantity_reject = c.Int(),
            //            device_type = c.String(),
            //            buyer_id = c.Int(),
            //            buyer_name = c.String(),
            //            buyer_phone = c.String(),
            //            buyer_email = c.String(),
            //            shop_id = c.Int(),
            //            payment_id = c.Int(),
            //            recipient_name = c.String(),
            //            recipient_address_address_full = c.String(),
            //            recipient_address_district = c.String(),
            //            recipient_address_city = c.String(),
            //            recipient_address_province = c.String(),
            //            recipient_address_country = c.String(),
            //            recipient_address_postal_code = c.String(),
            //            recipient_address_district_id = c.Int(),
            //            recipient_address_city_id = c.Int(),
            //            recipient_address_province_id = c.Int(),
            //            recipient_address_geo = c.String(),
            //            recipient_phone = c.String(),
            //            logistics_shipping_id = c.Int(),
            //            logistics_shipping_agency = c.String(),
            //            logistics_service_type = c.String(),
            //            amt_ttl_product_price = c.Int(),
            //            amt_shipping_cost = c.Int(),
            //            amt_insurance_cost = c.Int(),
            //            amt_ttl_amount = c.Int(),
            //            amt_voucher_amount = c.Int(),
            //            amt_toppoints_amount = c.Int(),
            //            dropshipper_info_name = c.String(),
            //            dropshipper_info_phone = c.String(),
            //            voucher_info_voucher_type = c.Decimal(precision: 18, scale: 2),
            //            voucher_info_voucher_code = c.String(),
            //            order_status = c.Int(),
            //            create_time = c.DateTime(nullable: false),
            //            custom_fields_awb = c.String(),
            //            CUST = c.String(),
            //            NAMA_CUST = c.String(),
            //        })
            //    .PrimaryKey(t => new { t.order_id, t.product_id, t.conn_id });

            //AddColumn("dbo.Promosis", "MP_PROMO_ID", c => c.String(maxLength: 100));
            AlterColumn("dbo.STF02H", "AVALUE_1", c => c.String());
            AlterColumn("dbo.TEMP_BRG_MP", "AVALUE_1", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.TEMP_BRG_MP", "AVALUE_1", c => c.String(maxLength: 250));
            AlterColumn("dbo.STF02H", "AVALUE_1", c => c.String(maxLength: 250));
            //DropColumn("dbo.Promosis", "MP_PROMO_ID");
            //DropTable("dbo.TEMP_TOKPED_ORDERS");
            //DropTable("dbo.TEMP_SHOPEE_ORDERS_ITEM");
            //DropTable("dbo.TEMP_SHOPEE_ORDERS");
        }
    }
}
