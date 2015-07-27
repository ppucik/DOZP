using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

using Comdat.DOZP.Core;

namespace Comdat.DOZP.Data.Mapping
{
    public class InstitutionMap : EntityTypeConfiguration<Institution>
    {
        public InstitutionMap()
        {
            // Primary Key
            this.HasKey(t => t.InstitutionID);

            // Properties
            this.Property(t => t.InstitutionID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.Property(t => t.Sigla)
                .IsRequired()
                .HasMaxLength(6);

            this.Property(t => t.Acronym )
                .HasMaxLength(20);

            this.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(250);

            this.Property(t => t.Address)
                .HasMaxLength(500);

            this.Property(t => t.Homepage)
                .HasMaxLength(100);

            this.Property(t => t.AlephUrl)
                .HasMaxLength(100);

            this.Property(t => t.Email)
                .HasMaxLength(100);

            this.Property(t => t.Telephone)
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("Institution");
            this.Property(t => t.InstitutionID).HasColumnName("InstitutionID");
            this.Property(t => t.Sigla).HasColumnName("Sigla");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.Acronym).HasColumnName("Acronym");
            this.Property(t => t.Description).HasColumnName("Description");
            this.Property(t => t.Address).HasColumnName("Address");
            this.Property(t => t.Homepage).HasColumnName("Homepage");
            this.Property(t => t.AlephUrl).HasColumnName("AlephUrl");
            this.Property(t => t.Email).HasColumnName("Email");
            this.Property(t => t.Telephone).HasColumnName("Telephone");
            this.Property(t => t.Enabled).HasColumnName("Enabled");
        }
    }
}
