using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace CodeReviewer.Models.Mapping
{
    public class UserContextMap : EntityTypeConfiguration<UserContext>
    {
        public UserContextMap()
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

            this.Property(t => t.KeyName)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("UserContext");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.UserName).HasColumnName("UserName");
            this.Property(t => t.ReviewerAlias).HasColumnName("ReviewerAlias");
            this.Property(t => t.KeyName).HasColumnName("KeyName");
            this.Property(t => t.Value).HasColumnName("Value");
        }
    }
}
