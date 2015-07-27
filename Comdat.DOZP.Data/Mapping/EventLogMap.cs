using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

using Comdat.DOZP.Core;

namespace Comdat.DOZP.Data.Mapping
{
    public class EventLogMap : EntityTypeConfiguration<EventLog>
    {
        public EventLogMap()
        {
            // Primary Key
            this.HasKey(t => t.EventLogID);

            // Properties
            this.Property(t => t.EventLogID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.Property(t => t.UserName)
                .HasMaxLength(100);

            this.Property(t => t.Computer)
                .HasMaxLength(100);

            this.Property(t => t.Message)
                .IsRequired();

            // Table & Column Mappings
            this.ToTable("EventLog");
            this.Property(t => t.EventLogID).HasColumnName("EventLogID");
            this.Property(t => t.Level).HasColumnName("Level");
            this.Property(t => t.UserName).HasColumnName("UserName");
            this.Property(t => t.Computer).HasColumnName("Computer");
            this.Property(t => t.Logged).HasColumnName("Logged");
            this.Property(t => t.Message).HasColumnName("Message");
        }
    }
}
