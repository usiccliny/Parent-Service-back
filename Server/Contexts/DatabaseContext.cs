using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Contexts
{
    public partial class DatabaseContext : DbContext
    {
        public DatabaseContext()
        {
        }

        public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
        {
        }

        public virtual DbSet<Device> Devices { get; set; }

        public virtual DbSet<User> Users { get; set; }

        public virtual DbSet<Link> Links { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Device>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("devices_pk");

                entity.ToTable("devices");

                entity.HasIndex(e => e.Name, "devices_un").IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name");
                entity.Property(e => e.LastOnline).HasColumnName("last_online");
                entity.Property(e => e.HourLimit)
                    .HasColumnName("hour_limit");
                entity.Property(e => e.MinuteLimit).HasColumnName("minute_limit");
                entity.Property(e => e.DaysOfWeek).HasColumnName("days_of_week");
                entity.Property(e => e.TurnOff)
                    .HasColumnName("turn_off")
                    .HasDefaultValue(false);
                entity.Property(e => e.HourUsed)
                    .HasColumnName("hour_used")
                    .HasDefaultValue(0);
                entity.Property(e => e.MinuteUsed)
                    .HasColumnName("minute_used")
                    .HasDefaultValue(0);
                entity.Property(e => e.second_used)
                    .HasColumnName("second_used")
                    .HasDefaultValue(0);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.TGID).HasName("users_pk");

                entity.ToTable("users");

                entity.HasIndex(e => e.Token, "users_un").IsUnique();

                entity.Property(e => e.Token).HasColumnName("token");
                entity.Property(e => e.PrimeStatus)
                .HasColumnName("prime_status")
                .HasDefaultValue(0);
                entity.Property(e => e.TGID).HasColumnName("tg_id");
            });

            modelBuilder.Entity<Link>(entity =>
            {
                entity.ToTable("links");

                entity.HasKey(e => new { e.DeviceID, e.UserId });

                entity.Property(e => e.DeviceID).HasColumnName("device_id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
