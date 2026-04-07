using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SynapseSRE.Domain.Entities;

public class IncidentAttachmentConfiguration : IEntityTypeConfiguration<IncidentAttachment>
{
    public void Configure(EntityTypeBuilder<IncidentAttachment> builder)
    {
        builder.ToTable("incident_attachments");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");

        builder.Property(a => a.FilePath).IsRequired();

        builder.HasOne(a => a.Incident)
               .WithMany(i => i.Attachments)
               .HasForeignKey(a => a.IncidentId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}