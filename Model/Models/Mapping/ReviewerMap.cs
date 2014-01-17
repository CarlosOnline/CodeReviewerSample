using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace CodeReviewer.Models.Mapping
{
    public class ReviewerMap : EntityTypeConfiguration<Reviewer>
    {
        public ReviewerMap()
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

            // Table & Column Mappings
            this.ToTable("Reviewer");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.UserName).HasColumnName("UserName");
            this.Property(t => t.ReviewerAlias).HasColumnName("ReviewerAlias");
            this.Property(t => t.ChangeListId).HasColumnName("ChangeListId");
            this.Property(t => t.Status).HasColumnName("Status");

            // Relationships
            this.HasRequired(t => t.ChangeList)
                .WithMany(t => t.Reviewers)
                .HasForeignKey(d => d.ChangeListId);

        }
    }
}
