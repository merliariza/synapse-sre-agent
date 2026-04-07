using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SynapseSRE.Domain.Entities;

public class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.ToTable("incidents");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");

        builder.Property(i => i.Title).IsRequired().HasMaxLength(255);
        builder.Property(i => i.Status).HasDefaultValue("Pending");

        builder.HasOne(i => i.User)
               .WithMany(u => u.Incidents)
               .HasForeignKey(i => i.UserId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}