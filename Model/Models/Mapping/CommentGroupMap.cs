using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace CodeReviewer.Models.Mapping
{
    public class CommentGroupMap : EntityTypeConfiguration<CommentGroup>
    {
        public CommentGroupMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.LineStamp)
                .IsRequired()
                .HasMaxLength(200);

            // Table & Column Mappings
            this.ToTable("CommentGroup");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.ReviewId).HasColumnName("ReviewId");
            this.Property(t => t.ChangeListId).HasColumnName("ChangeListId");
            this.Property(t => t.FileId).HasColumnName("FileId");
            this.Property(t => t.FileVersionId).HasColumnName("FileVersionId");
            this.Property(t => t.Line).HasColumnName("Line");
            this.Property(t => t.LineStamp).HasColumnName("LineStamp");
            this.Property(t => t.Status).HasColumnName("Status");

            // Relationships
            this.HasRequired(t => t.ChangeFile)
                .WithMany(t => t.CommentGroups)
                .HasForeignKey(d => d.FileId);
            this.HasRequired(t => t.FileVersion)
                .WithMany(t => t.CommentGroups)
                .HasForeignKey(d => d.FileVersionId);
            this.HasRequired(t => t.Review)
                .WithMany(t => t.CommentGroups)
                .HasForeignKey(d => d.ReviewId);

        }
    }
}
