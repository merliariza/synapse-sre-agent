using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SynapseSRE.Domain.Entities;

public class TriageAnalysisConfiguration : IEntityTypeConfiguration<TriageAnalysis>
{
    public void Configure(EntityTypeBuilder<TriageAnalysis> builder)
    {
        builder.ToTable("triage_analyses");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");

        builder.HasOne(t => t.Incident)
               .WithOne(i => i.TriageAnalysis)
               .HasForeignKey<TriageAnalysis>(t => t.IncidentId);
    }
}