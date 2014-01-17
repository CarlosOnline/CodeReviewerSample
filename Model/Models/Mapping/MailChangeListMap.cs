using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace CodeReviewer.Models.Mapping
{
    public class MailChangeListMap : EntityTypeConfiguration<MailChangeList>
    {
        public MailChangeListMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("MailChangeList");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.ReviewerId).HasColumnName("ReviewerId");
            this.Property(t => t.ChangeListId).HasColumnName("ChangeListId");
            this.Property(t => t.RequestType).HasColumnName("RequestType");

            // Relationships
            this.HasRequired(t => t.ChangeList)
                .WithMany(t => t.MailChangeLists)
                .HasForeignKey(d => d.ChangeListId);
            this.HasRequired(t => t.Reviewer)
                .WithMany(t => t.MailChangeLists)
                .HasForeignKey(d => d.ReviewerId);

        }
    }
}
