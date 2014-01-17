using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace CodeReviewer.Models.Mapping
{
    public class CommentMap : EntityTypeConfiguration<Comment>
    {
        public CommentMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.CommentText)
                .IsRequired()
                .HasMaxLength(2048);

            this.Property(t => t.UserName)
                .IsRequired()
                .HasMaxLength(200);

            this.Property(t => t.ReviewerAlias)
                .IsRequired()
                .HasMaxLength(200);

            // Table & Column Mappings
            this.ToTable("Comment");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.CommentText).HasColumnName("CommentText");
            this.Property(t => t.FileVersionId).HasColumnName("FileVersionId");
            this.Property(t => t.ReviewerId).HasColumnName("ReviewerId");
            this.Property(t => t.ReviewRevision).HasColumnName("ReviewRevision");
            this.Property(t => t.GroupId).HasColumnName("GroupId");
            this.Property(t => t.UserName).HasColumnName("UserName");
            this.Property(t => t.ReviewerAlias).HasColumnName("ReviewerAlias");

            // Relationships
            this.HasOptional(t => t.CommentGroup)
                .WithMany(t => t.Comments)
                .HasForeignKey(d => d.GroupId);
            this.HasRequired(t => t.FileVersion)
                .WithMany(t => t.Comments)
                .HasForeignKey(d => d.FileVersionId);
            this.HasRequired(t => t.Reviewer)
                .WithMany(t => t.Comments)
                .HasForeignKey(d => d.ReviewerId);

        }
    }
}
