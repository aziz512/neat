using System.Data.Entity;

namespace News.Models
{
    public class DatabaseContext : DbContext
    {
        public virtual DbSet<Article> Articles { get; set; }
        public virtual DbSet<Comment> Comments { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<LogEntry> Logs { get; set; }
        public virtual DbSet<AdditionalField> AdditionalFields { get; set; }
        public virtual DbSet<FieldValue> FieldValues { get; set; }
    }
}