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
        public DbSet<GroupInvitation> GroupInvitations { get; set; }
        public DbSet<SavingsGoal> SavingsGoals { get; set; }
// SavingsGoal-GroupSession relation
// modelBuilder.Entity<SavingsGoal>().HasOne(sg => sg.GroupSession).WithMany(gs => gs.SavingsGoals).HasForeignKey(sg => sg.GroupSessionId);
        public DbSet<AuditLog> AuditLogs { get; set; }

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
                entity.HasOne(e => e.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.CreatedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // GroupRole configurations
            modelBuilder.Entity<GroupRole>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
            });

            // GroupMember configurations
            modelBuilder.Entity<GroupMember>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Group)
                      .WithMany(g => g.GroupMembers)
                      .HasForeignKey(e => e.GroupId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Role)
                      .WithMany()
                      .HasForeignKey(e => e.RoleId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // GroupSession configurations
            modelBuilder.Entity<GroupSession>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Group)
                      .WithMany(g => g.GroupSessions)
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
                entity.HasOne(e => e.Member)
                      .WithMany(m => m.Contributions)
                      .HasForeignKey(e => e.MemberId)
                      .OnDelete(DeleteBehavior.Cascade);
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

            // GroupInvitation configurations
            modelBuilder.Entity<GroupInvitation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Group)
                      .WithMany(g => g.GroupInvitations)
                      .HasForeignKey(e => e.GroupId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Inviter)
                      .WithMany()
                      .HasForeignKey(e => e.InviterId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // SavingsGoal configurations
            modelBuilder.Entity<SavingsGoal>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.GroupSession)
                      .WithMany(gs => gs.SavingsGoals)
                      .HasForeignKey(e => e.GroupSessionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // AuditLog configurations
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired();
                entity.Property(e => e.Timestamp).IsRequired();
            });
        }
    }
}

