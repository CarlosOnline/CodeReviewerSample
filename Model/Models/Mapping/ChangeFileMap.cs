using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace CodeReviewer.Models.Mapping
{
    public class ChangeFileMap : EntityTypeConfiguration<ChangeFile>
    {
        public ChangeFileMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.LocalFileName)
                .IsRequired()
                .HasMaxLength(512);

            this.Property(t => t.ServerFileName)
                .IsRequired()
                .HasMaxLength(512);

            // Table & Column Mappings
            this.ToTable("ChangeFile");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.ChangeListId).HasColumnName("ChangeListId");
            this.Property(t => t.LocalFileName).HasColumnName("LocalFileName");
            this.Property(t => t.ServerFileName).HasColumnName("ServerFileName");
            this.Property(t => t.IsActive).HasColumnName("IsActive");
            this.Property(t => t.ReviewRevision).HasColumnName("ReviewRevision");

            // Relationships
            this.HasRequired(t => t.ChangeList)
                .WithMany(t => t.ChangeFiles)
                .HasForeignKey(d => d.ChangeListId);

        }
    }
}
