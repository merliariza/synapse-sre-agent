namespace SynapseSRE.Application.DTOs;

public class IncidentAttachmentDto
{
    public Guid Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}