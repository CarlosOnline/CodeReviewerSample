using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace CodeReviewer.Models.Mapping
{
    public class MailReviewMap : EntityTypeConfiguration<MailReview>
    {
        public MailReviewMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("MailReview");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.ReviewId).HasColumnName("ReviewId");

            // Relationships
            this.HasRequired(t => t.Review)
                .WithMany(t => t.MailReviews)
                .HasForeignKey(d => d.ReviewId);

        }
    }
}
