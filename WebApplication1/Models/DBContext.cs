﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class DBContext: DbContext
    {
        //public DBContext() : base("Data Source=.;Initial Catalog=cp33105_db;Integrated Security=true")
        public DBContext() : base("Data Source=.;Initial Catalog=sarzami1_shop;User Id=sarzami1_shopusr;Password=Amir@amir$amir2;")
        {
            this.Configuration.ProxyCreationEnabled = false;
            this.Configuration.LazyLoadingEnabled = false;   
        }
        static DBContext()
        {
           Database.SetInitializer<DBContext>(new MigrateDatabaseToLatestVersion<DBContext,configure>());
        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products{ get; set; }
        public DbSet<Factor> Factors{ get; set; }
        public DbSet<FactorItem> FactorItems{ get; set; }

        public DbSet<User> Users{ get; set; }
        public DbSet<MobileSlider> MobileSliders{ get; set; }
        public DbSet<SiteSlider> SiteSliders { get; set; }
        public DbSet<SpecialProduct> SpecialProducts{ get; set; }
        public DbSet<Banner> Banners{ get; set; }
        public DbSet<Member> Members{ get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<DiscountCode> DiscountCode { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Role> Roles{ get; set; }
        public DbSet<UserProduct> UserProducts{ get; set; }
        public DbSet<UserComment> UserComments { get; set; }
        public DbSet<WholeSaler> WholeSalers{ get; set; }
        public DbSet<Email> Emails{ get; set; }
        public DbSet<ConfirmEmail> ConfirmEmails{ get; set; }
        public DbSet<UserRecover> UserRecover{ get; set; }
        public DbSet<Complaint> Complaints { get; set; }


        public DbSet<MarketerSlider> MarketerSliders{ get; set; }
        public DbSet<MarketerUser> MarketerUsers { get; set; }

        public DbSet<MarketerNews> MarketerNews { get; set; }
        public DbSet<MarketerPrize> MarketerPrizes{ get; set; }
        public DbSet<MarketerChat> MarketerChats { get; set; }
        public DbSet<MarketerFactor> MarketerFactor { get; set; }
        public DbSet<MarketerFactorItem> MarketerFactorItem { get; set; }
        public DbSet<Commission> Commission { get; set; }
        public DbSet<ProductPresent> ProductPresent { get; set; }
        class configure : System.Data.Entity.Migrations.DbMigrationsConfiguration<DBContext>
        {
            public configure()
            {
                this.AutomaticMigrationsEnabled = true;
                this.AutomaticMigrationDataLossAllowed = true;
            }
        }
    }
}
