using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace CodeReviewer.Models.Mapping
{
    public class FileVersionMap : EntityTypeConfiguration<FileVersion>
    {
        public FileVersionMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("FileVersion");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.FileId).HasColumnName("FileId");
            this.Property(t => t.Revision).HasColumnName("Revision");
            this.Property(t => t.ReviewRevision).HasColumnName("ReviewRevision");
            this.Property(t => t.Action).HasColumnName("Action");
            this.Property(t => t.TimeStamp).HasColumnName("TimeStamp");
            this.Property(t => t.IsText).HasColumnName("IsText");
            this.Property(t => t.IsFullText).HasColumnName("IsFullText");
            this.Property(t => t.IsRevisionBase).HasColumnName("IsRevisionBase");
            this.Property(t => t.Text).HasColumnName("Text");

            // Relationships
            this.HasRequired(t => t.ChangeFile)
                .WithMany(t => t.FileVersions)
                .HasForeignKey(d => d.FileId);

        }
    }
}
