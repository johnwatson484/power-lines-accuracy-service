using Microsoft.EntityFrameworkCore;
using PowerLinesAccuracyService.Models;

namespace PowerLinesAccuracyService.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }
    public DbSet<Result> Results { get; set; }
    public DbSet<MatchOdds> MatchOdds { get; set; }
    public DbSet<Models.Accuracy> Accuracy { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Result>()
            .HasIndex(x => new { x.Date, x.HomeTeam, x.AwayTeam }).IsUnique();

        modelBuilder.Entity<Models.Accuracy>()
            .HasIndex(x => new { x.Division }).IsUnique();
    }
}
