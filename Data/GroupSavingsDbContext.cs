using Microsoft.EntityFrameworkCore;
using GroupSavingsApi.Models;

namespace GroupSavingsApi.Data
{
    public class GroupSavingsDbContext : DbContext
    {
        public GroupSavingsDbContext(DbContextOptions<GroupSavingsDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<AccountType> AccountTypes { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupRole> GroupRoles { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<GroupSession> GroupSessions { get; set; }
        public DbSet<Contribution> Contributions { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configurations
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.PasswordHash).IsRequired();
            });

            // Member configurations
            modelBuilder.Entity<Member>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                      .WithOne(u => u.Member)
                      .HasForeignKey<Member>(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // PaymentMethod configurations
            modelBuilder.Entity<PaymentMethod>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Member)
                      .WithMany(m => m.PaymentMethods)
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.AccountType)
                      .WithMany(at => at.PaymentMethods)
                      .HasForeignKey(e => e.AccountTypeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Group configurations
            modelBuilder.Entity<Group>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
            });

            // GroupMember configurations
            modelBuilder.Entity<GroupMember>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Group)
                      .WithMany(g => g.Members)
                      .HasForeignKey(e => e.GroupId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.User)
                      .WithMany(u => u.GroupMemberships)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Role)
                      .WithMany(r => r.GroupMembers)
                      .HasForeignKey(e => e.RoleId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // GroupSession configurations
            modelBuilder.Entity<GroupSession>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Group)
                      .WithMany(g => g.Sessions)
                      .HasForeignKey(e => e.GroupId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Contribution configurations
            modelBuilder.Entity<Contribution>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.GroupSession)
                      .WithMany(gs => gs.Contributions)
                      .HasForeignKey(e => e.GroupSessionId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Contributions)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Notification configurations
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Notifications)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed data
            modelBuilder.Entity<AccountType>().HasData(
                new AccountType { Id = 1, Name = "Bank Account", Description = "Traditional bank account" },
                new AccountType { Id = 2, Name = "Mobile Money", Description = "Mobile money account" },
                new AccountType { Id = 3, Name = "Credit Card", Description = "Credit card account" }
            );

            modelBuilder.Entity<GroupRole>().HasData(
                new GroupRole { Id = 1, Name = "Admin", Description = "Group administrator with full permissions" },
                new GroupRole { Id = 2, Name = "Member", Description = "Regular group member" },
                new GroupRole { Id = 3, Name = "Moderator", Description = "Group moderator with limited admin permissions" }
            );
        }
    }
}
