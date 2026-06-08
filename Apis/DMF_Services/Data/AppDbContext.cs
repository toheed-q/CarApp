using DMF_Services.DTOs.Cars;
using DMF_Services.Models;
using Microsoft.EntityFrameworkCore;

namespace DMF_Services.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<UserDetail> UserDetails => Set<UserDetail>();
        public DbSet<CarDetail> CarDetails => Set<CarDetail>();
        public DbSet<CarImage> CarImages => Set<CarImage>();
        public DbSet<UserOtp> UserOtps { get; set; } = null!;
        public DbSet<CityLocation> CityLocations => Set<CityLocation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserDetail>(entity =>
            {
                entity.ToTable("UserDetail");

                entity.HasKey(e => e.ID);

                // ---------- Personal Info ----------
                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.MidleName)
                    .HasMaxLength(100);

                entity.Property(e => e.LastName)
                    .HasMaxLength(100);

                // ---------- Company ----------
                entity.Property(e => e.CompanyName)
                    .HasMaxLength(500)
                    .IsUnicode(true);

                // ---------- Contact ----------
                entity.Property(e => e.PrimaryMobile)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.SecondaryMobile)
                    .HasMaxLength(20);

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .IsUnicode(true);

                // ---------- Address ----------
                entity.Property(e => e.Address1)
                    .HasMaxLength(100);

                entity.Property(e => e.Address2)
                    .HasMaxLength(100);

                entity.Property(e => e.City)
                    .HasMaxLength(100);

                entity.Property(e => e.District)
                    .HasMaxLength(100);

                entity.Property(e => e.State)
                    .HasMaxLength(100);

                entity.Property(e => e.Pincode)
                    .HasMaxLength(20);

                // ---------- Profile ----------
                entity.Property(e => e.ProfileImage)
                    .IsRequired()
                    .HasMaxLength(255);

                // ---------- Role ----------
                entity.Property(e => e.IsDealers)
                    .IsRequired();

                // ---------- Services (IMPORTANT) ----------
                entity.Property(e => e.LoanService)
                    .HasColumnName("Loan_Service");

                entity.Property(e => e.RegistrationService)
                    .HasColumnName("Registration_Service");

                entity.Property(e => e.NocService)
                    .HasColumnName("NOC_Service");

                entity.Property(e => e.IsActive);

                // ---------- Auth ----------
                entity.Property(e => e.PasswordHash)
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CarDetail>(entity =>
            {
                entity.ToTable("CarDetail");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.CarLocation)
                      .HasColumnType("geography");

                entity.Property(x => x.CreatedDate)
                      .HasDefaultValueSql("GETDATE()");

                entity.HasOne(x => x.CarImage)
                      .WithOne(x => x.CarDetail)
                      .HasForeignKey<CarImage>(x => x.CarDetailID);
            });

            modelBuilder.Entity<CarImage>(entity =>
            {
                entity.ToTable("CarImage");
                entity.HasKey(x => x.Id);
            });

            modelBuilder.Entity<UserOtp>(entity =>
            {
                entity.ToTable("UserOtp");

                entity.HasKey(e => e.ID);

                entity.Property(e => e.Mobile)
                      .IsRequired()
                      .HasMaxLength(20);

                entity.Property(e => e.OtpCode)
                      .IsRequired()
                      .HasMaxLength(4);

                entity.Property(e => e.IsUsed)
                      .IsRequired();

                entity.Property(e => e.CreatedOn)
                      .HasDefaultValueSql("GETDATE()");
            });


            modelBuilder.Entity<CityLocation>(entity =>
            {
                entity.ToTable("CityLocations");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CityName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            modelBuilder.Entity<CarFilterRaw>().HasNoKey();
            modelBuilder.Entity<CarBrandRaw>().HasNoKey();
            modelBuilder.Entity<CarModelRaw>().HasNoKey();
            modelBuilder.Entity<WishlistToggleResultDto>().HasNoKey();
        }
    }
}
