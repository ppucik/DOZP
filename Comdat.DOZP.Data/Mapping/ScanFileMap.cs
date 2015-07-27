using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

using Comdat.DOZP.Core;

namespace Comdat.DOZP.Data.Mapping
{
    public class ScanFileMap : EntityTypeConfiguration<ScanFile>
    {
        public ScanFileMap()
        {
            // Primary Key
            this.HasKey(t => t.ScanFileID);

            // Properties
            this.Property(t => t.ScanFileID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            
            this.Property(t => t.FileName)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.Comment)
                .HasMaxLength(1000);

            // Table & Column Mappings
            this.ToTable("ScanFile");
            this.Property(t => t.ScanFileID).HasColumnName("ScanFileID");
            this.Property(t => t.BookID).HasColumnName("BookID");
            this.Property(t => t.PartOfBook).HasColumnName("PartOfBook");
            this.Property(t => t.FileName).HasColumnName("FileName");
            this.Property(t => t.PageCount).HasColumnName("PageCount");
            this.Property(t => t.UseOCR).HasColumnName("UseOCR");
            this.Property(t => t.OcrText).HasColumnName("OcrText");
            this.Property(t => t.OcrTime).HasColumnName("OcrTime");
            this.Property(t => t.Created).HasColumnName("Created");
            this.Property(t => t.Modified).HasColumnName("Modified");
            this.Property(t => t.Comment).HasColumnName("Comment");
            this.Property(t => t.Status).HasColumnName("Status");

            //Not Mapped
            this.Ignore(t => t.ImageChanged);
            this.Ignore(t => t.ObalkyKnihUrl);

            // Relationships
            this.HasRequired(t => t.Book)
                .WithMany(t => t.ScanFiles)
                .HasForeignKey(d => d.BookID);
        }
    }
}
