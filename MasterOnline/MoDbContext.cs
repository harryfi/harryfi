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

        public MoDbContext()
            : base("name=MoDbContext")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
