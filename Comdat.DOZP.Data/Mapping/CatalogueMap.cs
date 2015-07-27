using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

using Comdat.DOZP.Core;

namespace Comdat.DOZP.Data.Mapping
{
    public class CatalogueMap : EntityTypeConfiguration<Catalogue>
    {
        public CatalogueMap()
        {
            // Primary Key
            this.HasKey(t => t.CatalogueID);

            // Properties
            this.Property(t => t.CatalogueID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(250);

            this.Property(t => t.ZServerUrl)
                .HasMaxLength(100);

            this.Property(t => t.DatabaseName)
                .HasMaxLength(20);

            this.Property(t => t.Charset)
                .HasMaxLength(20);

            this.Property(t => t.UserName)
                .HasMaxLength(20);

            this.Property(t => t.Password)
                .HasMaxLength(20);

            // Table & Column Mappings
            this.ToTable("Catalogue");
            this.Property(t => t.CatalogueID).HasColumnName("CatalogueID");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.Description).HasColumnName("Description");
            this.Property(t => t.ZServerUrl).HasColumnName("ZServerUrl");
            this.Property(t => t.ZServerPort).HasColumnName("ZServerPort");
            this.Property(t => t.DatabaseName).HasColumnName("DatabaseName");
            this.Property(t => t.Charset).HasColumnName("Charset");
            this.Property(t => t.UserName).HasColumnName("UserName");
            this.Property(t => t.Password).HasColumnName("Password");
            this.Property(t => t.Configuration).HasColumnName("Configuration");
            this.Property(t => t.Created).HasColumnName("Created");
            this.Property(t => t.Modified).HasColumnName("Modified");
            this.Property(t => t.Enabled).HasColumnName("Enabled");

            // Relationships
            this.HasMany(t => t.Institutions)
                .WithMany(t => t.Catalogues)
                .Map(m =>
                {
                    m.ToTable("InstitutionCatalogues");
                    m.MapLeftKey("CatalogueID");
                    m.MapRightKey("InstitutionID");
                });
        }
    }
}
