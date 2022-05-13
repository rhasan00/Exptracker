using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ExpenceTracker.Models
{
    public class DBContext:DbContext
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Accountscatagory> Accountscatagories { get; set; }
        public DbSet<Accountshead> Accountsheads { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Accountshead>(entity => {
                entity.HasIndex(e => e.expence).IsUnique();
            });
            builder.Entity<Transaction>().HasOne(p => p.exp).WithMany(w => w.trans)
           .OnDelete(DeleteBehavior.ClientCascade);
        }

    }
}
