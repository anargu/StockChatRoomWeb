using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StockChatRoomWeb.Core.Entities;

namespace StockChatRoomWeb.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<ChatRoom> ChatRooms => Set<ChatRoom>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // User entity
        builder.Entity<User>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            
            // Indexes
            entity.HasIndex(e => e.UserName).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // ChatMessage entity
        builder.Entity<ChatMessage>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Content).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

            // Relationships
            entity.HasOne(e => e.User)
                .WithMany(u => u.ChatMessages)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.CreatedAt).IsDescending();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.MessageType);
        });

        builder.Entity<ChatRoom>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

            // Relationships
            entity.HasOne(e => e.User)
                .WithMany(u => u.ChatRooms)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.ChatMessages)
                .WithOne(m => m.ChatRoom)
                .HasForeignKey(m => m.ChatRoomId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

            // Indexes
            entity.HasIndex(e => e.CreatedAt).IsDescending();
            entity.HasIndex(e => e.UserId);
        });

        // Identity tables
        builder.Entity<IdentityRole<Guid>>().ToTable("AspNetRoles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("AspNetUserRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("AspNetUserClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("AspNetUserLogins");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("AspNetRoleClaims");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("AspNetUserTokens");
    }
}