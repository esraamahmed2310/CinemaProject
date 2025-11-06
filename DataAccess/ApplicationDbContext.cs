using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using CinemaProject.ViewModels;


namespace CinemaProject.DataAccess
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Cinema> Cinemas { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<MovieActor> MovieActors { get; set; }
        public DbSet<MovieSubImage> MovieSubImages { get; set; }
        public DbSet<ApplicationUserOTP> ApplicationUserOTPs { get; set; }

        // Depracted
        public ApplicationDbContext()
        {
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    base.OnConfiguring(optionsBuilder);
        //    optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=CinemaDb;Integrated Security=True;" +
        //        "Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;" +
        //        "Application Intent=ReadWrite;Multi Subnet Failover=False");

        //}
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MovieActor>(entity =>
            {
                entity.ToTable("MovieActors");

                // المفتاح الأساسي المركب
                entity.HasKey(ma => new { ma.MovieId, ma.ActorId });

                // العلاقات
                entity.HasOne(ma => ma.Movie)
                      .WithMany(m => m.MovieActors)
                      .HasForeignKey(ma => ma.MovieId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ma => ma.Actor)
                      .WithMany(a => a.MovieActors)
                      .HasForeignKey(ma => ma.ActorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
        public DbSet<CinemaProject.ViewModels.ValidateOTP> ValidateOTP { get; set; } = default!;
        public DbSet<CinemaProject.ViewModels.NewPasswordVM> NewPasswordVM { get; set; } = default!;
        //public DbSet<CinemaProject.ViewModels.RegisterVM> RegisterVM { get; set; } = default!;
        //public DbSet<CinemaProject.ViewModels.LoginVM> LoginVM { get; set; } = default!;
        //public DbSet<CinemaProject.ViewModels.ResendEmailConfirmationVM> ResendEmailConfirmationVM { get; set; } = default!;
        //public DbSet<CinemaProject.ViewModels.ForgetPasswordVM> ForgetPasswordVM { get; set; } = default!;

    }
}
