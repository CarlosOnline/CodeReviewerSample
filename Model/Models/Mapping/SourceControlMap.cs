using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace CodeReviewer.Models.Mapping
{
    public class SourceControlMap : EntityTypeConfiguration<SourceControl>
    {
        public SourceControlMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Server)
                .HasMaxLength(50);

            this.Property(t => t.Client)
                .HasMaxLength(50);

            this.Property(t => t.Description)
                .HasMaxLength(256);

            this.Property(t => t.WebsiteName)
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("SourceControl");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.Type).HasColumnName("Type");
            this.Property(t => t.Server).HasColumnName("Server");
            this.Property(t => t.Client).HasColumnName("Client");
            this.Property(t => t.Description).HasColumnName("Description");
            this.Property(t => t.WebsiteName).HasColumnName("WebsiteName");
        }
    }
}
