using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace CodeReviewer.Models.Mapping
{
    public class MailReviewRequestMap : EntityTypeConfiguration<MailReviewRequest>
    {
        public MailReviewRequestMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.ReviewerAlias)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("MailReviewRequest");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.ReviewerAlias).HasColumnName("ReviewerAlias");
            this.Property(t => t.ChangeListId).HasColumnName("ChangeListId");

            // Relationships
            this.HasRequired(t => t.ChangeList)
                .WithMany(t => t.MailReviewRequests)
                .HasForeignKey(d => d.ChangeListId);

        }
    }
}
