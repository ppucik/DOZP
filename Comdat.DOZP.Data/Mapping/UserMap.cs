using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

using Comdat.DOZP.Core;

namespace Comdat.DOZP.Data.Mapping
{
    public class UserMap : EntityTypeConfiguration<User>
    {
        public UserMap()
        {
            // Primary Key
            this.HasKey(t => t.UserName);

            // Properties
            this.Property(t => t.UserName)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.PasswordHash)
                .IsRequired()
                .IsFixedLength()
                .HasMaxLength(64);

            this.Property(t => t.PasswordSalt)
                .IsRequired()
                .IsFixedLength()
                .HasMaxLength(128);

            this.Property(t => t.Email)
                .IsRequired();

            this.Property(t => t.FullName)
                .HasMaxLength(200);

            this.Property(t => t.Telephone)
                .HasMaxLength(100);

           
            // Table & Column Mappings
            this.ToTable("User");
            this.Property(t => t.UserName).HasColumnName("UserName");
            this.Property(t => t.PasswordHash).HasColumnName("PasswordHash");
            this.Property(t => t.PasswordSalt).HasColumnName("PasswordSalt");
            this.Property(t => t.Email).HasColumnName("Email");
            this.Property(t => t.Comment).HasColumnName("Comment");
            this.Property(t => t.IsApproved).HasColumnName("IsApproved");
            this.Property(t => t.DateCreated).HasColumnName("DateCreated");
            this.Property(t => t.DateLastLogin).HasColumnName("DateLastLogin");
            this.Property(t => t.DateLastActivity).HasColumnName("DateLastActivity");
            this.Property(t => t.DateLastPasswordChange).HasColumnName("DateLastPasswordChange");
            this.Property(t => t.DateLastLogout).HasColumnName("DateLastLogout");
            this.Property(t => t.LastUpdate).HasColumnName("LastUpdate");
            this.Property(t => t.InstitutionID).HasColumnName("InstitutionID");
            this.Property(t => t.FullName).HasColumnName("FullName");
            this.Property(t => t.Telephone).HasColumnName("Telephone");

            //Not Mapped
            this.Ignore(t => t.RoleName);

            // Relationships
            this.HasOptional(t => t.Institution)
                .WithMany(t => t.Users)
                .HasForeignKey(d => d.InstitutionID);
        }
    }
}
