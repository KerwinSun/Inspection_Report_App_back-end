﻿// <auto-generated />
using System;
using InspectionReport.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace InspectionReport.Migrations
{
    [DbContext(typeof(ReportContext))]
    [Migration("20180922063137_area-name-change")]
    partial class areanamechange
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.1-rtm-30846")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("InspectionReport.Models.AreaInspected", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("Exterior");

                    b.Property<bool>("RoofExterior");

                    b.Property<bool>("RoofSpace");

                    b.Property<bool>("Services");

                    b.Property<bool>("Site");

                    b.Property<bool>("Subfloor");

                    b.Property<bool>("other");

                    b.HasKey("Id");

                    b.ToTable("AreaInspected");
                });

            modelBuilder.Entity("InspectionReport.Models.Category", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("Count");

                    b.Property<long>("HouseId");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.HasIndex("HouseId");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("InspectionReport.Models.Client", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("EmailAddress");

                    b.Property<string>("HomePhoneNumber");

                    b.Property<string>("MobilePhoneNumber");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Client");
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

                    b.Property<long?>("AreaInspectedId");

                    b.Property<string>("Comments");

                    b.Property<bool>("Completed");

                    b.Property<string>("ConstructionType");

                    b.Property<string>("EstimateSummary");

                    b.Property<DateTime>("InspectionDate");

                    b.Property<string>("RoomsSummary");

                    b.Property<long?>("SummonsedById");

                    b.HasKey("Id");

                    b.HasIndex("AreaInspectedId");

                    b.HasIndex("SummonsedById");

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

            modelBuilder.Entity("InspectionReport.Models.Media", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long>("FeatureId");

                    b.Property<string>("MediaName")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("FeatureId");

                    b.ToTable("Media");
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

            modelBuilder.Entity("InspectionReport.Models.House", b =>
                {
                    b.HasOne("InspectionReport.Models.AreaInspected", "AreaInspected")
                        .WithMany()
                        .HasForeignKey("AreaInspectedId");

                    b.HasOne("InspectionReport.Models.Client", "SummonsedBy")
                        .WithMany()
                        .HasForeignKey("SummonsedById");
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

            modelBuilder.Entity("InspectionReport.Models.Media", b =>
                {
                    b.HasOne("InspectionReport.Models.Feature", "Feature")
                        .WithMany("ImageFileNames")
                        .HasForeignKey("FeatureId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
