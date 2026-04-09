namespace SynapseSRE.Api.Models;

public class ResolveIncidentRequest
{
    public string ResolvedBy       { get; set; } = "SRE Team";
    public string ResolutionNotes  { get; set; } = string.Empty;
    public string? ReporterEmail   { get; set; }
}