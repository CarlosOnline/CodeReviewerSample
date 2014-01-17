using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using CodeReviewer.Models.Mapping;

namespace CodeReviewer.Models
{
    public partial class CodeReviewerContext : DbContext
    {
        static CodeReviewerContext()
        {
            Database.SetInitializer<CodeReviewerContext>(null);
        }

        public CodeReviewerContext()
            : base("Name=CodeReviewerContext")
        {
        }

        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<AuditRecord> AuditRecords { get; set; }
        public DbSet<ChangeFile> ChangeFiles { get; set; }
        public DbSet<ChangeList> ChangeLists { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<CommentGroup> CommentGroups { get; set; }
        public DbSet<FileVersion> FileVersions { get; set; }
        public DbSet<MailChangeList> MailChangeLists { get; set; }
        public DbSet<MailReview> MailReviews { get; set; }
        public DbSet<MailReviewRequest> MailReviewRequests { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Reviewer> Reviewers { get; set; }
        public DbSet<SourceControl> SourceControls { get; set; }
        public DbSet<UserContext> UserContexts { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new AttachmentMap());
            modelBuilder.Configurations.Add(new AuditRecordMap());
            modelBuilder.Configurations.Add(new ChangeFileMap());
            modelBuilder.Configurations.Add(new ChangeListMap());
            modelBuilder.Configurations.Add(new CommentMap());
            modelBuilder.Configurations.Add(new CommentGroupMap());
            modelBuilder.Configurations.Add(new FileVersionMap());
            modelBuilder.Configurations.Add(new MailChangeListMap());
            modelBuilder.Configurations.Add(new MailReviewMap());
            modelBuilder.Configurations.Add(new MailReviewRequestMap());
            modelBuilder.Configurations.Add(new ReviewMap());
            modelBuilder.Configurations.Add(new ReviewerMap());
            modelBuilder.Configurations.Add(new SourceControlMap());
            modelBuilder.Configurations.Add(new UserContextMap());
            modelBuilder.Configurations.Add(new UserProfileMap());
        }
    }
}
