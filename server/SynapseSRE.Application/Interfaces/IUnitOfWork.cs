
using SynapseSRE.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IIncidentRepository Incidents { get; }
    IActivityLogRepository Logs { get; }
    ITriageAnalysisRepository TriageAnalyses { get; } 
    IAttachmentRepository Attachments { get; }   
    
    Task<int> SaveAsync();
}