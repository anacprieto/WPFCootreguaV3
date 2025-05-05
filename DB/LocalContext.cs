using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace DB
{

    public class LocalContext : DbContext
    {
        //public LocalContext( DbContextOptions<LocalContext> options) : base(options) { }
        public DbSet<DB_Transaction> DB_Transactions { get; set; }
        public DbSet<DB_TransactionDetail> DB_TransactionDetails {  get; set; }

        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(connectionString: "Filename=" + "Local.db",
                sqliteOptionsAction: op =>
                {
                    op.MigrationsAssembly(
                        Assembly.GetExecutingAssembly().FullName
                        );
                });
            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DB_Transaction>().ToTable("Transaction");
            modelBuilder.Entity<DB_Transaction>(ent =>
            {
                ent.HasKey(e => e.TransactionId);
                ent.HasIndex(e => e.IdApi).IsUnique();
            });
            

            modelBuilder.Entity<DB_TransactionDetail>().ToTable("TransactionDetail");
            modelBuilder.Entity<DB_TransactionDetail>(ent =>
            {
                ent.HasKey(e => e.TranDetailId);
                ent.HasIndex(e => e.IdApi).IsUnique();
            });
            base.OnModelCreating(modelBuilder);
        }
    }
}