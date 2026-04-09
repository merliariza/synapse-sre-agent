using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SynapseSRE.Domain.Entities;

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.ToTable("activity_logs");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");

        builder.Property(a => a.Details)
               .HasColumnType("text"); 

        builder.Property(a => a.Action).IsRequired();
    }
}