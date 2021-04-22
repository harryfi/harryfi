using MasterOnline.Migrations;
using MasterOnline.Models;

namespace MasterOnline
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class MoDbContext : DbContext
    {
        public DbSet<Account> Account { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<SecUser> SecUser { get; set; }
        public DbSet<FormMos> FormMoses { get; set; }
        public DbSet<Marketplace> Marketplaces { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Provinsi> Provinsi { get; set; }
        public DbSet<KabupatenKota> KabupatenKota { get; set; }
        public DbSet<Ekspedisi> Ekspedisi { get; set; }
        public DbSet<Subscription> Subscription { get; set; }
        public DbSet<AktivitasSubscription> AktivitasSubscription { get; set; }
        public DbSet<TransaksiMidtrans> TransaksiMidtrans { get; set; }
        public DbSet<Promo> Promo { get; set; }
        public DbSet<ATTRIBUTE_BLIBLI> AttributeBlibli { get; set; }
        public DbSet<ATTRIBUTE_OPT_BLIBLI> AttributeOptBlibli { get; set; }
        public DbSet<CATEGORY_BLIBLI> CategoryBlibli { get; set; }
        public DbSet<MIDTRANS_DATA> MidtransData { get; set; }
        public virtual DbSet<CATEGORY_LAZADA> CATEGORY_LAZADA { get; set; }
        public virtual DbSet<ATTRIBUTE_LAZADA> ATTRIBUTE_LAZADA { get; set; }
        public virtual DbSet<ATTRIBUTE_OPT_LAZADA> ATTRIBUTE_OPT_LAZADA { get; set; }
        public DbSet<CATEGORY_ELEVENIA> CategoryElevenia { get; set; }
        public DbSet<ATTRIBUTE_ELEVENIA> AttributeElevenia { get; set; }
        public DbSet<Partner> Partner { get; set; }
        public DbSet<CATEGORY_SHOPEE> CategoryShopee { get; set; }
        public DbSet<ATTRIBUTE_SHOPEE> AttributeShopee { get; set; }
        public DbSet<ATTRIBUTE_OPT_SHOPEE> AttributeOptShopee { get; set; }
        public DbSet<CATEGORY_TOKPED> CategoryTokped { get; set; }
        public DbSet<ATTRIBUTE_TOKPED> AttributeTokped { get; set; }
        public DbSet<ATTRIBUTE_OPT_TOKPED> AttributeOptTokped { get; set; }
        public DbSet<ATTRIBUTE_UNIT_TOKPED> AttributeUnitTokped { get; set; }
        public DbSet<CATEGORY_BUKALAPAK> CATEGORY_BUKALAPAKs { get; set; }
        public DbSet<BRAND_LAZADA> BrandLazada { get; set; }
        //add by fauzi for JDID
        public DbSet<BRAND_JDID> BrandJDID { get; set; }
        //end by fauzi


        public DbSet<CATEGORY_JDID> CATEGORY_JDID { get; set; }
        public DbSet<CATEGORY_SHOPIFY> CategoryShopify { get; set; }

        //add by fauzi for 82cart
        public DbSet<CATEGORY_82CART> Category82Cart { get; set; }
        public DbSet<ATTRIBUTE_82CART> Attribute82Cart { get; set; }
        public DbSet<Tutorial_Header> Tutorial_Header { get; set; }
        public DbSet<Tutorial_Detail> Tutorial_Detail { get; set; }

        //add by Tri for bukalapak
        public DbSet<BUKALAPAK_TOKEN> BUKALAPAK_TOKEN { get; set; }
        public DbSet<BUKALAPAK_KURIR_MANUAL> BUKALAPAK_KURIR_MANUAL { get; set; }

        public DbSet<Addons> Addons { get; set; }
        public DbSet<Addons_Customer> Addons_Customer { get; set; }

        public MoDbContext()
            : base("name=MoDbContext")
        {
        }

#if (AWS || Debug_AWS)

        //public MoDbContext(string dbSourceEra)
        //    : base($"Server=13.250.232.74, 1433;initial catalog=MO;" +
        //           $"user id=sa;password=admin123^;multipleactiveresultsets=True;" +
        //           $"application name=EntityFramework")
        //{
        //}

        public MoDbContext(string dbSourceEra)
             : base($"Server=172.31.20.192, 1433;initial catalog=MO;" +
                    $"user id=sa;password=admin123^;multipleactiveresultsets=True;" +
                    $"application name=EntityFramework")
        {
        }
        //end remark by fauzi for testing REDIS elasticache AWS 26/01/2021

        //changes by fauzi for testing REDIS elasticache AWS 26/01/2021
        //public MoDbContext(string dbSourceEra)
        //     : base($"Server=172.31.20.73, 1433;initial catalog=MO;" +
        //            $"user id=sa;password=admin123^;multipleactiveresultsets=True;" +
        //            $"application name=EntityFramework")
        //{
        //}
        //end changes by fauzi for testing REDIS elasticache AWS 26/01/2021


        //add by nurul 21/12/2020, khusus proses akhir tahun
        public MoDbContext(string dbSourceEra, string dbSourceEra2)
            : base($"Server=" + dbSourceEra2 + ", 1433;initial catalog=MO;" +
                   $"user id=sa;password=admin123^;multipleactiveresultsets=True;" +
                   $"application name=EntityFramework")
        {
        }
        //end add by nurul 21/12/2020, khusus proses akhir tahun

        //public MoDbContext(string dbSourceEra)
        //   : base($"Server=172.31.20.73, 1433;initial catalog=MO;" +
        //          $"user id=sa;password=admin123^;multipleactiveresultsets=True;" +
        //          $"application name=EntityFramework")
        //{
        //}

#elif (DEV || DEBUG)
        //remark by fauzi for testing REDIS elasticache AWS 26/01/2021
        //public MoDbContext(string dbSourceEra)
        //     : base($"Server=13.251.222.53, 1433;initial catalog=MO;" +
        //            $"user id=sa;password=admin123^;multipleactiveresultsets=True;" +
        //            $"application name=EntityFramework")
        //{
        //}
        public MoDbContext(string dbSourceEra)
             : base($"Server=54.151.175.62, 12350;initial catalog=MO;" +
                    $"user id=sa;password=admin123^;multipleactiveresultsets=True;" +
                    $"application name=EntityFramework")
        {
        }
        //end remark by fauzi for testing REDIS elasticache AWS 26/01/2021

        //changes by fauzi for testing REDIS elasticache AWS 26/01/2021
        //public MoDbContext(string dbSourceEra)
        //     : base($"Server=172.31.20.73, 1433;initial catalog=MO;" +
        //            $"user id=sa;password=admin123^;multipleactiveresultsets=True;" +
        //            $"application name=EntityFramework")
        //{
        //}
        //end changes by fauzi for testing REDIS elasticache AWS 26/01/2021

        //add by nurul 21/12/2020, khusus proses akhir tahun
        public MoDbContext(string dbSourceEra, string dbSourceEra2)
             : base($"Server=" + dbSourceEra2 + ", " + dbSourceEra + ";initial catalog=MO;" +
                    $"user id=sa;password=admin123^;multipleactiveresultsets=True;" +
                    $"application name=EntityFramework")
        {
        }
        //end add by nurul 21/12/2020, khusus proses akhir tahun

#endif

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
