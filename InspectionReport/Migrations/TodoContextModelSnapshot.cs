﻿// <auto-generated />
using System;
using InspectionReport.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace InspectionReport.Migrations
{
    [DbContext(typeof(ReportContext))]
    partial class TodoContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.1-rtm-30846")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("InspectionReport.Models.Category", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long>("HouseId");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.HasIndex("HouseId");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("InspectionReport.Models.Feature", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long?>("CategoryId");

                    b.Property<string>("Comments");

                    b.Property<int?>("Grade");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.ToTable("Feature");
                });

            modelBuilder.Entity("InspectionReport.Models.House", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Address");

                    b.Property<bool>("Completed");

                    b.Property<string>("ConstructionType");

                    b.Property<DateTime>("InspectionDate");

                    b.HasKey("Id");

                    b.ToTable("House");
                });

            modelBuilder.Entity("InspectionReport.Models.HouseUser", b =>
                {
                    b.Property<long>("UserId");

                    b.Property<long>("HouseId");

                    b.HasKey("UserId", "HouseId");

                    b.HasIndex("HouseId");

                    b.ToTable("HouseUser");
                });

            modelBuilder.Entity("InspectionReport.Models.TodoItem", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("IsComplete");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("TodoItems");
                });

            modelBuilder.Entity("InspectionReport.Models.User", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("InspectionReport.Models.Category", b =>
                {
                    b.HasOne("InspectionReport.Models.House", "House")
                        .WithMany("Categories")
                        .HasForeignKey("HouseId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("InspectionReport.Models.Feature", b =>
                {
                    b.HasOne("InspectionReport.Models.Category", "Category")
                        .WithMany("Features")
                        .HasForeignKey("CategoryId");
                });

            modelBuilder.Entity("InspectionReport.Models.HouseUser", b =>
                {
                    b.HasOne("InspectionReport.Models.House", "House")
                        .WithMany("InspectedBy")
                        .HasForeignKey("HouseId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("InspectionReport.Models.User", "User")
                        .WithMany("Inspected")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
