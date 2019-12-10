using Microsoft.EntityFrameworkCore;
using WebCsvParser.Models;

namespace WebCsvParser.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<Category> Category { get; set; }
        public DbSet<DataFile> DataFile { get; set; }
        public DbSet<ErrorList> ErrorList { get; set; }
        public DbSet<LineItem> LineItem { get; set; }
        public DbSet<Mapping> Mapping { get; set; }
        public DbSet<TempData> TempData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DataFile>(entity =>
            {
                entity
                    .HasKey(i => i.Id);

                entity.Property(m => m.FileName)
					.HasMaxLength(50);

				entity.Property(m => m.FilePath)
					.HasMaxLength(100);

                entity
                    .Property(i => i.Id)    
                    .HasAnnotation("MySql:ValueGeneratedOnAdd",true)
                    .ValueGeneratedOnAdd();

                entity
                    .HasIndex(u => u.FileName)
                    .IsUnique();

                entity
                    .HasIndex(u => u.FilePath)
                    .IsUnique();

                entity
                    .HasMany(p => p.LineItems)
                    .WithOne(b => b.DataFile)
                    .HasForeignKey(b => b.DataFileId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ErrorList>(entity =>
            {
                entity
                    .HasKey(i => i.Id);

                entity
                    .Property(i => i.Id)    
                    .HasAnnotation("MySql:ValueGeneratedOnAdd",true)
                    .ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<TempData>(entity =>
            {
                entity
                    .HasKey(i => i.Id);

                entity
                    .Property(i => i.Id)    
                    .HasAnnotation("MySql:ValueGeneratedOnAdd",true)
                    .ValueGeneratedOnAdd();

                entity
                    .HasOne(p => p.DataFile)
                    .WithMany(b => b.LineItems)
                    .HasForeignKey(b => b.DataFileId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<LineItem>(entity =>
            {
                entity
                    .HasKey(i => i.Id);

                entity.Property(m => m.Name)
					.HasMaxLength(190);

                entity
                    .Property(i => i.Id)    
                    .HasAnnotation("MySql:ValueGeneratedOnAdd",true)
                    .ValueGeneratedOnAdd();

                entity
                    .HasMany(p => p.Mapping)
                    .WithOne(b => b.LineItem)
                    .HasForeignKey(b => b.LineItemId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity
                    .HasIndex(p => p.Name)
                    .IsUnique();
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity
                    .HasKey(i => i.Id);

                entity
                    .Property(i => i.Id)    
                    .HasAnnotation("MySql:ValueGeneratedOnAdd",true)
                    .ValueGeneratedOnAdd();

                entity
                    .HasMany(p => p.Mapping)
                    .WithOne(b => b.Category)
                    .HasForeignKey(b => b.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Mapping>(entity =>
            {
                entity
                    .HasKey(i => i.Id);

                entity
                    .Property(i => i.Id)    
                    .HasAnnotation("MySql:ValueGeneratedOnAdd",true)
                    .ValueGeneratedOnAdd();

                entity
                    .HasOne(p => p.Category)
                    .WithMany(b => b.Mapping)
                    .HasForeignKey(b => b.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity
                    .HasOne(p => p.LineItem)
                    .WithMany(b => b.Mapping)
                    .HasForeignKey(b => b.LineItemId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity
                    .HasIndex(p => new { p.CategoryId, p.LineItemId })
                    .IsUnique();
            });
        }
    }
}
