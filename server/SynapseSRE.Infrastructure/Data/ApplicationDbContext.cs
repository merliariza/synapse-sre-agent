using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SynapseSRE.Domain.Entities;

namespace SynapseSRE.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<TriageAnalysis> TriageAnalyses => Set<TriageAnalysis>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<IncidentAttachment> IncidentAttachments => Set<IncidentAttachment>();
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.HasPostgresExtension("pgcrypto"); 

    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
}
}