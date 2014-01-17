using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace CodeReviewer.Models.Mapping
{
    public class ReviewMap : EntityTypeConfiguration<Review>
    {
        public ReviewMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.UserName)
                .IsRequired()
                .HasMaxLength(200);

            this.Property(t => t.ReviewerAlias)
                .IsRequired()
                .HasMaxLength(200);

            this.Property(t => t.CommentText)
                .HasMaxLength(2048);

            // Table & Column Mappings
            this.ToTable("Review");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.ChangeListId).HasColumnName("ChangeListId");
            this.Property(t => t.UserName).HasColumnName("UserName");
            this.Property(t => t.ReviewerAlias).HasColumnName("ReviewerAlias");
            this.Property(t => t.TimeStamp).HasColumnName("TimeStamp");
            this.Property(t => t.IsSubmitted).HasColumnName("IsSubmitted");
            this.Property(t => t.OverallStatus).HasColumnName("OverallStatus");
            this.Property(t => t.CommentText).HasColumnName("CommentText");

            // Relationships
            this.HasRequired(t => t.ChangeList)
                .WithMany(t => t.Reviews)
                .HasForeignKey(d => d.ChangeListId);

        }
    }
}
