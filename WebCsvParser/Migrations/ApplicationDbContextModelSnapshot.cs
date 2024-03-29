﻿// <auto-generated />

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using WebCsvParser.Context;

namespace WebCsvParser.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.1-rtm-30846")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("WebCsvParser.Models.Category", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("MySql:ValueGeneratedOnAdd", true);

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("Category");
                });

            modelBuilder.Entity("WebCsvParser.Models.DataFile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("MySql:ValueGeneratedOnAdd", true);

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<string>("FilePath")
                        .IsRequired()
                        .HasMaxLength(100);

                    b.HasKey("Id");

                    b.HasIndex("FileName")
                        .IsUnique();

                    b.HasIndex("FilePath")
                        .IsUnique();

                    b.ToTable("DataFile");
                });

            modelBuilder.Entity("WebCsvParser.Models.ErrorList", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("MySql:ValueGeneratedOnAdd", true);

                    b.Property<int>("LineNumber");

                    b.Property<string>("Message")
                        .IsRequired();

                    b.Property<string>("Property")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("ErrorList");
                });

            modelBuilder.Entity("WebCsvParser.Models.LineItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("MySql:ValueGeneratedOnAdd", true);

                    b.Property<int?>("LineNumber")
                        .IsRequired();

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(190);

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("LineItem");
                });

            modelBuilder.Entity("WebCsvParser.Models.Mapping", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("MySql:ValueGeneratedOnAdd", true);

                    b.Property<int>("CategoryId");

                    b.Property<string>("Description")
                        .IsRequired();

                    b.Property<int>("LineItemId");

                    b.HasKey("Id");

                    b.HasIndex("LineItemId");

                    b.HasIndex("CategoryId", "LineItemId")
                        .IsUnique();

                    b.ToTable("Mapping");
                });

            modelBuilder.Entity("WebCsvParser.Models.TempData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("MySql:ValueGeneratedOnAdd", true);

                    b.Property<string>("Category")
                        .IsRequired();

                    b.Property<int>("DataFileId");

                    b.Property<int?>("LineNumber")
                        .IsRequired();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<double?>("Price");

                    b.Property<double?>("Quantity");

                    b.HasKey("Id");

                    b.HasIndex("DataFileId");

                    b.ToTable("TempData");
                });

            modelBuilder.Entity("WebCsvParser.Models.Mapping", b =>
                {
                    b.HasOne("WebCsvParser.Models.Category", "Category")
                        .WithMany("Mapping")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("WebCsvParser.Models.LineItem", "LineItem")
                        .WithMany("Mapping")
                        .HasForeignKey("LineItemId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("WebCsvParser.Models.TempData", b =>
                {
                    b.HasOne("WebCsvParser.Models.DataFile", "DataFile")
                        .WithMany("LineItems")
                        .HasForeignKey("DataFileId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
