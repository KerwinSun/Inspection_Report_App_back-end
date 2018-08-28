using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.EntityFrameworkCore;

namespace InspectionReport.Models
{
    public class ReportContext : DbContext
    {
        public ReportContext(DbContextOptions<ReportContext> options)
            : base(options)
        {
        }

        public DbSet<TodoItem> TodoItems { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<House> House { get; set; }

        public DbSet<HouseUser> HouseUser { get; set; }

        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<Category>()
                .HasOne(p => p.House)
                .WithMany(b => b.Categories);

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
        }
    }
}