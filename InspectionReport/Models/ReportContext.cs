using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InspectionReport.Models
{
    public class ReportContext : IdentityDbContext<ApplicationUser>
    {

        public ReportContext(DbContextOptions<ReportContext> options)
            : base(options)
        {
        }

        public DbSet<TodoItem> TodoItems { get; set; }

        public DbSet<User> User { get; set; }

        public DbSet<House> House { get; set; }

        public DbSet<HouseUser> HouseUser { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Feature> Feature { get; set; }

        public DbSet<Media> Media { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>()
                .HasOne(p => p.House)
                .WithMany(b => b.Categories)
                .OnDelete(DeleteBehavior.Cascade);

            // establishing a many-to-many relationship between 
            modelBuilder.Entity<HouseUser>()
                .HasKey(t => new { t.UserId, t.HouseId });

            modelBuilder.Entity<HouseUser>()
                .HasOne(pt => pt.User)
                .WithMany(p => p.Inspected)
                .HasForeignKey(pt => pt.UserId);

            modelBuilder.Entity<HouseUser>()
                .HasOne(pt => pt.House)
                .WithMany(t => t.InspectedBy)
                .HasForeignKey(pt => pt.HouseId);

            // establishing a one-many relationship between Category and Feature
            modelBuilder.Entity<Feature>()
                .HasOne(p => p.Category)
                .WithMany(b => b.Features);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EFProviders.InMemory;Trusted_Connection=True;ConnectRetryCount=0");
            }
        }
    }
}