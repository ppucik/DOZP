using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

using Comdat.DOZP.Core;

namespace Comdat.DOZP.Data.Mapping
{
    public class OperationMap : EntityTypeConfiguration<Operation>
    {
        public OperationMap()
        {
            // Primary Key
            this.HasKey(t => t.OperationID);

            // Properties
            this.Property(t => t.OperationID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.Property(t => t.UserName)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.Computer)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.Comment)
                .HasMaxLength(1000);

            // Table & Column Mappings
            this.ToTable("Operation");
            this.Property(t => t.OperationID).HasColumnName("OperationID");
            this.Property(t => t.ScanFileID).HasColumnName("ScanFileID");
            this.Property(t => t.UserName).HasColumnName("UserName");
            this.Property(t => t.Computer).HasColumnName("Computer");
            this.Property(t => t.Executed).HasColumnName("Executed");
            this.Property(t => t.Comment).HasColumnName("Comment");
            this.Property(t => t.Status).HasColumnName("Status");

            // Relationships
            this.HasRequired(t => t.ScanFile)
                .WithMany(t => t.Operations)
                .HasForeignKey(d => d.ScanFileID)
                .WillCascadeOnDelete(true);

            this.HasRequired(t => t.User)
                .WithMany(t => t.Operations)
                .HasForeignKey(d => d.UserName);
        }
    }
}
