namespace SynapseSRE.Domain.Entities;

public class IncidentAttachment
{
    public Guid Id { get; set; }
    public Guid IncidentId { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty; 
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public Incident Incident { get; set; } = null!;
}