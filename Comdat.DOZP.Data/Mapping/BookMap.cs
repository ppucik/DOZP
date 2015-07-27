using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

using Comdat.DOZP.Core;

namespace Comdat.DOZP.Data.Mapping
{
    public class BookMap : EntityTypeConfiguration<Book>
    {
        public BookMap()
        {
            // Primary Key
            this.HasKey(t => t.BookID);

            // Properties
            this.Property(t => t.BookID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.Property(t => t.SysNo)
                .IsRequired()
                .HasMaxLength(20);

            this.Property(t => t.ISBN)
                .HasMaxLength(50);

            this.Property(t => t.ISBN)
                .HasMaxLength(50);

            this.Property(t => t.NBN)
                .HasMaxLength(50);

            this.Property(t => t.OCLC)
                .HasMaxLength(50);

            this.Property(t => t.Author)
                .HasMaxLength(200);

            this.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(1000);

            this.Property(t => t.Year)
                .HasMaxLength(20);

            this.Property(t => t.Volume)
                .HasMaxLength(100);

            this.Property(t => t.Barcode)
                .HasMaxLength(20);

            this.Property(t => t.Comment)
                .HasMaxLength(1000);

            // Table & Column Mappings
            this.ToTable("Book");
            this.Property(t => t.BookID).HasColumnName("BookID");
            this.Property(t => t.CatalogueID).HasColumnName("CatalogueID");
            this.Property(t => t.SysNo).HasColumnName("SysNo");
            this.Property(t => t.FileIndex).HasColumnName("FileIndex");
            this.Property(t => t.ISBN).HasColumnName("ISBN");
            this.Property(t => t.NBN).HasColumnName("NBN");
            this.Property(t => t.OCLC).HasColumnName("OCLC");
            this.Property(t => t.Author).HasColumnName("Author");
            this.Property(t => t.Title).HasColumnName("Title");
            this.Property(t => t.Year).HasColumnName("Year");
            this.Property(t => t.Volume).HasColumnName("Volume");
            this.Property(t => t.Barcode).HasColumnName("Barcode");
            this.Property(t => t.Created).HasColumnName("Created");
            this.Property(t => t.Modified).HasColumnName("Modified");
            this.Property(t => t.Comment).HasColumnName("Comment");

            // Relationships
            this.HasRequired(t => t.Catalogue)
                .WithMany(t => t.Books)
                .HasForeignKey(d => d.CatalogueID);
        }
    }
}
