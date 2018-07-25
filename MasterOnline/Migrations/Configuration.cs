using System.IO;
using System.Reflection;
using System.Text;
using MasterOnline.Models;

namespace MasterOnline.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<MasterOnline.MoDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            MigrationsDirectory = @"Migrations";
        }

        protected override void Seed(MasterOnline.MoDbContext context)
        {
            //No seed currently
        }
    }
}
