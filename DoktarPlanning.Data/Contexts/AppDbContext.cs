using DoktarPlanning.Domain.Entities;

using Humanizer;

using Microsoft.EntityFrameworkCore;

namespace DoktarPlanning.Data.Contexts
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<TaskItem> TaskItems { get; set; } = null!;
        public DbSet<RecurrenceRule> RecurrenceRules { get; set; } = null!;
        public DbSet<SubTask> SubTasks { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (entityType.IsOwned()) continue;

                var clrName = entityType.ClrType.Name;
                var tableName = clrName.Pluralize();
                entityType.SetTableName(tableName);
            }

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Email).IsRequired().HasMaxLength(256);
                b.Property(x => x.DisplayName).HasMaxLength(200);
                b.Property(x => x.PasswordHash).IsRequired();
                b.Property(x => x.IsActive).HasDefaultValue(true);
                b.HasMany(x => x.Tasks)
                 .WithOne(t => t.User)
                 .HasForeignKey(t => t.UserId)
                 .OnDelete(DeleteBehavior.NoAction);

                b.HasMany(x => x.RecurrenceRules)
                 .WithOne(r => r.User)
                 .HasForeignKey(r => r.UserId)
                 .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<TaskItem>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Title).IsRequired().HasMaxLength(250);
                b.Property(x => x.Description).HasMaxLength(5000);
                b.Property(x => x.Notes).HasMaxLength(5000);
                b.Property(x => x.Priority).HasConversion<int>().IsRequired();
                b.Property(x => x.IsCompleted).HasDefaultValue(false);
                b.Property(x => x.IsRecurring).HasDefaultValue(false);

                b.HasOne(x => x.RecurrenceRule)
                 .WithMany()
                 .HasForeignKey(x => x.RecurrenceRuleId)
                 .OnDelete(DeleteBehavior.NoAction);

                b.HasMany(x => x.SubTasks)
                 .WithOne(s => s.TaskItem)
                 .HasForeignKey(s => s.TaskItemId)
                 .OnDelete(DeleteBehavior.NoAction);

                b.HasIndex(x => new { x.UserId, x.DueAt });
            });

            modelBuilder.Entity<RecurrenceRule>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Frequency).HasConversion<int>().IsRequired();
                b.Property(x => x.Interval).HasDefaultValue(1);
                b.HasIndex(x => new { x.UserId });
            });

            modelBuilder.Entity<SubTask>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Title).IsRequired().HasMaxLength(250);
                b.Property(x => x.IsCompleted).HasDefaultValue(false);
                b.HasIndex(x => x.TaskItemId);
            });
        }
    }
}