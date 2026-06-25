using Microsoft.EntityFrameworkCore;
using TrainineeAPI.Models;

public class AppDbContext : DbContext
{
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }

    public DbSet<SubmissionFile> SubmissionFiles {get; set;} = null!;
    public DbSet<ProcessingJob> ProcessingJobs {get; set;} = null!;
}