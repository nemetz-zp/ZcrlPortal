using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using ZcrlPortal.Models;

namespace ZcrlPortal.DAL
{
    public class ZcrlContext : DbContext
    {
        // Соединяем контекст со строкой подключения в Web.config
        public ZcrlContext() : base("name=Default") { }

        public DbSet<Publication> PortalPublications { get; set; }
        
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserProfile> Profiles { get; set; }

        public DbSet<StaticChapter> Chapters { get; set; }

        public DbSet<RegistrationRequest> UserRegistrationRequests { get; set; }

        public DbSet<UploadFile> UploadFiles { get; set; }

        public DbSet<TenderYear> TenderYears { get; set; }
        public DbSet<TenderItem> TenderItems { get; set; }

        public DbSet<AdvBanner> Banners { get; set; }

        public DbSet<DataGroup> PortalDataGroups { get; set; }

        public DbSet<LogRecord> LogJournal { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Publication>().HasMany(p => p.Themes).WithMany(d => d.RelatedPublications);
        }
    }
}