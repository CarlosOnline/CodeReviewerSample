using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace CodeReviewer.Models.Mapping
{
    public class ChangeListMap : EntityTypeConfiguration<ChangeList>
    {
        public ChangeListMap()
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

            this.Property(t => t.UserClient)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.CL)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.Url)
                .IsRequired()
                .HasMaxLength(2048);

            this.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.Description)
                .IsRequired();

            // Table & Column Mappings
            this.ToTable("ChangeList");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.SourceControlId).HasColumnName("SourceControlId");
            this.Property(t => t.UserName).HasColumnName("UserName");
            this.Property(t => t.ReviewerAlias).HasColumnName("ReviewerAlias");
            this.Property(t => t.UserClient).HasColumnName("UserClient");
            this.Property(t => t.CL).HasColumnName("CL");
            this.Property(t => t.Url).HasColumnName("Url");
            this.Property(t => t.Title).HasColumnName("Title");
            this.Property(t => t.Description).HasColumnName("Description");
            this.Property(t => t.TimeStamp).HasColumnName("TimeStamp");
            this.Property(t => t.Stage).HasColumnName("Stage");
            this.Property(t => t.CurrentReviewRevision).HasColumnName("CurrentReviewRevision");

            // Relationships
            this.HasRequired(t => t.SourceControl)
                .WithMany(t => t.ChangeLists)
                .HasForeignKey(d => d.SourceControlId);

        }
    }
}
