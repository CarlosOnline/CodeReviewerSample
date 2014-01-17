using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace CodeReviewer.Models.Mapping
{
    public class AttachmentMap : EntityTypeConfiguration<Attachment>
    {
        public AttachmentMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Description)
                .HasMaxLength(128);

            this.Property(t => t.Link)
                .IsRequired();

            // Table & Column Mappings
            this.ToTable("Attachment");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.ChangeListId).HasColumnName("ChangeListId");
            this.Property(t => t.TimeStamp).HasColumnName("TimeStamp");
            this.Property(t => t.Description).HasColumnName("Description");
            this.Property(t => t.Link).HasColumnName("Link");

            // Relationships
            this.HasRequired(t => t.ChangeList)
                .WithMany(t => t.Attachments)
                .HasForeignKey(d => d.ChangeListId);

        }
    }
}
