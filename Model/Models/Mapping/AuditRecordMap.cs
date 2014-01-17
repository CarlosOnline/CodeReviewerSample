using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace CodeReviewer.Models.Mapping
{
    public class AuditRecordMap : EntityTypeConfiguration<AuditRecord>
    {
        public AuditRecordMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.UserName)
                .IsRequired()
                .HasMaxLength(200);

            this.Property(t => t.Action)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("AuditRecord");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.TimeStamp).HasColumnName("TimeStamp");
            this.Property(t => t.UserName).HasColumnName("UserName");
            this.Property(t => t.ChangeListId).HasColumnName("ChangeListId");
            this.Property(t => t.Action).HasColumnName("Action");
            this.Property(t => t.Description).HasColumnName("Description");
        }
    }
}
